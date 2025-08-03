using SQLite;

namespace TruckSlip.Models
{
    public class Jobsite
    {
        [PrimaryKey, AutoIncrement]
        public int JobsiteId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = "Address";
        public string Address2 { get; set; } = "City, State Zip";
        public string Contact { get; set; } = "Jobsite Contact (123) 456-7890";
        public string Number { get; set; } = "12-1234";
        public int CompanyId { get; set; } 
    }
}
