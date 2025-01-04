using interview.HttpClientWrapper;
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

builder.Services.AddSingleton<IRetry, Retry>();
builder.Services.AddSingleton<ISanitation, Sanitation>();

// Adds Http Client as a DI Constructor parameter to be injected into HttpClientWrapper
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>(
    serviceProvider => new HttpClientWrapper(serviceProvider.GetRequiredService<HttpClient>())
);

// Adds a Sql connector singleton, for dependency injection into SqlDbService
builder.Services.AddSingleton<IDbConnector, SqlConnector>(serviceProvider => new SqlConnector(
    builder.Configuration.GetValue<string>("SqlConnectionString")
        ?? throw new InvalidOperationException(
            "SqlConnectionString is a required configuration value."
        )
));

// Adds DqlConnection dependency to be injected into SqlDbService
builder.Services.AddSingleton<ISqlDbService, SqlDbService>(serviceProvider => new SqlDbService(
    serviceProvider.GetRequiredService<IDbConnector>(),
    serviceProvider.GetRequiredService<ISanitation>(),
    serviceProvider.GetRequiredService<ILogger<SqlDbService>>(),
    builder.Configuration.GetValue<string>("dbSchema") ?? "dbo",
    builder.Configuration.GetValue<string>("shipmentTableName") ?? "markShipment",
    builder.Configuration.GetValue<string>("shipmentLinesTableName") ?? "markShipment_line"
));

builder.Build().Run();
