using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using aspnetapp.Collections;

namespace aspnetapp.Controllers
{
    [StructLayout(LayoutKind.Explicit)]
    public readonly struct GroupKey : IEquatable<GroupKey>, IComparable<GroupKey>
    {
        [FieldOffset(0)] public readonly SexStatus SexStatus;
        [FieldOffset(2)] public readonly ushort InterestIndex;

        [FieldOffset(4)] private readonly int _cityCountry;
        [FieldOffset(4)] public readonly ushort CountryIndex;
        [FieldOffset(6)] public readonly ushort CityIndex;

        [FieldOffset(0)] private readonly long _dummy;
        public GroupKey(SexStatus sexStatus, ushort interestIndex, int cityCountry)
        {
            _dummy = 0; //will be overwritten by the further
            CountryIndex = 0;
            CityIndex = 0;
            _cityCountry = cityCountry;
            SexStatus = sexStatus;
            InterestIndex = interestIndex;

        }

        public GroupKey(SexStatus sexStatus, ushort interestIndex, ushort countryIndex, ushort cityIndex)
        {
            _dummy = 0; //will be overwritten by the further
            _cityCountry = 0;
            SexStatus = sexStatus;
            InterestIndex = interestIndex;
            CountryIndex = countryIndex;
            CityIndex = cityIndex;
        }

        public bool Equals(GroupKey other) => _dummy == other._dummy;

        public int CompareTo(GroupKey other) => _dummy.CompareTo(other._dummy);

        public override bool Equals(object obj) => obj is GroupKey other && Equals(other);

        public override int GetHashCode() => _dummy.GetHashCode();

        public static readonly GroupKeyEqualityComparer Comparer = new GroupKeyEqualityComparer();

        public static bool operator ==(GroupKey left, GroupKey right) => left.Equals(right);

        public static bool operator !=(GroupKey left, GroupKey right) => !left.Equals(right);

        public sealed class GroupKeyEqualityComparer : IEqualityComparer<GroupKey>
        {
            public bool Equals(GroupKey x, GroupKey y) => x.Equals(y);

            public int GetHashCode(GroupKey obj) => obj.GetHashCode();
        }

        public string this[GroupKeys key]
        {
            get {
                switch (key)
                {
                    case GroupKeys.Sex:
                        return SexStatus.ToNullableSexString();
                    case GroupKeys.Status:
                        return SexStatus.ToNullableStatusString();
                    case GroupKeys.Interests:
                        return StringIndexer.Interests[InterestIndex];
                    case GroupKeys.City:
                        return StringIndexer.Cities[CityIndex];
                    case GroupKeys.Country:
                        return StringIndexer.Countries[CountryIndex];
                }
                throw new InvalidOperationException("This should never happen");
            }
        }
    }
}