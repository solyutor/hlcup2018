using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace aspnetapp
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct LikeLink : IEquatable<LikeLink>
    {
        [FieldOffset(0)]
        private readonly int _liker;
        [FieldOffset(4)]
        private readonly int _likee;

        [FieldOffset(0)]
        private readonly long _dummy;

        public LikeLink(int liker, int likee)
        {
            _dummy = 0;
            _liker = liker;
            _likee = likee;
        }

        public bool Equals(LikeLink other) => _dummy == other._dummy;

        public override bool Equals(object obj) => obj is LikeLink other && Equals(other);

        public override int GetHashCode() => _dummy.GetHashCode();

        public static bool operator ==(LikeLink left, LikeLink right) => left.Equals(right);

        public static bool operator !=(LikeLink left, LikeLink right) => !left.Equals(right);

        public sealed class LikeLinkComparer : IEqualityComparer<LikeLink>
        {
            public bool Equals(LikeLink x, LikeLink y) => x._dummy == y._dummy;

            public int GetHashCode(LikeLink obj) => obj._dummy.GetHashCode();
        }

        public static readonly LikeLinkComparer Comparer  = new LikeLinkComparer();


    }
}