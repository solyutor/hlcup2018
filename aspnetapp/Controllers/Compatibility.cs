using System;
using System.Runtime.InteropServices;
using aspnetapp.Domain;

namespace aspnetapp.Controllers
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Compatibility : IEquatable<Compatibility>, IComparable<Compatibility>
    {
        [FieldOffset(0)]
        private  ulong _value;

        [FieldOffset(0)]
        private  int _ageDiff;

        [FieldOffset(5)]
        private  byte _interests;
        [FieldOffset(6)]
        private  SexStatus _status;
        [FieldOffset(7)]
        private  byte _premium;

        public bool HasPremium => _premium != 0;
        public SexStatus Status => _status;


        public static Compatibility Of(Account account, Account candidate, int sharedInterests, RecommendIndexEntry entry)
        {
            Compatibility result = default;

            result._premium = (byte) (entry.HasPremium ? 1 : 0);

            result._status =  entry.Status;
            result._interests = (byte) sharedInterests;
            result._ageDiff = (int.MaxValue - Math.Abs(account.birth - candidate.birth));
            return result;
        }

        public static Compatibility Of(Account account, Account candidate)
        {
            Compatibility result = default;
            byte sharedInterests = (byte) account.InterestIndexes.IntersectCount(candidate.InterestIndexes);

            if (sharedInterests == 0)
            {
                return default;
            }

            result._premium = (byte) (candidate.HasPremium() ? 1 : 0);

            result._status = candidate.SexStatus & SexStatus.AllStatus;
            result._interests = sharedInterests;
            result._ageDiff = (int.MaxValue - Math.Abs(account.birth - candidate.birth));
            return result;
        }


        public bool Equals(Compatibility other) => _value == other._value;


        public override bool Equals(object obj) => obj is Compatibility other && Equals(other);

        public override int GetHashCode() => _value.GetHashCode();

        public static bool operator ==(Compatibility left, Compatibility right) => left.Equals(right);

        public static bool operator !=(Compatibility left, Compatibility right) => !left.Equals(right);

        public static bool operator <(Compatibility left, ulong right) => left._value < right;

        public static bool operator >(Compatibility left, ulong right) => left._value > right;

        public int CompareTo(Compatibility other) => _value.CompareTo(other._value);

        public override string ToString() => $"P={_premium}, S={_status}, I={_interests} A={_ageDiff:N0}";



    }
}