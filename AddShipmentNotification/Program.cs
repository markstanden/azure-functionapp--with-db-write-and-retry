using interview.HttpClientWrapper;
using interview.Retry;
using interview.Sanitation;
using interview.Services.Database;
using interview.Services.Webhook;
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
builder.Services.AddSingleton<IHttpClientWrapper>(serviceProvider => new HttpClientWrapper(
    serviceProvider.GetRequiredService<HttpClient>()
));

// Injects the HttpClientWrapper into the webhookService
builder.Services.AddSingleton<IWebhookService>(serviceProvider => new WebhookService(
    serviceProvider.GetRequiredService<IHttpClientWrapper>(),
    serviceProvider.GetRequiredService<ILogger<WebhookService>>(),
    builder.Configuration.GetValue<string>("webhookUrl")
        // Throws early if a required configuration value is missing (rather than wasting compute)
        ?? throw new InvalidOperationException("'webhookUrl' is a required configuration value.")
));

// Adds a Sql connector singleton, for dependency injection into SqlDbService
builder.Services.AddSingleton<IDbConnector, SqlConnector>(_ => new SqlConnector(
    builder.Configuration.GetValue<string>("SqlConnectionString")
        // Throws early if a required configuration value is missing.
        ?? throw new InvalidOperationException(
            "'SqlConnectionString' is a required configuration value."
        )
));

// Adds DqlConnection dependency to be injected into SqlDbService
builder.Services.AddSingleton<ISqlDbService, SqlDbService>(serviceProvider => new SqlDbService(
    serviceProvider.GetRequiredService<IDbConnector>(),
    serviceProvider.GetRequiredService<ISanitation>(),
    serviceProvider.GetRequiredService<ILogger<SqlDbService>>(),
    // Adds default configuration values, as unlikely to change in code
    builder.Configuration.GetValue<string>("dbSchema") ?? "dbo",
    builder.Configuration.GetValue<string>("shipmentTableName") ?? "markShipment",
    builder.Configuration.GetValue<string>("shipmentLinesTableName") ?? "markShipment_line"
));

builder.Build().Run();
