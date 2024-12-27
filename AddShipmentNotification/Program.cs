using interview.Retry;
using interview.Sanitation;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

// Adds Http Client as a DI Constructor parameter
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IRetry>(new Retry());
builder.Services.AddSingleton<ISanitation>(new Sanitation());

builder.Build().Run();
