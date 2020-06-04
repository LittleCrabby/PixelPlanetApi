using PixelPlanetApi.Enums;
using System;

namespace PixelPlanetApi.Eventing
{
    public class PixelRetrunEventArgs : EventArgs
    {
        public ReturnCode ReturnCode { get; set; }
        public uint WaitSeconds { get; set; }
        public short CoolDownSeconds { get; set; }
    }
}