namespace TruckSlip.Models
{
    public class OrderItemsQuery
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TaskCode { get; set; } = string.Empty;
        public float Quantity { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }
}
