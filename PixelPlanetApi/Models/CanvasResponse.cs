using Newtonsoft.Json;

namespace PixelPlanetApi.Models
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class CanvasResponse
    {
        [JsonProperty("ident")]
        public string Identifier { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("colors")]
        public byte[][] Colors { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("bcd")]
        public int BaseCooldown { get; set; }

        [JsonProperty("pcd")]
        public int PlacedCooldown { get; set; }

        [JsonProperty("cds")]
        public int WaitTime { get; set; }

        [JsonProperty("ranked")]
        public bool Ranked { get; set; }

        [JsonProperty("req")]
        public int Requirement { get; set; }

        [JsonProperty("sd")]
        public string StartDate { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("v")]
        public bool Is3d { get; set; }
    }
}