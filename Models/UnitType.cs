using SQLite;

namespace TruckSlip.Models
{
    public partial class UnitType
    {
        [PrimaryKey, AutoIncrement]
        public int UnitId { get; set; }
        public string UnitName { get; set; } = string.Empty;
    }
}
