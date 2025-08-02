using SQLite;

namespace TruckSlip.Models
{
    public partial class OrderItem //Truck Slip
    {
        [PrimaryKey, AutoIncrement]
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public float Quantity { get; set; }
    }
}