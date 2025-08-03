using SQLite;

namespace TruckSlip.Models
{
    public class Company
        {
            [PrimaryKey, AutoIncrement]
            public int CompanyId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Address { get; set; } = "Address";
            public string Address2 { get; set; } = "City, State Zip";
            public string Phone { get; set; } = "4125551234";
            public byte[]? Logo { get; set; }
        }
}
