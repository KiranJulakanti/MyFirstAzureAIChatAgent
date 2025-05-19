using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MCPTriggerFunction.Services.Telemetry;
using MCPTriggerFunction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {        // Add TelemetryClient without using AddApplicationInsightsTelemetry
        services.AddSingleton<TelemetryClient>(sp => {
            var telemetryConfiguration = new TelemetryConfiguration();
            telemetryConfiguration.ConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            return new TelemetryClient(telemetryConfiguration);
        });
        
        // Register telemetry service
        services.AddSingleton<ITelemetryService, ApplicationInsightsTelemetryService>();
        
        // Register HttpClient factory
        services.AddHttpClient();
        
        // Register ApplicationInsightsQueryService with manual DI
        services.AddSingleton<ApplicationInsightsQueryService>(sp => {
            var logger = sp.GetRequiredService<ILogger<ApplicationInsightsQueryService>>();
            var configuration = sp.GetRequiredService<IConfiguration>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new ApplicationInsightsQueryService(httpClient, logger, configuration);
        });
    })
    .Build();

host.Run();
