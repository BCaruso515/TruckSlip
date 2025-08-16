using System.Text.Json;

namespace TruckSlip.Helpers
{
    public class DefaultDataHelper
    {
        private readonly IDataServiceProvider _provider;
        private IDataService Database => _provider.Current;

        public DefaultDataHelper(IDataServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task LoadDefaultsAsync()
        {
            // Ensure the database is initialized
            await Database.DropTablesAsync();
            await Database.CreateTablesAsync();

            // Load default data from JSON file
            await LoadDataFromJsonAsync();
        }

        private async Task LoadDataFromJsonAsync()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("truck-slip.json");
            using var reader = new StreamReader(stream);
            var jsonContent = await reader.ReadToEndAsync();

            // Parse the JSON
            var doc = JsonDocument.Parse(jsonContent);
            await LoadCompaniesFromJsonAsync(doc);
            await LoadUnitTypesFromJsonAsync(doc);
            await LoadProductsFromJsonAsync(doc);
        }

        private async Task LoadCompaniesFromJsonAsync(JsonDocument jsonDocument)
        {
            if (!jsonDocument.RootElement.TryGetProperty("Company", out var companyObj))
                throw new Exception("Company data not found");
            foreach (var company in companyObj.EnumerateObject())
            {
                var companyData = company.Value.Deserialize<Company>();
                if (companyData != null)
                    await Database.AddCompanyAsync(companyData);
            }            
        }

        private async Task LoadUnitTypesFromJsonAsync(JsonDocument jsonDocument)
        {
            if (! jsonDocument.RootElement.TryGetProperty("UnitType", out var unitTypeObj))
                throw new Exception("UnitType data not found");
            foreach (var unitType in unitTypeObj.EnumerateObject())
            {
                var unitTypeData = unitType.Value.Deserialize<UnitType>();
                if (unitTypeData != null)
                    await Database.AddUnitTypeAsync(unitTypeData);
            }
        }

        private async Task LoadProductsFromJsonAsync(JsonDocument jsonDocument)
        {
            if (! jsonDocument.RootElement.TryGetProperty("Product", out var productObj))
                throw new Exception("Product data not found");
            
            foreach (var product in productObj.EnumerateObject())
            {
                var productData = product.Value.Deserialize<Product>();
                if (productData != null)
                    await Database.AddProductAsync(productData);
            }
        }
    }
}
