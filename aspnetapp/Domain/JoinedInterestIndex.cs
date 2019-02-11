using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class JoinedInterestIndex
    {
        private static readonly ushort[] JoinedInterests;
        private static readonly ushort[] BirthInterests;
        static JoinedInterestIndex()
        {
            JoinedInterests = new ushort[10 * 100];
            BirthInterests = new ushort[60 * 100];
        }

        public static void Index(Account account)
        {
            foreach (var interest in account.InterestIndexes)
            {
                JoinedInterests[GetJoinedOffset(account, interest)]++;
                BirthInterests[GetBirthOffset(account, interest)]++;
            }
        }

        public static void Remove(Account account)
        {
            foreach (var interest in account.InterestIndexes)
            {
                JoinedInterests[GetJoinedOffset(account, interest)]--;
                BirthInterests[GetBirthOffset(account, interest)]--;
            }
        }

        public static int GetInterestsJoinedCount(ushort joinedYear, ushort interest)
        {
            return JoinedInterests[GetJoinedOffset(joinedYear, interest)];
        }

        public static int GetInterestsBirthCount(ushort birthYear, ushort interest)
        {
            return BirthInterests[GetBirthOffset(birthYear, interest)];
        }

        private static ushort GetJoinedOffset(Account account, ushort interest)
        {
            var joinedYear = account.GetJoinedYear();
            return GetJoinedOffset(joinedYear, interest);
        }

        private static ushort GetJoinedOffset(ushort joinedYear, ushort interest) => (ushort) ((joinedYear - TimeStamp.MinJoinedYear) * 100 + interest);

        private static int GetBirthOffset(Account account, ushort interest)
        {
            var birthYear = account.GetBirthYear();
            return GetBirthOffset(birthYear, interest);
        }

        private static int GetBirthOffset(ushort birthYear, ushort interest) => (ushort) ((birthYear - TimeStamp.MinBirthYear) * 100 + interest);
    }
}