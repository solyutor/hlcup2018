using System;
using System.Runtime.InteropServices;

namespace aspnetapp.Domain
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RecommendIndexEntry : IComparable<RecommendIndexEntry>, IEquatable<RecommendIndexEntry>
    {
        public const int IdMask = 0x00FFFFFF;

        [FieldOffset(0)]
        private readonly uint _all;

        [FieldOffset(3)]
        private readonly byte _premiumStatus;

        public uint Id => _all & IdMask;

        public bool HasPremium => _premiumStatus >= 32;

        public SexStatus Status => (SexStatus)_premiumStatus & SexStatus.AllStatus;


        public RecommendIndexEntry(Account account)
        {
            _all = account.id;
            _premiumStatus = (byte)(account.SexStatus & SexStatus.AllStatus);

            if (account.HasPremium())
            {
                _premiumStatus |= 32;
            }
        }

        public int CompareTo(RecommendIndexEntry other) => _all.CompareTo(other._all);

        public bool Equals(RecommendIndexEntry other) => _all == other._all;

        public override bool Equals(object obj)
        {
            return obj is RecommendIndexEntry other && Equals(other);
        }

        public override int GetHashCode() => (int) _all;

        public static bool operator ==(RecommendIndexEntry left, RecommendIndexEntry right) => left.Equals(right);

        public static bool operator !=(RecommendIndexEntry left, RecommendIndexEntry right) => !left.Equals(right);

        public static bool operator <(RecommendIndexEntry left, RecommendIndexEntry right) => left.CompareTo(right) < 0;

        public static bool operator >(RecommendIndexEntry left, RecommendIndexEntry right) => left.CompareTo(right) > 0;

        public static bool operator <=(RecommendIndexEntry left, RecommendIndexEntry right) => left.CompareTo(right) <= 0;

        public static bool operator >=(RecommendIndexEntry left, RecommendIndexEntry right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{Id}: P={HasPremium} S={Status}";
    }
}