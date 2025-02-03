using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using fncTickWorks.Config;
using clsCms.Services;
using clsCMs.Data;
using clsCms.Interfaces;

var builder = FunctionsApplication.CreateBuilder(args);

// Add custom dependency injection setup
builder.Services.AddFunctionServices(); // Ensure this includes all your services

// Register ApplicationDbContext with the connection string from environment variables
string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString")
                          ?? throw new InvalidOperationException("SqlConnectionString not configured.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Optional: Enable Application Insights (uncomment if needed)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService();

// Build and validate service registrations
var host = builder.Build();

// Validation check for service resolution during startup
using (var scope = host.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    try
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        Console.WriteLine("ApplicationDbContext resolved successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error resolving ApplicationDbContext: {ex.Message}");
    }

    try
    {
        var tickServices = serviceProvider.GetRequiredService<TickServices>();
        Console.WriteLine("TickServices resolved successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error resolving TickServices: {ex.Message}");
    }
}

// Run the application
host.Run();
