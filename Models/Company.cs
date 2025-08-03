using SQLite;

namespace TruckSlip.Models
{
    public class Company
        {
            [PrimaryKey, AutoIncrement]
            public int CompanyId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Address { get; set; }
            public string? Address2 { get; set; }
            public string? Phone { get; set; }
            public byte[]? Logo { get; set; }
        }
}
