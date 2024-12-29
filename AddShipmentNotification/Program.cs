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
    serviceProvider => new HttpClientWrapper(serviceProvider.GetService<HttpClient>())
);

// Adds DqlConnection dependency to be injected into SqlDbService
builder.Services.AddSingleton<ISqlDbService, SqlDbService>(serviceProvider => new SqlDbService(
    builder.Configuration.GetValue<string>("SqlConnectionString") ?? "SqlConnectionString_NOT_SET",
    serviceProvider.GetRequiredService<ISanitation>(),
    serviceProvider.GetRequiredService<ILogger<SqlDbService>>(),
    builder.Configuration.GetValue<string>("dbName") ?? "dbName_NOT_SET",
    builder.Configuration.GetValue<string>("shipmentTableName") ?? "shipmentTableName_NOT_SET",
    builder.Configuration.GetValue<string>("shipmentLinesTableName")
        ?? "shipmentLinesTableName_NOT_SET"
));

builder.Build().Run();
