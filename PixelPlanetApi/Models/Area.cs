namespace PixelPlanetApi.Models
{
    public class Area
    {
        public short X1 { get; set; }
        public short Y1 { get; set; }
        public short? Z1 { get; set; }
        public short X2 { get; set; }
        public short Y2 { get; set; }
        public short? Z2 { get; set; }

        public byte CanvasId { get; set; }

        public bool Contains(short x, short y, short? z)
        {
            return x >= X1 && x <= X2 &&
                y >= Y1 && y <= Y2 &&
                (z == null || z >= Z1 && z <= Z2);
        }
    }
}