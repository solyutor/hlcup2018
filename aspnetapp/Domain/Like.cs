using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using aspnetapp.Controllers;

namespace aspnetapp
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct Like : IEquatable<Like>, IComparable<Like>
    {
        public const int IdMask = 0x00FFFFFF;

        [FieldOffset(0)]
        private readonly uint id;

        [FieldOffset(3)]
        private readonly byte count;
        [FieldOffset(4)]
        public readonly uint totalTs;

        public uint Id => id & IdMask;
        public double AvgTs => (totalTs + TimeStamp.MinJoined * count) / (double)count;

        public byte Count => count;

        public Like(uint likeeId, int ts)
        {
            id = likeeId;
            count = 1;
            totalTs = (uint) (ts - TimeStamp.MinJoined);
        }

        public Like(uint likeeId, IEnumerable<int> tss)
        {
            id = likeeId;
            count = 0;
            totalTs = 0;
            foreach (var ts1 in tss)
            {
                totalTs += (uint) (ts1 - TimeStamp.MinJoined);
                count++;
            }
        }

        private Like(Like like, int ts)
        {
            id = like.id;
            count = (byte) (like.count + 1);
            totalTs = (uint) (like.totalTs + (ts - TimeStamp.MinJoined));
        }

        public bool Equals(Like other) => Id  == other.Id;

        public override bool Equals(object obj) => obj is Like other && Equals(other);

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(Like left, Like right) => left.Equals(right);

        public static bool operator !=(Like left, Like right) => !left.Equals(right);

        //Descending order!
        public int CompareTo(Like other) => other.Id.CompareTo(Id);

        public static bool operator <(Like left, Like right) => left.CompareTo(right) < 0;

        public static bool operator >(Like left, Like right) => left.CompareTo(right) > 0;

        public static bool operator <=(Like left, Like right) => left.CompareTo(right) <= 0;

        public static bool operator >=(Like left, Like right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{Id}:{totalTs}/{count}";


        public sealed class IdEqualityComparer : IEqualityComparer<Like>, IComparer<Like>
        {
            public int Compare(Like x, Like y) => x.Id.CompareTo(y.Id);
            public bool Equals(Like x, Like y) => x.Id == y.Id;

            public int GetHashCode(Like obj) => obj.GetHashCode();
        }

        public static readonly IdEqualityComparer IdComparer = new IdEqualityComparer();

        public static Like IdLookUp(uint accountId)
        {
            return new Like(accountId, 0);
        }

        public Like Add(int ts)
        {
            return new Like(this, ts);
        }
    }
}