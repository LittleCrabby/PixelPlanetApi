using System;
using System.Collections.Generic;
using System.Linq;
using PixelPlanetApi.Models;

namespace PixelPlanetApi
{
    public class Canvas
    {
        public byte Id { get; }
        public int BaseCooldown { get; }
        public int PlacedCooldown { get; }
        public int WaitTime { get; }
        public int CanvasSize { get; }
        public int ChunkSize { get; }
        public int Requirement { get; }
        public bool Is3d { get; }

        private (byte, byte, byte)[] _palette = Array.Empty<(byte, byte, byte)>();

        public (byte, byte, byte)[] Palette
        {
            get => ((byte, byte, byte)[])_palette.Clone();
            private set => _palette = value;
        }

        public Canvas(KeyValuePair<byte, CanvasResponse> canvas)
        {
            Id = canvas.Key;
            BaseCooldown = canvas.Value.BaseCooldown;
            PlacedCooldown = canvas.Value.PlacedCooldown;
            WaitTime = canvas.Value.WaitTime;
            ChunkSize = canvas.Value.Is3d ? 32 : 256;
            CanvasSize = canvas.Value.Size;
            Requirement = canvas.Value.Requirement;
            Is3d = canvas.Value.Is3d;

            Palette = canvas.Value.Colors.Select(x => (x[0], x[1], x[2])).ToArray();
        }

        public (byte, byte) GetChunkOfPixel(int x, int y, int? z = default)
        {
            var halfSize = CanvasSize / 2;
            y = z ?? y;

            if (x < -halfSize || x > halfSize || y < -halfSize || y > halfSize)
            {
                throw new ArgumentOutOfRangeException($"Coordinates are out of canvas bounds");
            }

            var cx = (byte)((x + halfSize) / ChunkSize);
            var cy = (byte)((y + halfSize) / ChunkSize);

            return (cx, cy);
        }

        public int GetOffsetOfPixel(int x, int y, int? z = default)
        {
            var halfSize = CanvasSize / 2;
            y = z ?? y;

            if (x < -halfSize || x > halfSize || y < -halfSize || y > halfSize)
            {
                throw new ArgumentOutOfRangeException($"Coordinates are out of canvas bounds");
            }

            var modOffset = halfSize % ChunkSize;

            var rx = (byte)((x + modOffset) % ChunkSize);
            var ry = (byte)((y + modOffset) % ChunkSize);

            return (ry * ChunkSize) + rx;
        }

        public short GetAbsoluteCoordinate(byte c, byte r)
        {
            return (short)(r + c * ChunkSize - CanvasSize / 2);
        }

        public HashSet<(byte, byte)> GetChunksForArea(Area area)
        {
            var chunks = new HashSet<(byte, byte)>();

            var (cx1, cy1) = GetChunkOfPixel(area.X1, area.Y1, area.Z1);
            var (cx2, cy2) = GetChunkOfPixel(area.X2, area.Y2, area.Z2);

            for (var i = cx1; i <= cx2; i++)
            {
                for (var j = cy1; j <= cy2; j++)
                {
                    chunks.Add((i, j));
                }
            }

            return chunks;
        }
    }
}