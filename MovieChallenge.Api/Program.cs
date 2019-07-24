using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using System;
using System.IO;

namespace MovieChallenge.Api
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext().MinimumLevel.Override("Microsoft", LogEventLevel.Error).MinimumLevel.Override("System", LogEventLevel.Error)
                .WriteTo.File(Configuration["Serilog:FileName"], rollOnFileSizeLimit: true).CreateLogger();

            try
            {
                Log.Information("Heating up...");

                var host = CreateWebHostBuilder(args);
                host.Build().Run();

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseSerilog();
    }
}
