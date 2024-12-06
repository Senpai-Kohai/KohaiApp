using System;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Kohai.Attributes;
using Kohai.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kohai.Client
{
    internal static class Program
    {
        public static IConfiguration? Configuration { get; private set; }
        public static ServiceProvider? ServiceProvider { get; private set; }


        private static CancellationTokenSource? s_shutdownTokenSource = null;
        public static CancellationTokenSource ShutdownTokenSource
        {
            get
            {
                s_shutdownTokenSource ??= new CancellationTokenSource();

                return s_shutdownTokenSource;
            }

            private set { s_shutdownTokenSource = value; }
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ShutdownTokenSource = new CancellationTokenSource();

            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs());

            Configuration = builder.Build();

            // Set up dependency injection
            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();

            // Enable visual styles for the application
            Application.EnableVisualStyles();

            // Ensure text rendering uses GDI+
            Application.SetCompatibleTextRenderingDefault(false);

            // Set high DPI mode to improve appearance on high-resolution displays
            Application.SetHighDpiMode(HighDpiMode.SystemAware);

            // Start the main form of the application
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());

            ShutdownTokenSource.Cancel();
        }

        public static void InitiateShutdown()
        {
            ShutdownTokenSource.Cancel();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // CONFIGURATION
            var config = new AppConfiguration();
            Configuration?.Bind(config);

            if (!Kohai.Configuration.IConfiguration.ValidateConfiguration(config))
            {
                Debug.WriteLine("Configuration validation failed. Exiting application.");
                Environment.Exit(1);
            }

            // SERVICES
            services.AddSingleton(ShutdownTokenSource);
            services.AddSingleton<AppConfiguration>(config);
            services.AddSingleton(new HttpClient());
            services.AddKohaiServices();
            services.AddKohaiServiceConfiguration();

            // UX
            services.AddSingleton<MainForm>();
        }
    }
}
