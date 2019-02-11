using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public readonly struct SexGroup
    {
        public readonly GenderGroup Men;

        public readonly GenderGroup Women;

        public int Total => Men.Count + Women.Count;
        public int MenCount => Men.Count;
        public int WomenCount => Women.Count;
        public bool IsEmpty => Men.IsEmpty;

        public int JoinedMen(ushort year) => Men.JoinedCount(year);

        public int JoinedWomen(ushort year) => Women.JoinedCount(year);

        public int BornMen(ushort birthYear) => Men.BornCount(birthYear);

        public int BornWomen(ushort birthYear) => Women.BornCount(birthYear);

        public int InterestMen(ushort interest, ushort joinedYear, ushort birthYear) => Men.InterestsCount(interest, joinedYear, birthYear);

        public int InterestWomen(ushort interest, ushort joinedYear, ushort birthYear) => Women.InterestsCount(interest, joinedYear, birthYear);


        public SexGroup(int size)
        {
            Men = new  GenderGroup(size);
            Women = new  GenderGroup(size);
        }

        public void Trim()
        {
            Men.TrimExcess();
            Women.TrimExcess();
        }


        public void Index(Account account)
        {
            if (account.SexStatus.IsMale())
            {
                Men.Index(account);
            }
            else
            {
                Women.Index(account);
            }
        }

        public void Remove(Account account)
        {
            if (account.SexStatus.IsMale())
            {
                Men.Remove(account);
            }
            else
            {
                Women.Remove(account);
            }
        }


        public void MenWithPhoneCode(byte codeOffset, UintMultiUnionList union)
        {
            union.Add(Men.ByPhoneCode(codeOffset));
        }

        public void WomenWithPhoneCode(byte codeOffset, UintMultiUnionList union)
        {
            union.Add(Women.ByPhoneCode(codeOffset));
        }
    }
}