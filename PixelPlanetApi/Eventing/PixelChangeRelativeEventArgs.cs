using System;

namespace PixelPlanetApi.Eventing
{
    internal class PixelChangeRelativeEventArgs : EventArgs
    {
        public byte CX { get; set; }
        public byte CY { get; set; }
        public byte? RZ { get; set; }
        public byte RY { get; set; }
        public byte RX { get; set; }
        public byte Color { get; set; }
        public byte Canvas { get; set; }
    }
}