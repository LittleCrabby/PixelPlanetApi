using System;
using System.Collections.Generic;
using System.Linq;
using PixelPlanetApi.Models;

namespace PixelPlanetApi
{
    /// <summary>
    /// Canvas values and helper methods
    /// </summary>
    public class Canvas
    {
        /// <summary>
        /// Gets the canvas index.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public byte Index { get; }

        /// <summary>
        /// Gets the canvas identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public char Identifier { get; }

        /// <summary>
        /// Gets the canvas name.
        /// </summary>
        /// <value>
        /// Canvas name.
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets the base cooldown for clean pixels.
        /// </summary>
        /// <value>
        /// The base cooldown.
        /// </value>
        public int BaseCooldown { get; }

        /// <summary>
        /// Gets the cooldown when replacing existing pixel.
        /// </summary>
        /// <value>
        /// The placed cooldown.
        /// </value>
        public int PlacedCooldown { get; }

        /// <summary>
        /// Gets the wait time after cooldown limit reached.
        /// </summary>
        /// <value>
        /// The wait time.
        /// </value>
        public int WaitTime { get; }

        /// <summary>
        /// Gets the size of the canvas.
        /// </summary>
        /// <value>
        /// The size of the canvas.
        /// </value>
        public int CanvasSize { get; }

        /// <summary>
        /// Gets the dimension of the single chunk.
        /// </summary>
        /// <value>
        /// The size of chunk.
        /// </value>
        public int ChunkSize { get; }

        /// <summary>
        /// Gets the number of pixels placed required for this <see cref="Canvas"/>.
        /// </summary>
        /// <value>
        /// The requirement.
        /// </value>
        public int Requirement { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Canvas"/> is 3d.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 3d
        /// </value>
        public bool Is3d { get; }

        private Color[] _palette = Array.Empty<Color>();

        /// <summary>
        /// Gets the canvas colors array. Each color is a tuple in format (r, g, b)
        /// </summary>
        /// <value>
        /// The palette.
        /// </value>
        public Color[] Palette
        {
            get => (Color[])_palette.Clone();
            private set => _palette = value;
        }

        internal Canvas(KeyValuePair<byte, CanvasResponse> canvas)
        {
            Index = canvas.Key;
            Identifier = canvas.Value.Identifier;
            Name = canvas.Value.Title;
            BaseCooldown = canvas.Value.BaseCooldown;
            PlacedCooldown = canvas.Value.PlacedCooldown;
            WaitTime = canvas.Value.WaitTime;
            ChunkSize = canvas.Value.Is3d ? 32 : 256;
            CanvasSize = canvas.Value.Size;
            Requirement = canvas.Value.Requirement;
            Is3d = canvas.Value.Is3d;

            Palette = canvas.Value.Colors.Select(x => new Color(x[0], x[1], x[2])).ToArray();
        }

        /// <summary>
        /// Gets the chunk of pixel.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="z">Z - only for 3D canvas.</param>
        /// <returns>Chunk coordinates tuple.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Coordinates are out of canvas bounds</exception>
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

        /// <summary>
        /// Gets the offset of pixel. Offset is the count of pixels from the start of chunk.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="z">Z - only for 3D canvas.</param>
        /// <returns>Pixels offset.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Coordinates are out of canvas bounds</exception>
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

        /// <summary>
        /// Gets the absolute coordinate.
        /// </summary>
        /// <param name="c">Canvas coordinate.</param>
        /// <param name="r">Relative to the canvas coordinate.</param>
        /// <returns>Absolute coordinate</returns>
        public short GetAbsoluteCoordinate(byte c, byte r)
        {
            return (short)(r + c * ChunkSize - CanvasSize / 2);
        }

        /// <summary>
        /// Gets the chunks for <see cref="Area"/>.
        /// </summary>
        /// <param name="area">The <see cref="Area"/>.</param>
        /// <returns>Chunks coordinates HashSet</returns>
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