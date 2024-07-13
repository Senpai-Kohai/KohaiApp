using System.Configuration;
using System.Net;

namespace client_app
{
    internal static class Program
    {
        public static AppConfig? AppConfig { get; set; }
        public static HttpClient HttpClient { get; private set; } = new HttpClient();
        public static string FilePath { get; private set; } = "tasks.json";

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            AppConfig = AppConfig.LoadFromEnvironmentAndArgs(args);

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
