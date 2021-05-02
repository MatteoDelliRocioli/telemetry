#region using
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

            var consoleProvider = new TraceLoggerConsoleProvider();
            var traceLoggerProvider = new TraceLoggerFormatProvider(builder.Configuration) { ConfigurationSuffix = "Console" };
            traceLoggerProvider.AddProvider(consoleProvider);
            builder.Logging.AddProvider(traceLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);

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
