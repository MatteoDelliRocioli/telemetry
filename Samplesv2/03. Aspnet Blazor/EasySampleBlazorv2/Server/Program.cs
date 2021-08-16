using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging.AzureAppServices;

namespace EasySampleBlazorv2.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = CreateHostBuilder(args)
                          .ConfigureLogging((context, loggingBuilder) =>
                          {
                              loggingBuilder.ClearProviders();

                              var options = new Log4NetProviderOptions();
                              options.Log4NetConfigFileName = "log4net.config";
                              var log4NetProvider = new Log4NetProvider(options);
                              loggingBuilder.AddDiginsightFormatted(log4NetProvider, context.Configuration);

                              //TelemetryConfiguration telemetryConfiguration = new TelemetryConfiguration("6600ae1e-1466-4ad4-aea7-c017a8ab5dce");
                              //ApplicationInsightsLoggerOptions appinsightOptions = new ApplicationInsightsLoggerOptions();
                              //var tco = Options.Create<TelemetryConfiguration>(telemetryConfiguration);
                              //var aio = Options.Create<ApplicationInsightsLoggerOptions>(appinsightOptions);
                              //loggingBuilder.AddDiginsightJson(new ApplicationInsightsLoggerProvider(tco, aio), context.Configuration);
                              ////loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Debug);

                              var debugProvider = new TraceLoggerDebugProvider();
                              var traceLoggerProvider = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Debug" };
                              traceLoggerProvider.AddProvider(debugProvider);
                              loggingBuilder.AddProvider(traceLoggerProvider); // i.e. builder.Services.AddSingleton(traceLoggerProvider);

                              // loggingBuilder.AddAzureWebAppDiagnostics(); // STREAMING LOG not working ?

                              //var consoleProvider = new TraceLoggerConsoleProvider();
                              //var traceLoggerProviderConsole = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Console" };
                              //traceLoggerProviderConsole.AddProvider(consoleProvider);
                              //loggingBuilder.AddProvider(traceLoggerProviderConsole); // i.e. builder.Services.AddSingleton(traceLoggerProvider);

                              //var debugProvider = new DebugLoggerProvider();
                              //var traceLoggerProviderDebug = new TraceLoggerFormatProvider(context.Configuration) { ConfigurationSuffix = "Debug" };
                              //traceLoggerProviderDebug.AddProvider(debugProvider);
                              //loggingBuilder.AddProvider(traceLoggerProviderDebug); // i.e. builder.Services.AddSingleton(traceLoggerProvider);
                          });

            var host = builder.Build();

            host.InitTraceLogger();

            var logger = host.GetLogger<Program>();
            using (var scope = logger.BeginMethodScope())
            {
                host.Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
