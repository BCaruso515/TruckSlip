using SQLite;

namespace TruckSlip.Models
{
    public partial class Order //Truck Slip
    {
        [PrimaryKey, AutoIncrement]
        public int OrderId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public int JobsiteId { get; set; }
        public int TruckTypeId { get; set; }
        public int OrderTypeId { get; set; } = Convert.ToByte(true); // 1 = Delivery, 0 = Pickup
    }
}