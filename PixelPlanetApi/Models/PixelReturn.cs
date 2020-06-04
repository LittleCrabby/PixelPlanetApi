using PixelPlanetApi.Enums;

namespace PixelPlanetApi.Models
{
    public class PixelReturn
    {
        public ReturnCode ReturnCode { get; set; }
        public uint WaitSeconds { get; set; }
        public short CoolDownSeconds { get; set; }
    }
}