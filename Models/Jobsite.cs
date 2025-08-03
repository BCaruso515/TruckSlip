using SQLite;

namespace TruckSlip.Models
{
    public class Jobsite
    {
        [PrimaryKey, AutoIncrement]
        public int JobsiteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Address2 { get; set; } 
        public string? Contact { get; set; } 
        public string? Number { get; set; }
        public int CompanyId { get; set; } 
    }
}
