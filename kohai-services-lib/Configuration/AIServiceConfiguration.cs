using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Kohai.Attributes;

namespace Kohai.Configuration
{
    /// <summary>
    /// Configuration for the AI Service.
    /// </summary>
    [ServiceConfiguration("AI")]
    public class AIServiceConfiguration : IConfiguration
    {
        [RequiredConfiguration]
        public string? ChatGPTApiKey { get; set; }

        [RequiredConfiguration]
        public string? ChatGPTApiUrl { get; set; }

        public string? ChatGPTAssistantAI { get; set; }

        public int ChatGPTRetryMaxAttempts { get; set; } = 50;

        public int ChatGPTRetryInterval { get; set; } = 200;
    }
}
