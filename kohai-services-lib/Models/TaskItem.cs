using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kohai;
using Newtonsoft.Json;

namespace Kohai.Models
{
    public class TaskItem
    {
        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("is_completed")]
        public bool IsCompleted { get; set; }
    }
}
