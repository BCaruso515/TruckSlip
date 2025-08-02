using Firebase.Database;
using Firebase.Database.Query;
using System.Reflection;

namespace TruckSlip.Data
{
    public class FirebaseDataService : IDataService
    {
        public string DataLocation { get; } = "Firebase";

        private readonly FirebaseClient _firebaseClient;
        public FirebaseClient FirebaseClient => _firebaseClient;

        public FirebaseDataService(FirebaseClient firebaseClient)
        {
            _firebaseClient = firebaseClient;
        }

        public async Task InitializeDatabaseAsync()
            => await CreateTablesAsync();

        public async Task CreateTablesAsync()
        {
            // Create tables if they do not exist
            var tasks = new List<Task>
            {
                CreateTableAsync("UnitType"),
                CreateTableAsync("Product"),
                CreateTableAsync("Order"),
                CreateTableAsync("OrderItem"),
                CreateTableAsync("Jobsite")
            };
            await Task.WhenAll(tasks);
        }

        public async Task DropTablesAsync()
        {
            var tasks = new List<Task>
            {
                DeleteTableAsync("UnitType"),
                DeleteTableAsync("Product"),
                CreateTableAsync("Order"),
                CreateTableAsync("OrderItem"),
                CreateTableAsync("Jobsite"),
                DeleteTableAsync("Tables")
            };
            await Task.WhenAll(tasks);
        }

        public async Task CloseDatabaseAsync()
        {
            await Task.Delay(100); // Simulate closing the database connection
        }

        private async Task CreateTableAsync(string tableName)
        {
            var items = await _firebaseClient
                .Child("Tables")
                .OnceAsync<Tables>();
            var item = items.FirstOrDefault(x => x.Object.TableName == tableName);

            if (item == null)
                await _firebaseClient.Child("Tables").PostAsync(new Tables { NextId = 1, TableName = tableName });
        }

        private async Task DeleteTableAsync(string tableName)
        {
            await _firebaseClient
                .Child(tableName)
                .DeleteAsync();
        }

        // GENERIC: Get all items
        public async Task<ObservableCollection<T>> GetAllAsync<T>(string tableName) where T : class, new()
        {
            var collection = new ObservableCollection<T>();
            var items = await _firebaseClient
                .Child(tableName)
                .OnceAsync<T>();
            foreach (var item in items)
            {
                if (item.Object != null)
                    collection.Add(item.Object);
            }
            return collection;
        }

