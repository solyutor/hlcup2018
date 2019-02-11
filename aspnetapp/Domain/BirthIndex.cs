using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class BirthIndex
    {
        private static readonly HList<uint> Empty = new HList<uint>();
        private static readonly HList<uint>[] ByYear = new HList<uint>[57];

        public static void Index(Account account)
        {
            var offset = GetOffset(account);
            ref HList<uint> list = ref ByYear[offset];
            if (list == null)
            {
                list = new HList<uint>();
            }
            list.InsertDescending(account.id);
        }

        public static HList<uint> GetList(ushort year) => ByYear[(ushort)(year - TimeStamp.MinBirthYear)] ?? Empty;

        public static void Trim()
        {
            foreach (var list in ByYear)
            {
                list?.TrimExcess();
            }
        }

        public static void Remove(Account account)
        {
            var offset = GetOffset(account);
            ref HList<uint> list = ref ByYear[offset];
            list.RemoveDescending(account.id);
        }

        private static ushort GetOffset(Account account)
        {
            return (ushort) (account.GetBirthYear() - TimeStamp.MinBirthYear);
        }
    }
}