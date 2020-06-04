using Newtonsoft.Json;

namespace PixelPlanetApi.Models
{
    public class CaptchaUpdate
    {
        [JsonProperty("token")]
        public string? Token { get; set; }
    }
}