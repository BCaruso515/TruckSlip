namespace TruckSlip.Helpers
{
    public class SqliteToFirebaseHelper
    {
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;
        private readonly SQLiteDataService _inventoryDB;

        public SqliteToFirebaseHelper(IDataServiceProvider provider)
        {
            _provider = provider;
            _provider.UseRemote();
            _inventoryDB = new SQLiteDataService();
        }

        public async Task SyncUnitTypesAsync()
        {
            var unitTypes = await _inventoryDB.GetUnitTypesAsync();
            var maxId = unitTypes.Max(x => x.UnitId);
            await Database.SetNextId("UnitType", maxId + 1);
            foreach (var unitType in unitTypes)
            {
                await Database.AddUnitTypeAsync(unitType);
            }
        }

        public async Task SyncProductsAsync()
        {
            var products = await _inventoryDB.GetProductAsync();
            var maxId = products.Max(x => x.ProductId);
            await Database.SetNextId("Product", maxId + 1);
            foreach (var product in products)
            {
                await Database.AddProductAsync(product);
            }
        }

        public async Task SyncOrderAsync()
        {
            var orders = await _inventoryDB.GetOrderAsync();
            var maxId = orders.Max(x => x.OrderId);
            await Database.SetNextId("Order", maxId + 1);
            foreach (var order in orders)
            {
                await Database.AddOrderAsync(order);
            }
        }

        public async Task SyncOrderItemAsync()
        {
            var orderItems = await _inventoryDB.GetOrderItemAsync();
            var maxId = orderItems.Max(x => x.OrderItemId);
            await Database.SetNextId("OrderItem", maxId + 1);
            foreach (var orderItem in orderItems)
            {
                await Database.AddOrderItemAsync(orderItem);
            }
        }

        public async Task SyncCompanyAsync()
        {
            var companies = await _inventoryDB.GetCompanyAsync();
            var maxId = companies.Max(x => x.CompanyId);
            await Database.SetNextId("Company", maxId + 1);
            foreach (var company in companies)
            {
                await Database.AddCompanyAsync(company);
            }
        }

        public async Task SyncJobsiteAsync()
        {
            var jobsites = await _inventoryDB.GetJobsiteAsync();
            var maxId = jobsites.Max(x => x.JobsiteId);
            await Database.SetNextId("Jobsite", maxId + 1);
            foreach (var jobsite in jobsites)
            {
                await Database.AddJobsiteAsync(jobsite);
            }
        }

        public async Task SyncAllAsync()
        {
            await SyncCompanyAsync();
            await SyncJobsiteAsync();
            await SyncUnitTypesAsync();
            await SyncProductsAsync();
            await SyncOrderAsync();
            await SyncOrderItemAsync();            
        }
    }
}
