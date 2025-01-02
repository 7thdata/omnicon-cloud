using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using fncTickWorks.Config;

var builder = FunctionsApplication.CreateBuilder(args);

// Add custom dependency injection setup
builder.Services.AddFunctionServices();

// Optional: Enable Application Insights (uncomment if needed)
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

// Build and run the application
builder.Build().Run();