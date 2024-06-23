using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app
{
    public class AppConfig
    {
        public string? ChatGPTApiKey { get; set; }
        public string? ChatGPTApiUrl { get; set; }

        public static AppConfig LoadFromEnvironmentAndArgs(string[] args)
        {
            var config = new AppConfig();

            // Load from environment variables
            config.ChatGPTApiKey = Environment.GetEnvironmentVariable("CHATGPT_API_KEY");
            config.ChatGPTApiUrl = Environment.GetEnvironmentVariable("CHATGPT_API_URL");

            // Override with command-line arguments if provided
            foreach (var arg in args)
            {
                if (arg.StartsWith("--apikey="))
                {
                    config.ChatGPTApiKey = arg.Substring("--apikey=".Length);
                }
                else if (arg.StartsWith("--apiurl="))
                {
                    config.ChatGPTApiUrl = arg.Substring("--apiurl=".Length);
                }
            }

            if (string.IsNullOrWhiteSpace(config.ChatGPTApiKey))
            {
                Debug.WriteLine("CHATGPT API KEY NOT CONFIGURED!");
            }

            if (string.IsNullOrWhiteSpace(config.ChatGPTApiUrl))
            {
                Debug.WriteLine("CHATGPT API URL NOT CONFIGURED!");
            }

            return config;
        }
    }
}
