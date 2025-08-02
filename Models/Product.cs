using SQLite;

namespace TruckSlip.Models
{
    public partial class Product
    {
        [PrimaryKey, AutoIncrement]
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TaskCode { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public int UnitId { get; set; }
    }
}
