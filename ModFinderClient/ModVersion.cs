using System;
using System.Text.RegularExpressions;

namespace ModFinder_WOTR
{
    public struct ModVersion : IComparable<ModVersion>, IEquatable<ModVersion>
    {
        public int Major, Minor, Patch;
        public char Suffix;

        public bool Valid => !(Major == 0 && Minor == 0 && Patch == 0);

        public static bool operator <(ModVersion left, ModVersion right)
        {
            return ((IComparable<ModVersion>)left).CompareTo(right) < 0;
        }

        public static bool operator <=(ModVersion left, ModVersion right)
        {
            return ((IComparable<ModVersion>)left).CompareTo(right) <= 0;
        }

        public static bool operator >(ModVersion left, ModVersion right)
        {
            return ((IComparable<ModVersion>)left).CompareTo(right) > 0;
        }

        public static bool operator >=(ModVersion left, ModVersion right)
        {
            return ((IComparable<ModVersion>)left).CompareTo(right) >= 0;
        }

        public static ModVersion Parse(string raw)
        {
            Regex extractVersion = new(@"[^\d]*(\d+)[^\d]*(\d+)[^\d]*(\d*)(.*)");
            var match = extractVersion.Match(raw);
            ModVersion version;
            version.Major = int.Parse(match.Groups[1].Value);
            version.Minor = int.Parse(match.Groups[2].Value);
            version.Patch = int.Parse(match.Groups[3].Length > 0 ? match.Groups[3].Value : "0");
            version.Suffix = (match.Groups[4].Success && match.Groups[4].Length == 1) ? match.Groups[4].Value[0] : default;
            return version;
        }

        public int CompareTo(ModVersion other)
        {
            int c;

            c = Major.CompareTo(other.Major);
            if (c != 0) return c;

            c = Minor.CompareTo(other.Minor);
            if (c != 0) return c;

            c = Patch.CompareTo(other.Patch);
            if (c != 0) return c;

            c = Suffix.CompareTo(other.Suffix);
            return c;
        }

        public override string ToString()
        {
            if (!Valid)
                return "-";
            var normal = $"{Major}.{Minor}.{Patch}";
            if (Suffix != default)
                normal += Suffix;
            return normal;
        }

        public override bool Equals(object obj)
        {
            if (obj is ModVersion other)
                return CompareTo(other) == 0;
            return false;
        }

        public bool Equals(ModVersion other)
        {
            return Major == other.Major &&
                   Minor == other.Minor &&
                   Patch == other.Patch &&
                   Suffix == other.Suffix;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, Suffix);
        }

        public static bool operator ==(ModVersion left, ModVersion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ModVersion left, ModVersion right)
        {
            return !(left == right);
        }
    }
}

