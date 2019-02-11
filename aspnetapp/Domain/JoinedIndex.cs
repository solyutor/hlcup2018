using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class JoinedIndex
    {
        private static readonly HList<uint> Empty = new HList<uint>();
        private static readonly HList<uint>[] ByYear = new HList<uint>[10];

        public static void Index(Account account)
        {
            var offset = GetJoinedOffset(account);
            ref HList<uint> list = ref ByYear[offset];
            if (list == null)
            {
                list = new HList<uint>();
            }
            list.InsertDescending(account.id);
        }

        public static void Remove(Account account)
        {
            var offset = GetJoinedOffset(account);
            ref HList<uint> list = ref ByYear[offset];
            list.RemoveDescending(account.id);
        }

        public static HList<uint> GetList(ushort year) => ByYear[(ushort)(year - TimeStamp.MinJoinedYear)] ?? Empty;

        public static void Trim()
        {
            foreach (var list in ByYear)
            {
                list?.TrimExcess();
            }
        }

        public static ushort GetJoinedOffset(this Account account)
        {
            ushort joinedYear = account.GetJoinedYear();
            return GetJoinedOffset(joinedYear);
        }

        public static ushort GetJoinedOffset(this ushort joinedYear) => (ushort) (joinedYear - TimeStamp.MinJoinedYear);
    }
}