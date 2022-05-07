using Azure.Identity;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

Func<AzureAppConfigurationOptions, IWebHostEnvironment, AzureAppConfigurationOptions> useManagedIdentitiesForProduction = (appConfigurationOptions, env) => env.IsProduction()
    ? appConfigurationOptions.Connect(new Uri("https://app-configurationn.azconfig.io"), new ManagedIdentityCredential())
    : appConfigurationOptions.Connect(builder.Configuration.GetConnectionString("AppConfig"));

builder.Host.ConfigureAppConfiguration(configBuilder =>
{
    configBuilder.AddAzureAppConfiguration(options =>
        useManagedIdentitiesForProduction(options, builder.Environment)
            .Select(KeyFilter.Any, LabelFilter.Null)
            .Select(KeyFilter.Any, builder.Environment.EnvironmentName)
    );
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
