using interview.Retry;
using interview.Sanitation;
using interview.SqlDbService;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

// Adds Http Client as a DI Constructor parameter
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IRetry>(new Retry());
builder.Services.AddSingleton<ISqlDbService, SqlDbService>(serviceProvider => new SqlDbService(
    builder.Configuration.GetValue<string>("SqlConnectionString") ?? "CONNECTION_STRING_NOT_SET",
    serviceProvider.GetRequiredService<ILogger<SqlDbService>>()
));
builder.Services.AddSingleton<ISanitation>(new Sanitation());

builder.Build().Run();
