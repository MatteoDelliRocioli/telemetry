#region using
using BlazorApplicationInsights;
using Common;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
#endregion

namespace EasySampleBlazorAppv2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Logging.SetMinimumLevel(LogLevel.Debug);
            var serviceProvider = builder.Services.BuildServiceProvider();

            // replaces the provider with the trace logger provider
            builder.Logging.ClearProviders();
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

            var consoleProvider = new TraceLoggerConsoleProvider();
            var traceLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Console" };
            traceLoggerProvider.AddProvider(consoleProvider);
            builder.Logging.AddProvider(traceLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);

            var appInsightsKey = builder.Configuration["AppSettings:AppInsightsKey"]; Console.WriteLine($"appInsightsKey:{appInsightsKey}");
            var appInsights = new ApplicationInsights(async applicationInsights =>
            {
                await applicationInsights.SetInstrumentationKey(appInsightsKey);
                await applicationInsights.LoadAppInsights();
                var telemetryItem = new TelemetryItem()
                {
                    Tags = new Dictionary<string, object>()
                        {
                            { "ai.cloud.role", "SPA" },
                            { "ai.cloud.roleInstance", "Blazor Wasm" },
                        }
                };
                await applicationInsights.AddTelemetryInitializer(telemetryItem);
            });
            var appinsightProvider = new ApplicationInsightsLoggerProvider(appInsights);
            var appinsightJsonLoggerProvider = new TraceLoggerJsonProvider(builder.Configuration) { ConfigurationSuffix = "Appinsights" };
            appinsightJsonLoggerProvider.AddProvider(appinsightProvider);
            builder.Logging.AddProvider(appinsightJsonLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);

            builder.Services.AddSingleton<IApplicationInsights, ApplicationInsights>(sp => appInsights);

            serviceProvider = builder.Services.BuildServiceProvider();
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            Console.WriteLine($"loggerFactory: '{loggerFactory}'");

            // gets a logger from the ILoggerFactory
            var logger = loggerFactory.CreateLogger<Program>();
            Console.WriteLine($"logger: '{logger}'");

            using (var scope = logger.BeginMethodScope())
            {
                builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

                var host = builder.Build();
                var ihost = new WebAssemblyIHostAdapter(host) as IHost;
                //ihost.UseWebCommands();
                ihost.InitTraceLogger();

                await host.RunAsync();
            }
        }
    }
}
