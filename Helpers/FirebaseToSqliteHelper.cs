namespace TruckSlip.Helpers
{
    public class FirebaseToSqliteHelper
    {
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;
        private readonly SQLiteDataService _inventoryDB;

        public FirebaseToSqliteHelper(IDataServiceProvider provider)
        {
            _provider = provider;
            _provider.UseRemote(); // Ensure we are using remote database by default
            _inventoryDB = new();
        }

        public async Task SyncUnitTypesAsync()
        {
            var unitTypes = await Database.GetUnitTypesAsync();
            if (unitTypes == null || !unitTypes.Any()) return;
            foreach (var unitType in unitTypes)
            {
                await _inventoryDB.AddUnitTypeAsync(unitType);
            }
        }

        public async Task SyncProductsAsync()
        {
            var products = await Database.GetProductAsync();
            if (products == null || !products.Any()) return;
            foreach (var product in products)
            {
                await _inventoryDB.AddProductAsync(product);
            }
        }

        public async Task SyncOrdersAsync()
        {
            var orders = await Database.GetOrderAsync();
            if (orders == null || !orders.Any()) return;
            foreach (var order in orders)
            {
                await _inventoryDB.AddOrderAsync(order);
            }
        }

        public async Task SyncOrderItemsAsync()
        {
            var orderItems = await Database.GetOrderItemAsync();
            if (orderItems == null || !orderItems.Any()) return;
            foreach (var orderItem in orderItems)
            {
                await _inventoryDB.AddOrderItemAsync(orderItem);
            }
        }

        public async Task SyncJobsiteAsync()
        {
            var jobsites = await Database.GetJobsiteAsync();
            if (jobsites == null || !jobsites.Any()) return;
            foreach (var jobsite in jobsites)
            {
                await _inventoryDB.AddJobsiteAsync(jobsite);
            }
        }

        private async Task SyncAllAsync()
        {
            await SyncJobsiteAsync();
            await SyncUnitTypesAsync();
            await SyncProductsAsync();
            
            await SyncOrdersAsync();
            await SyncOrderItemsAsync();
        }

        public async Task CreateBackupAsync()
        {
            await _inventoryDB.CloseDatabaseAsync(); // Ensure the database is closed before creating a backup
            await _inventoryDB.DropTablesAsync(); // Clear the backup database
            await _inventoryDB.CreateTablesAsync(); // Ensure tables are created in the backup database
            await SyncAllAsync(); // Sync all data from Firebase to SQLite backup
        }
    }
}
