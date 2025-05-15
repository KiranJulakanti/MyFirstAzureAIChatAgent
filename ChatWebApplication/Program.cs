using ChatWebApplication.AzureOpenAi;
using ChatWebApplication.Models;
using ChatWebApplication.SemanticKernel;
using ChatWebApplication.Services;
using ChatWebApplication.Services.Interfaces;
using ChatWebApplication.Services.Telemetry;
using ChatWebApplication.SignalRHub;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.SemanticKernel;

namespace ChatWebApplication
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Application Insights telemetry
            builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                // You can customize options here if needed
            });

            // Configure Application Insights for advanced tracking options
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                // Track dependencies for Azure services
                module.EnableW3CHeadersInjection = true;
            });

            // Register telemetry services
            builder.Services.AddSingleton<ITelemetryService, ApplicationInsightsTelemetryService>();

            builder.Services.AddRazorPages();

            // Add SignalR services
            builder.Services.AddSignalR();

            // Add the Semantic Kernel services
            builder.Services.AddKernel()
            .AddAzureOpenAIChatCompletion(
                AzureOpenAIServiceConfig.OpenAIEndpoint,
                AzureOpenAIServiceConfig.Key,
                AzureOpenAIServiceConfig.DeploymentName,
                AzureOpenAIServiceConfig.ModelId);

            // Add other services and models for instances
            builder.Services.AddSingleton<ChatKernelPlugin>();
            builder.Services.AddSingleton<AzureOpenAIService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<IBigCatService, BigCatService>();
            builder.Services.AddSingleton<BigCatSettings>();

            var app = builder.Build();

            // Register the plugin with the kernel
            using (var scope = app.Services.CreateScope())
            {
                var kernel = scope.ServiceProvider.GetRequiredService<Kernel>();
                var plugin = scope.ServiceProvider.GetRequiredService<ChatKernelPlugin>();

                // Register the plugin with the kernel where the 
                kernel.Plugins.AddFromObject(plugin, "ChatKernelPlugin");
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
