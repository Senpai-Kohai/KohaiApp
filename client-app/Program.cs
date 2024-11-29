using System;
using System.CodeDom;
using System.Configuration;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using client_app.Attributes;
using client_app.Services;
using client_app.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace client_app
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
            var config = new AppConfiguration();
            Configuration?.Bind(config);

            if (!ConfigurationUtils.ValidateConfiguration(config))
            {
                Debug.WriteLine("Configuration validation failed. Exiting application.");
                Environment.Exit(1);
            }

            services.AddSingleton<AppConfiguration>(config);
            services.AddSingleton(new HttpClient());
            services.AddSingleton<ProjectService>();
            services.AddSingleton<AIService>();
            services.AddSingleton<MainForm>();

            // add all service configuration types to service registry, so forms/etc can easily use them via constructors
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract || type.IsNotPublic || type.IsValueType)
                    continue;

                try
                {
                    var configAttr = type.GetCustomAttribute<ServiceConfigurationAttribute>();
                    if (configAttr == null)
                        continue;

                    var serviceConfig = Activator.CreateInstance(type);
                    if (serviceConfig == null)
                        continue;

                    Debug.WriteLine($"Adding service configuration type [{type.Name}] directly to the service provider.");

                    ConfigurationUtils.LoadConfiguration(serviceConfig, configAttr?.SectionName);
                    services.AddSingleton(type, serviceConfig);
                }
                catch (Exception exc)
                {
                    Debug.WriteLine($"An exception occurred trying to add service configuration type [{type.Name}] to the service provider: {exc}.");
                }
            }
        }
    }
}
