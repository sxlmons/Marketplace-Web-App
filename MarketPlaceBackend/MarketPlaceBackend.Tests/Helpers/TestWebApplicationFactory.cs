using MarketPlaceBackend.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MarketPlaceBackend.Tests.Helpers;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL existing DbContext registrations
            var descriptorsToRemove = services
                .Where(d => d.ServiceType.FullName.Contains("DbContextOptions") ||
                            d.ServiceType.FullName.Contains("Npgsql"))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
                services.Remove(descriptor);

            // Replace with in-memory database (unique per factory)
            var dbName = $"TestDb_{Guid.NewGuid()}";
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });

            // Isolate Data Protection keys per factory instance
            // so auth cookies don't bleed across test fixtures
            services.AddDataProtection()
                .SetApplicationName($"TestApp_{Guid.NewGuid()}");
        });
    }
}