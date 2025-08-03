using SQLite;

namespace TruckSlip.Data
{
    public class SQLiteDataService : IDataService
    {
        private readonly SQLiteAsyncConnection _connection;
        public string DataLocation { get; } = Path.Combine(FileSystem.Current.AppDataDirectory, "TruckSlip.db3");

        public SQLiteDataService()
        {
            _connection = new SQLiteAsyncConnection(DataLocation);
        }

        public async Task InitializeDatabaseAsync()
            => await CreateTablesAsync();
        
        public async Task CreateTablesAsync()
        {
            await _connection.CreateTableAsync<UnitType>();
            await _connection.CreateTableAsync<Order>();
            await _connection.CreateTableAsync<Company>();

            await CreateJobsiteAsync();
            await CreateProductAsync();
            await CreateOrderItemAsync();

            await _connection.ExecuteAsync("VACUUM;");
        }

        public async Task DropTablesAsync()
        {
            await _connection.DropTableAsync<Product>();
            await _connection.DropTableAsync<UnitType>();
            await _connection.DropTableAsync<Order>();
            await _connection.DropTableAsync<OrderItem>();
            await _connection.DropTableAsync<Jobsite>();
            await _connection.DropTableAsync<Company>();
        }
        
        public async Task CloseDatabaseAsync()
            => await _connection.CloseAsync();

        public Task SetNextId(string tableName, int id)
            => throw new NotImplementedException("SetNextId is not implemented in SQLiteMyFilesDataService");

        //Generic Methods
        private async Task<ObservableCollection<T>> GetAsync<T>() where T : new()
            => new ObservableCollection<T>(await _connection.Table<T>().ToListAsync());

        private async Task<bool> AddOrUpdateAsync<T>(T entity) where T : new()
        {
            ArgumentNullException.ThrowIfNull(entity);

            var primaryKey = typeof(T).GetProperties().FirstOrDefault(p => p.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Length != 0);
            if (primaryKey != null && primaryKey.GetValue(entity) is int primaryKeyValue && primaryKeyValue == 0)
            {
                return Convert.ToBoolean(await _connection.InsertAsync(entity));
            }
            else
            {
                return Convert.ToBoolean(await _connection.UpdateAsync(entity));
            }
        }

        private async Task<bool> AddAsync<T>(T entity) where T : new()
        {
            ArgumentNullException.ThrowIfNull(entity);
            return Convert.ToBoolean(await _connection.InsertOrReplaceAsync(entity));
        }

        private async Task<bool> DeleteAsync<T>(T entity) where T : new()
        {
            var sql = "PRAGMA foreign_keys = ON;";
            await _connection.ExecuteAsync(sql);
            return Convert.ToBoolean(await _connection.DeleteAsync(entity));
        }

        //Products
        private async Task<bool> CreateProductAsync()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS Product(
                      ProductId INTEGER PRIMARY KEY AUTOINCREMENT,
                      Name TEXT NOT NULL,
                      TaskCode TEXT NOT NULL,
                      UnitId  INTEGER NOT NULL,
                      TypeId  INTEGER NOT NULL,
                      FOREIGN KEY (UnitId)
                      REFERENCES UnitType (UnitId)
                      ON UPDATE CASCADE
                      ON DELETE CASCADE)";

            return Convert.ToBoolean(await _connection.ExecuteAsync(sql));
        }

        public async Task<ObservableCollection<Product>> GetProductAsync()
            => await GetAsync<Product>();

        public async Task<bool> AddOrUpdateProductAsync(Product product)
            => await AddOrUpdateAsync(product);

        public async Task<bool> AddProductAsync(Product product)
            => await AddAsync(product);

        public async Task<bool> DeleteProductAsync(Product product) =>
            await DeleteAsync(product);


        //Unit Types
        public async Task<ObservableCollection<UnitType>> GetUnitTypesAsync()
            => await GetAsync<UnitType>();

        public async Task<bool> AddOrUpdateUnitTypeAsync(UnitType unitType)
            => await AddOrUpdateAsync(unitType);

        public async Task<bool> AddUnitTypeAsync(UnitType unitType)
            => await AddAsync(unitType);

        public async Task<bool> DeleteUnitTypeAsync(UnitType unitType) =>
            await DeleteAsync(unitType);

        
        //Order
        public async Task<ObservableCollection<Order>> GetOrderAsync()
            => await GetAsync<Order>();

        public async Task<bool> AddOrUpdateOrderAsync(Order order)
            => await AddOrUpdateAsync(order);

        public async Task<bool> AddOrderAsync(Order order)
            => await AddAsync(order);

        public async Task<bool> DeleteOrderAsync(Order order) =>
            await DeleteAsync(order);
        
        //OrderItem
        private async Task<bool> CreateOrderItemAsync()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS OrderItem(
                      OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                      OrderId INTEGER NOT NULL,
                      Quantity REAL,
                      ProductId INTEGER NOT NULL,
                      FOREIGN KEY(ProductId)
                      REFERENCES Product (ProductId)
                      ON UPDATE CASCADE
                      ON DELETE CASCADE)";

            return Convert.ToBoolean(await _connection.ExecuteAsync(sql));
        }

        public async Task<ObservableCollection<OrderItem>> GetOrderItemAsync()
            => await GetAsync<OrderItem>();

        public async Task<bool> AddOrUpdateOrderItemAsync(OrderItem orderItem)
            => await AddOrUpdateAsync(orderItem);

        public async Task<bool> AddOrderItemAsync(OrderItem orderItem)
            => await AddAsync(orderItem);

        public async Task<bool> DeleteOrderItemAsync(OrderItem orderItem) =>
            await DeleteAsync(orderItem);

        //Company
        public async Task<ObservableCollection<Company>> GetCompanyAsync()
            => await GetAsync<Company>();

        public async Task<bool> AddOrUpdateCompanyAsync(Company company)
            => await AddOrUpdateAsync(company);

        public async Task<bool> AddCompanyAsync(Company company)
            => await AddAsync(company);

        public async Task<bool> DeleteCompanyAsync(Company company) =>
            await DeleteAsync(company);

        //Jobsite
        private async Task<bool> CreateJobsiteAsync()
        {
            var sql = @"CREATE TABLE IF NOT EXISTS Jobsite(
                      JobsiteId INTEGER PRIMARY KEY AUTOINCREMENT,
                      Name TEXT NOT NULL,
                      Address TEXT ,
                      Address2 TEXT ,
                      Contact TEXT ,
                      Number TEXT ,
                      CompanyId  INTEGER NOT NULL,
                      FOREIGN KEY (CompanyId)
                      REFERENCES Company(CompanyId)
                      ON UPDATE CASCADE
                      ON DELETE CASCADE)";

            return Convert.ToBoolean(await _connection.ExecuteAsync(sql));
        }

        public async Task<ObservableCollection<Jobsite>> GetJobsiteAsync()
            => await GetAsync<Jobsite>();

        public async Task<bool> AddOrUpdateJobsiteAsync(Jobsite jobsite)
            => await AddOrUpdateAsync(jobsite);

        public async Task<bool> AddJobsiteAsync(Jobsite jobsite)
            => await AddAsync(jobsite);

        public async Task<bool> DeleteJobsiteAsync(Jobsite jobsite) =>
            await DeleteAsync(jobsite);

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
                        join product in products on orderItem.ProductId equals product.ProductId
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



