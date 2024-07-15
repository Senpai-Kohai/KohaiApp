using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_app
{
    public class AppConfiguration
    {
        public string ProjectsDirectory { get; set; } = "Projects";
        public string RecentProjectsFilename { get; set; } = "recentProjects.json";
        public string TasksFilename { get; set; } = "tasks.json";

        public bool LoadLastProjectOnStartup { get; set; } = true;
        public string? ChatGPTApiKey { get; set; }
        public string? ChatGPTApiUrl { get; set; }
        public string? ChatGPTAssistantAI { get; set; }
        public int ChatGPTRetryMaxAttempts { get; set; } = 50;
        public int ChatGPTRetryInterval { get; set; } = 200;
    }
}
