using System.Collections.Generic;
using Newtonsoft.Json;

namespace PixelPlanetApi.Models
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public class MeResponse
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("dailyRanking")]
        public int DailyRanking { get; set; }

        [JsonProperty("dailyTotalPixels")]
        public int DailyTotalPixels { get; set; }

        [JsonProperty("ranking")]
        public int Ranking { get; set; }

        [JsonProperty("totalPixels")]
        public int TotalPixels { get; set; }

        [JsonProperty("canvases")]
        public Dictionary<byte, CanvasResponse> Canvases { get; set; }
    }
}