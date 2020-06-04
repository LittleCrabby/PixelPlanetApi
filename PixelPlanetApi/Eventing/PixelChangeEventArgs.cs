using System;
using PixelPlanetApi.Models;

namespace PixelPlanetApi.Eventing
{
    public class PixelChangeEventArgs : EventArgs
    {
        public Pixel Pixel { get; set; } = new Pixel();
        public byte CanvasId { get; set; }
    }
}