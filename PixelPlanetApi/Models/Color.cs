using System;

namespace PixelPlanetApi.Models
{
    public struct Color : IEquatable<Color>
    {
        public readonly byte r;
        public readonly byte g;
        public readonly byte b;

        public Color(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        public static bool operator ==(Color lhs, Color rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Color lhs, Color rhs)
        {
            return !lhs.Equals(rhs);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Color?);
        }

        public bool Equals(Color other)
        {
            return (r == other.r) && (g == other.g) && (b == other.b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + r.GetHashCode();
                hash = hash * 31 + g.GetHashCode();
                hash = hash * 31 + b.GetHashCode();
                return hash;
            }
        }
    }
}