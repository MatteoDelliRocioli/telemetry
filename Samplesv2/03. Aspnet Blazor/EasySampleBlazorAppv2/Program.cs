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
        private static Type T = typeof(Program);

        public static async Task Main(string[] args)
        {
            using var scope = TraceLogger.BeginMethodScope(T);

            var builder = WebAssemblyHostBuilder.CreateDefault(args); scope.LogDebug($"WebAssemblyHostBuilder.CreateDefault(args); returned {builder.GetLogString()}");
            builder.RootComponents.Add<App>("#app"); scope.LogDebug($"builder.RootComponents.Add<App>('#app');");

            builder.Logging.SetMinimumLevel(LogLevel.Debug); scope.LogDebug($"builder.Logging.SetMinimumLevel({LogLevel.Debug});");
            var serviceProvider = builder.Services.BuildServiceProvider(); scope.LogDebug($"var serviceProvider = builder.Services.BuildServiceProvider();");

            // replaces the provider with the trace logger provider
            builder.Logging.ClearProviders(); scope.LogDebug($"builder.Logging.ClearProviders();");
            builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging")); scope.LogDebug($"builder.Logging.AddConfiguration(builder.Configuration.GetSection('Logging'));");

            var consoleProvider = new TraceLoggerConsoleProvider();
            var traceLoggerConsoleProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Console" };
            traceLoggerConsoleProvider.AddProvider(consoleProvider);
            builder.Logging.AddProvider(traceLoggerConsoleProvider); scope.LogDebug($"builder.Logging.AddProvider(traceLoggerConsoleProvider);");

            var appInsightsKey = builder.Configuration["AppSettings:AppInsightsKey"]; scope.LogDebug(new { appInsightsKey });
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
            //var appinsightJsonLoggerProvider = new TraceLoggerJsonProvider(builder.Configuration) { ConfigurationSuffix = "Appinsights" };
            var appinsightFormatLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Appinsights" };
            appinsightFormatLoggerProvider.AddProvider(appinsightProvider);
            builder.Logging.AddProvider(appinsightFormatLoggerProvider); scope.LogDebug($"builder.Logging.AddProvider(appinsightFormatLoggerProvider);");
            builder.Services.AddSingleton<IApplicationInsights, ApplicationInsights>(sp => appInsights); scope.LogDebug($"builder.Services.AddSingleton<IApplicationInsights, ApplicationInsights>(sp => appInsights);");

            serviceProvider = builder.Services.BuildServiceProvider(); scope.LogDebug($"serviceProvider = builder.Services.BuildServiceProvider();");

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>(); scope.LogDebug($"var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();");

            // gets a logger from the ILoggerFactory
            var logger = loggerFactory.CreateLogger<Program>(); scope.LogDebug($"var logger = loggerFactory.CreateLogger<Program>();");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); scope.LogDebug($"builder.Services.AddScoped(sp => new HttpClient({builder.HostEnvironment.BaseAddress}));");

            var host = builder.Build(); scope.LogDebug($"var host = builder.Build();");
            var ihost = new WebAssemblyIHostAdapter(host) as IHost;
            //ihost.UseWebCommands();
            ihost.InitTraceLogger(); scope.LogDebug($"ihost.InitTraceLogger();");

            await host.RunAsync(); scope.LogDebug($"await host.RunAsync();");
        }
    }
}
