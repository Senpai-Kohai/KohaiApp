using System;
using System.CodeDom;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace client_app
{
    internal static class Program
    {
        private static IConfiguration? Configuration { get; set; }
        private static ServiceProvider? ServiceProvider { get; set; }

        public static CancellationTokenSource ShutdownTokenSource { get; private set; }

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
            var config = new AppConfiguration();
            Configuration?.Bind(config);

            if (!ValidateConfiguration(config))
            {
                Debug.WriteLine("Configuration validation failed. Exiting application.");
                Environment.Exit(1);
            }

            services.AddSingleton(config);
            services.AddSingleton(new HttpClient());
            services.AddSingleton<ProjectService>();
            services.AddSingleton<AIService>();
            services.AddSingleton<MainForm>();
        }

        public static bool ValidateConfiguration(object config)
        {
            var configType = config.GetType();
            var properties = configType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetCustomAttribute<RequiredConfigurationAttribute>() != null)
                {
                    var value = property.GetValue(config);
                    if (value == null)
                    {
                        Debug.WriteLine($"Configuration property '{property.Name}' is required but is not set.");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
