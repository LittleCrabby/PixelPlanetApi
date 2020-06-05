using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixelPlanetApi.Models
{
    public class MeResponse
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("dailyRanking")]
        public int DailyRanking { get; set; }

        [JsonProperty("dailyTotalPixels")]
        public int DailyTotalPixels { get; set; }

        [JsonProperty("ranking")]
        public int Ranking { get; set; }

        [JsonProperty("totalPixels")]
        public int TotalPixels { get; set; }

        [JsonProperty("canvases")]
        public IDictionary<byte, CanvasResponse> Canvases { get; set; } = new Dictionary<byte, CanvasResponse>();
    }
}