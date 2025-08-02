using Microsoft.Extensions.Configuration;
using CommunityToolkit.Maui;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;

namespace TruckSlip
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("brands-regular-400.otf", "BrandsRegular");
                    fonts.AddFont("free-regular-400.otf", "FreeRegular");
                    fonts.AddFont("free-solid-900.otf", "FreeSolid");
                });

            IConfiguration config;

            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            builder.Services.AddSingleton<IConfiguration>(config);

            IConfigurationSection section = config.GetSection("Firebase");

            var apiKey = section["ApiKey"];
            var authDomain = section["AuthDomain"];
            var databaseUrl = section["DatabaseUrl"];

            builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig
            {
                ApiKey = apiKey,
                AuthDomain = authDomain,
                Providers =
                [
                    new EmailProvider()
                ],
                UserRepository = new FileUserRepository("TruckSlip")
            }));

            builder.Services.AddSingleton<FirebaseClient>(sp =>
            {
                var authClient = sp.GetRequiredService<FirebaseAuthClient>();
                return new FirebaseClient(databaseUrl, new FirebaseOptions
                {
                    AuthTokenAsyncFactory = async () =>
                    {
                        var user = authClient.User;
                        return user != null ? await user.GetIdTokenAsync(true) : string.Empty;
                    }
                });
            });

            builder.Services.AddSingleton<SQLiteDataService>();
            builder.Services.AddSingleton<FirebaseDataService>();
            builder.Services.AddSingleton<IDataServiceProvider>(sp =>
            {
                var local = sp.GetRequiredService<SQLiteDataService>();
                var remote = sp.GetRequiredService<FirebaseDataService>();
                return new DataServiceProvider(local, remote);
            });

            builder.Services.AddSingleton<SignUpPage>();
            builder.Services.AddSingleton<SignUpViewModel>();

            builder.Services.AddTransient<HomePage>();
            builder.Services.AddTransient<HomeViewModel>();
            builder.Services.AddTransient<JobsitePage>();
            builder.Services.AddTransient<JobsiteViewModel>();
            builder.Services.AddTransient<OrderPage>();
            builder.Services.AddTransient<OrderViewModel>();
            builder.Services.AddTransient<OrderItemsPage>();
            builder.Services.AddTransient<OrderItemsViewModel>();
            builder.Services.AddTransient<ProductPage>();
            builder.Services.AddTransient<ProductViewModel>();
            builder.Services.AddTransient<SignFormPage>();
            builder.Services.AddTransient<SignFormViewModel>();
            builder.Services.AddTransient<SignInPage>();
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<UnitTypesPage>();
            builder.Services.AddTransient<UnitTypesViewModel>();

            return builder.Build();
        }
    }
}
