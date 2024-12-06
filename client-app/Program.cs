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
        private static ServiceProvider? s_serviceProvider;
        private static AppConfiguration? s_appConfiguration;
        private static readonly CancellationTokenSource s_shutdownTokenSource = new();

        public static AppConfiguration? AppConfiguration
        {
            get { return s_appConfiguration; }
            private set { s_appConfiguration = value; }
        }

        public static ServiceProvider? ServiceProvider
        {
            get { return s_serviceProvider; }
            private set { s_serviceProvider = value; }
        }


        public static bool ShuttingDown => s_shutdownTokenSource.IsCancellationRequested;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AppConfiguration = InitializeConfiguration();
            if (!ValidateConfiguration(AppConfiguration))
            {
                Debug.WriteLine("App configuration validation failed. Exiting application.");
                Environment.Exit(1);
            }

            var services = new ServiceCollection();
            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.StartKohaiServices();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.Run(ServiceProvider.GetRequiredService<MainForm>());

            InitiateShutdown();
        }

        public static void InitiateShutdown()
        {
            s_shutdownTokenSource.Cancel();
        }

        private static AppConfiguration InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs());

            var configRoot = builder.Build();
            var config = new AppConfiguration();

            configRoot?.Bind(config);

            return config;
        }

        private static bool ValidateConfiguration(AppConfiguration? appConfiguration)
        {
            if (appConfiguration == null || !Configuration.IConfiguration.ValidateConfiguration(appConfiguration))
                return false;

            return true;
        }

        private static void ConfigureServices(ServiceCollection servicesCollection)
        {
            if (AppConfiguration != null)
                servicesCollection.AddSingleton(AppConfiguration);

            servicesCollection.AddSingleton(s_shutdownTokenSource);
            servicesCollection.AddSingleton(new HttpClient());

            servicesCollection.AddKohaiServices();
            servicesCollection.AddKohaiServiceConfiguration();

            servicesCollection.AddSingleton<MainForm>();
        }
    }
}
