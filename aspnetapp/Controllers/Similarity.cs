using System;
using System.Collections.Generic;
using System.Globalization;

namespace aspnetapp.Controllers
{
    public readonly struct Similarity : IEquatable<Similarity>, IComparable<Similarity>
    {
        public readonly double Value;

        public Similarity(double value)
        {
            Value = value;
        }

        public bool Equals(Similarity other) => Value.Equals(other.Value);

        public override bool Equals(object obj)
        {
            return obj is Similarity other && Equals(other);
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static bool operator ==(Similarity left, Similarity right) => left.Equals(right);

        public static bool operator !=(Similarity left, Similarity right) => !left.Equals(right);

        public int CompareTo(Similarity other) => Value.CompareTo(other.Value);

        public static bool operator <(Similarity left, Similarity right) => left.CompareTo(right) < 0;

        public static bool operator >(Similarity left, Similarity right) => left.CompareTo(right) > 0;

        public static bool operator <=(Similarity left, Similarity right) => left.CompareTo(right) <= 0;

        public static bool operator >=(Similarity left, Similarity right) => left.CompareTo(right) >= 0;

        public static Similarity Of(Account me, Account other, Span<uint> accounts, out uint count)
        {
            double result = 0;

            uint meIndex = 0;
            uint otherIndex = 0;

            count = 0;
            Like[] meLikes = me.likes.Items;
            Like[] otherLikes = other.likes.Items;

            //TODO: Try to optimize using border checks taking into account min and max ids. and binary search.
            uint meCount = (uint) me.likes.Count;

            uint otherCount = (uint) other.likes.Count;
            while (meIndex < meCount && otherIndex < otherCount)
            {
                ref Like meLike = ref meLikes[meIndex];
                ref Like otherLike = ref otherLikes[otherIndex];

                //set positions to the same like id

                if (otherLike.Id < meLike.Id)
                {
                    meIndex++;
                    continue;
                }

                if(meLike.Id < otherLike.Id)
                {
                    accounts[(int) count++] = otherLike.Id;
                    otherIndex++;
                    continue;
                }
                //TODO: Got rid of division
                result += 1.0 / Math.Abs(meLike.AvgTs - otherLike.AvgTs);
                meIndex++;
                otherIndex++;
            }

            while (otherIndex < otherCount)
            {
                accounts[(int) count++] = otherLikes[otherIndex++].Id;
            }

            return new Similarity(result);
        }

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }
}