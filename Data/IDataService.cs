namespace TruckSlip.Data
{
    public interface IDataService
    {
        Task CreateTablesAsync();
        Task DropTablesAsync();
        Task CloseDatabaseAsync();
        Task InitializeDatabaseAsync();
        Task SetNextId(string tableName, int id);

        // Products
        Task<ObservableCollection<Product>> GetProductAsync();
        Task<bool> AddOrUpdateProductAsync(Product product);
        Task<bool> AddProductAsync(Product product);
        Task<bool> DeleteProductAsync(Product product);

        // Unit Types
        Task<ObservableCollection<UnitType>> GetUnitTypesAsync();
        Task<bool> AddOrUpdateUnitTypeAsync(UnitType unitType);
        Task<bool> AddUnitTypeAsync(UnitType unitType);
        Task<bool> DeleteUnitTypeAsync(UnitType unitType);

        // Order
        Task<ObservableCollection<Order>> GetOrderAsync();
        Task<bool> AddOrUpdateOrderAsync(Order order);
        Task<bool> AddOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(Order order);

        // OrderItem
        Task<ObservableCollection<OrderItem>> GetOrderItemAsync();
        Task<bool> AddOrUpdateOrderItemAsync(OrderItem orderItem);
        Task<bool> AddOrderItemAsync(OrderItem orderItem);
        Task<bool> DeleteOrderItemAsync(OrderItem orderItem);

        //Jobsite
        Task<ObservableCollection<Jobsite>> GetJobsiteAsync();
        Task<bool> AddOrUpdateJobsiteAsync(Jobsite jobsite);
        Task<bool> AddJobsiteAsync(Jobsite jobsite);
        Task<bool> DeleteJobsiteAsync(Jobsite jobsite);

        // OrderItemsQuery
        Task<ObservableCollection<OrderItemsQuery>> GetOrderItemsQueryAsync();
    }
}
