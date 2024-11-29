using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace client_app
{
    /// <summary>
    /// The configuration for the app as a whole, such a setting logging levels, file management, etc.
    /// Please create additional configuration types for individual services, and decorate them with the ServiceConfigurationAttribute.
    /// Feel free to dervice from AppConfiguration to include the app config in your service config type as well.
    /// </summary>
    public class AppConfiguration
    {
        public string ProjectsDirectory { get; set; } = "Projects";
        public string RecentProjectsFilename { get; set; } = "recentProjects.json";
        public string TasksFilename { get; set; } = "tasks.json";

        public bool LoadLastProjectOnStartup { get; set; } = true;
    }
}