        // GENERIC: Add or update item
        public async Task<bool> AddOrUpdateAsync<T>(T entity, string tableName, string keyProperty) where T : class, new()
        {
            var prop = typeof(T).GetProperty(keyProperty, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                throw new InvalidOperationException($"Property '{keyProperty}' not found on type '{typeof(T).Name}'.");

            int id = (int)(prop.GetValue(entity) ?? 0);

            if (id == 0)
            {
                // If ID is 0, we assume it's a new entity and should be added
                int nextId = await GetNextIdAsync(tableName);
                prop.SetValue(entity, nextId);

                return await AddAsync(entity, tableName);
            }
                
            else
                return await UpdateAsync(entity, tableName, keyProperty, prop, id);
        }

        // GENERIC: Add item
        private async Task<bool> AddAsync<T>(T entity, string tableName) where T : class, new()
        {
            var result = await _firebaseClient
                .Child(tableName)
                .PostAsync(entity);
            return result.Key != null;
        }

        // GENERIC: Update item
        private async Task<bool> UpdateAsync<T>(T entity, string tableName, string keyProperty, PropertyInfo prop, int id) where T : class, new()
        {
            var items = await _firebaseClient
                .Child(tableName)
                .OnceAsync<T>();

            var itemToUpdate = items.FirstOrDefault(x =>
            {
                var value = prop.GetValue(x.Object);
                return value != null && (int)value == id;
            });

            if (itemToUpdate == null) return false;

            await _firebaseClient
                .Child(tableName)
                .Child(itemToUpdate.Key)
                .PutAsync(entity);

            return true;
        }

        // GENERIC: Delete item
        public async Task<bool> DeleteAsync<T>(T entity, string tableName, string keyProperty) where T : class, new()
        {
            var prop = typeof(T).GetProperty(keyProperty, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                throw new InvalidOperationException($"Property '{keyProperty}' not found on type '{typeof(T).Name}'.");

            int id = (int)(prop.GetValue(entity) ?? 0);

            var items = await _firebaseClient
                .Child(tableName)
                .OnceAsync<T>();

            var itemToDelete = items.FirstOrDefault(x =>
            {
                var value = prop.GetValue(x.Object);
                return value != null && (int)value == id;
            });

            if (itemToDelete != null)
            {
                await _firebaseClient
                    .Child(tableName)
                    .Child(itemToDelete.Key)
                    .DeleteAsync();
                return true;
            }
            return false;
        }

        // GENERIC: Get next ID
        private async Task<int> GetNextIdAsync(string tableName)
        {
            var items = await _firebaseClient
                .Child("Tables")
                .OnceAsync<Tables>();

            var item = items.FirstOrDefault(x => x.Object.TableName == tableName) ?? throw new Exception($"Table '{tableName}' does not exist.");

            var nextId = item.Object.NextId;
            item.Object.NextId++;
            await _firebaseClient
                .Child("Tables")
                .Child(item.Key)
                .PutAsync(item.Object);
            return nextId;
        }

        // GENERIC: Set next ID
        public async Task SetNextId(string tableName, int id)
        {
            var items = await _firebaseClient
                 .Child("Tables")
                 .OnceAsync<Tables>();
            var item = items.FirstOrDefault(x => x.Object.TableName == tableName);

            if (item != null)
            {
                item.Object.NextId = id;
                await _firebaseClient
                    .Child("Tables")
                    .Child(item.Key)
                    .PutAsync(item.Object);
            }
        }
        
        // Products
        public async Task<ObservableCollection<Product>> GetProductAsync() 
            => await GetAllAsync<Product>("Product");
        public async Task<bool> AddOrUpdateProductAsync(Product product)
            => await AddOrUpdateAsync(product, "Product", nameof(Product.ProductId));
        public async Task<bool> AddProductAsync(Product product)
            => await AddAsync(product, "Product");
        public async Task<bool> DeleteProductAsync(Product product)
        {
            var orderItems = await GetOrderItemAsync();
            var orderItemDelete = orderItems.Where(x => x.ProductId == product.ProductId).ToList();
            foreach (var orderItem in orderItemDelete)
            {
                await DeleteAsync(orderItem, "OrderItem", nameof(OrderItem.OrderItemId));
            }
            
            return await DeleteAsync(product, "Product", nameof(Product.ProductId));
        } 

        // Unit Types
        public async Task<ObservableCollection<UnitType>> GetUnitTypesAsync()
            => await GetAllAsync<UnitType>("UnitType");
        public async Task<bool> AddOrUpdateUnitTypeAsync(UnitType unitType) 
            => await AddOrUpdateAsync(unitType, "UnitType", nameof(UnitType.UnitId));
        public async Task<bool> AddUnitTypeAsync(UnitType unitType) 
            => await AddAsync(unitType, "UnitType");
        public async Task<bool> DeleteUnitTypeAsync(UnitType unitType)
        {
            var products = await GetProductAsync();
            var productsToDelete = products.Where(x => x.UnitId == unitType.UnitId).ToList();
            foreach (var product in productsToDelete)
            {
                await DeleteProductAsync(product); 
            }
            return await DeleteAsync(unitType, "UnitType", nameof(UnitType.UnitId));
        }

       // Order
        public async Task<ObservableCollection<Order>> GetOrderAsync()
            => await GetAllAsync<Order>("Order");
        public async Task<bool> AddOrUpdateOrderAsync(Order order)
            => await AddOrUpdateAsync(order, "Order", nameof(Order.OrderId));
        public async Task<bool> AddOrderAsync(Order order)
            => await AddAsync(order, "Order");
        public async Task<bool> DeleteOrderAsync(Order order)
        {
            var orderItems = await GetOrderItemAsync();
            var orderItemToDelete = orderItems.Where(x => x.OrderId == order.OrderId).ToList();
            foreach (var orderItem in orderItemToDelete)
            {
                await DeleteAsync(orderItem, "OrderItem", nameof(OrderItem.OrderItemId));
            }
            return await DeleteAsync(order, "Order", nameof(Order.OrderId));
        }

        // OrderItem
        public async Task<ObservableCollection<OrderItem>> GetOrderItemAsync()
            => await GetAllAsync<OrderItem>("OrderItem");
        public async Task<bool> AddOrUpdateOrderItemAsync(OrderItem orderItem)
            => await AddOrUpdateAsync(orderItem, "OrderItem", nameof(OrderItem.OrderItemId));
        public async Task<bool> AddOrderItemAsync(OrderItem orderItem)
            => await AddAsync(orderItem, "OrderItem");
        public async Task<bool> DeleteOrderItemAsync(OrderItem orderItem)
            => await DeleteAsync(orderItem, "OrderItem", nameof(OrderItem.OrderItemId));

        // Jobsite
        public async Task<ObservableCollection<Jobsite>> GetJobsiteAsync()
            => await GetAllAsync<Jobsite>("Jobsite");
        public async Task<bool> AddOrUpdateJobsiteAsync(Jobsite jobsite)
            => await AddOrUpdateAsync(jobsite, "Jobsite", nameof(Jobsite.JobsiteId));
        public async Task<bool> AddJobsiteAsync(Jobsite jobsite)
            => await AddAsync(jobsite, "Jobsite");
        public async Task<bool> DeleteJobsiteAsync(Jobsite jobsite)
            => await DeleteAsync(jobsite, "Jobsite", nameof(Jobsite.JobsiteId));

        // OrderItemsQuery
        public async Task<ObservableCollection<OrderItemsQuery>> GetOrderItemsQueryAsync()
        {
            // Fetch all data
            var products = await GetProductAsync();
            var orders = await GetOrderAsync();
            var orderItems = await GetOrderItemAsync();
            var unitTypes = await GetUnitTypesAsync();

            var query = from orderItem in orderItems
                        join order in orders on orderItem.OrderId equals order.OrderId
                        join product in products on orderItem.ProductId  equals product.ProductId
                        join unit in unitTypes on product.UnitId equals unit.UnitId
                        select new OrderItemsQuery
                           {
                                 OrderItemId = orderItem.OrderItemId,
                                 OrderId = order.OrderId,
                                 ProductId = product.ProductId,
                                 Name = product.Name,
                                 TaskCode = product.TaskCode,
                                 UnitName = unit.UnitName,
                                 Quantity = orderItem.Quantity
                        };

            return new ObservableCollection<OrderItemsQuery>(query);
        }
    }
}
