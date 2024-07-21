using System;
using Newtonsoft.Json;

namespace client_app
{
    [JsonObject()]
    public class ProjectData
    {
        [JsonProperty("id")]
        public Guid ID { get; set; }

        [JsonProperty("name")]
        public string? DisplayName { get; set; }

        [JsonProperty("author")]
        public string? Author { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("created_at")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? LastUpdatedAt { get; set; }

        [JsonProperty("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        public ProjectData()
        {
            this.ID = Guid.NewGuid();

            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = CreatedAt;
        }

        public ProjectData(Guid ID)
        {
            this.ID = ID;

            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = CreatedAt;
        }
    }
}
