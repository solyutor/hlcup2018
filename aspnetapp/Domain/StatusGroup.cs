using System;
using System.Collections.Generic;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public readonly struct StatusGroup
    {
        public int TotalMen => Free.MenCount + NotFree.MenCount + Complex.MenCount;
        public int TotalWomen => Free.WomenCount + NotFree.WomenCount + Complex.WomenCount;

        public int TotalFree => Free.MenCount + Free.WomenCount;
        public int TotalNotFree => NotFree.MenCount + NotFree.WomenCount;
        public int TotalComplex => Complex.MenCount + Complex.WomenCount;

        public int TotalAll => TotalMen + TotalWomen;
        public bool IsEmpty => Free.IsEmpty;

        public int this[SexStatus index]
        {
            get
            {
                switch (index)
                {
                    case SexStatus.None:
                        return TotalAll;
                    case SexStatus.Male:
                        return TotalMen;
                    case SexStatus.Female:
                        return TotalWomen;
                    case SexStatus.Free:
                        return TotalFree;
                    case SexStatus.NotFree:
                        return TotalNotFree;
                    case SexStatus.Complex:
                        return TotalComplex;

                    case SexStatus.Male | SexStatus.Free:
                        return Free.MenCount;
                    case SexStatus.Male | SexStatus.NotFree:
                        return NotFree.MenCount;
                    case SexStatus.Male | SexStatus.Complex:
                        return Complex.MenCount;

                    case SexStatus.Female | SexStatus.Free:
                        return Free.WomenCount;
                    case SexStatus.Female | SexStatus.NotFree:
                        return NotFree.WomenCount;
                    case SexStatus.Female | SexStatus.Complex:
                        return Complex.WomenCount;
                }

                return 0;
            }
        }


        public readonly SexGroup Free;
        public readonly SexGroup NotFree;
        public readonly SexGroup Complex;

        public StatusGroup(int size)
        {
            Free = new SexGroup(size);
            NotFree = new SexGroup(size);
            Complex = new SexGroup(size);
        }

        public void Trim()
        {
            Free.Trim();
            NotFree.Trim();
            Complex.Trim();
        }

        public IEnumerable<uint> GetListBy(SexStatus sexStatus, UintMultiUnionList union)
        {
            switch (sexStatus)
            {
                case SexStatus.Male | SexStatus.Free:
                    return Free.Men.All;
                case SexStatus.Male | SexStatus.NotFree:
                    return NotFree.Men.All;
                case SexStatus.Male | SexStatus.Complex:
                    return Complex.Men.All;
                case SexStatus.Female | SexStatus.Free:
                    return Free.Women.All;
                case SexStatus.Female | SexStatus.NotFree:
                    return NotFree.Women.All;
                case SexStatus.Female | SexStatus.Complex:
                    return Complex.Women.All;

                case SexStatus.Male:
                    return union.Add(Free.Men.All).Add(NotFree.Men.All).Add(Complex.Men.All);
                case SexStatus.Female:
                    return union.Add(Free.Women.All).Add(NotFree.Women.All).Add(Complex.Women.All);

                case SexStatus.Male | SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Men.All).Add(NotFree.Men.All);
                case SexStatus.Female | SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Women.All).Add(NotFree.Women.All);

                case SexStatus.Male | SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Men.All).Add(Complex.Men.All);
                case SexStatus.Female | SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Women.All).Add(Complex.Women.All);

                case SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Men.All).Add(Free.Women.All).Add(NotFree.Men.All).Add(NotFree.Women.All);
                case SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Men.All).Add(Free.Women.All).Add(Complex.Men.All).Add(Complex.Women.All);
                case SexStatus.Complex | SexStatus.NotFree:
                    return union.Add(Complex.Men.All).Add(Complex.Women.All).Add(NotFree.Men.All).Add(NotFree.Women.All);

                case SexStatus.Free:
                    return union.Add(Free.Men.All).Add(Free.Women.All);
                case SexStatus.NotFree:
                    return union.Add(NotFree.Men.All).Add(NotFree.Women.All);
                case SexStatus.Complex:
                    return union.Add(Complex.Men.All).Add(Complex.Women.All);


                case SexStatus.Male | SexStatus.NotFree | SexStatus.Complex:
                    return union.Add(NotFree.Men.All).Add(Complex.Men.All);
                case SexStatus.Female | SexStatus.NotFree | SexStatus.Complex:
                    return union.Add(NotFree.Women.All).Add(Complex.Women.All);

                case SexStatus.Male | SexStatus.AllStatus:
                    return union.Add(Free.Men.All).Add(NotFree.Men.All).Add(Complex.Men.All);
                case SexStatus.Female | SexStatus.AllStatus:
                    return union.Add(Free.Women.All).Add(NotFree.Women.All).Add(Complex.Women.All);
            }
            throw new InvalidOperationException(sexStatus.ToString());
        }


        public IEnumerable<uint> GetListBy(SexStatus sexStatus, ushort birthYear, UintMultiUnionList union)
        {
            switch (sexStatus)
            {
                case SexStatus.Male | SexStatus.Free:
                    return Free.Men.Born(birthYear);
                case SexStatus.Male | SexStatus.NotFree:
                    return NotFree.Men.Born(birthYear);
                case SexStatus.Male | SexStatus.Complex:
                    return Complex.Men.Born(birthYear);
                case SexStatus.Female | SexStatus.Free:
                    return Free.Women.Born(birthYear);
                case SexStatus.Female | SexStatus.NotFree:
                    return NotFree.Women.Born(birthYear);
                case SexStatus.Female | SexStatus.Complex:
                    return Complex.Women.Born(birthYear);

                case SexStatus.Male:
                    return union.Add(Free.Men.Born(birthYear)).Add(NotFree.Men.Born(birthYear)).Add(Complex.Men.Born(birthYear));
                case SexStatus.Female:
                    return union.Add(Free.Women.Born(birthYear)).Add(NotFree.Women.Born(birthYear)).Add(Complex.Women.Born(birthYear));

                case SexStatus.Free:
                    return union.Add(Free.Men.Born(birthYear)).Add(Free.Women.Born(birthYear));
                case SexStatus.NotFree:
                    return union.Add(NotFree.Men.Born(birthYear)).Add(NotFree.Women.Born(birthYear));
                case SexStatus.Complex:
                    return union.Add(Complex.Men.Born(birthYear)).Add(Complex.Women.Born(birthYear));


                case SexStatus.Male | SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Men.Born(birthYear)).Add(NotFree.Men.Born(birthYear));
                case SexStatus.Female | SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Women.Born(birthYear)).Add(NotFree.Women.Born(birthYear));

                case SexStatus.Male | SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Men.Born(birthYear)).Add(Complex.Men.Born(birthYear));
                case SexStatus.Female | SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Women.Born(birthYear)).Add(Complex.Women.Born(birthYear));

                case SexStatus.Free | SexStatus.NotFree:
                    return union.Add(Free.Men.Born(birthYear)).Add(Free.Women.Born(birthYear)).Add(NotFree.Men.Born(birthYear)).Add(NotFree.Women.Born(birthYear));
                case SexStatus.Free | SexStatus.Complex:
                    return union.Add(Free.Men.Born(birthYear)).Add(Free.Women.Born(birthYear)).Add(Complex.Men.Born(birthYear)).Add(Complex.Women.Born(birthYear));
                case SexStatus.Complex | SexStatus.NotFree:
                    return union.Add(Complex.Men.Born(birthYear)).Add(Complex.Women.Born(birthYear)).Add(NotFree.Men.Born(birthYear)).Add(NotFree.Women.Born(birthYear));


                case SexStatus.Male | SexStatus.NotFree | SexStatus.Complex:
                    return union.Add(NotFree.Men.Born(birthYear)).Add(Complex.Men.Born(birthYear));
                case SexStatus.Female | SexStatus.NotFree | SexStatus.Complex:
                    return union.Add(NotFree.Women.Born(birthYear)).Add(Complex.Women.Born(birthYear));
            }
            throw new InvalidOperationException(sexStatus.ToString());
        }

        public int Joined(ushort joinedYear, SexStatus sexStatus)
        {
            switch (sexStatus)
            {
                case SexStatus.Male:
                    return Free.JoinedMen(joinedYear) + NotFree.JoinedMen(joinedYear) + Complex.JoinedMen(joinedYear);
                case SexStatus.Female:
                    return Free.JoinedWomen(joinedYear) + NotFree.JoinedWomen(joinedYear) + Complex.JoinedWomen(joinedYear);

                case SexStatus.Free:
                    return Free.JoinedMen(joinedYear) + Free.JoinedWomen(joinedYear);
                case SexStatus.NotFree:
                    return NotFree.JoinedMen(joinedYear) + NotFree.JoinedWomen(joinedYear);
                case SexStatus.Complex:
                    return Complex.JoinedMen(joinedYear) + Complex.JoinedWomen(joinedYear);


                case SexStatus.Male | SexStatus.Free:
                    return Free.JoinedMen(joinedYear);
                case SexStatus.Male | SexStatus.NotFree:
                    return NotFree.JoinedMen(joinedYear);
                case SexStatus.Male | SexStatus.Complex:
                    return Complex.JoinedMen(joinedYear);


                case SexStatus.Female | SexStatus.Free:
                    return Free.JoinedWomen(joinedYear);
                case SexStatus.Female | SexStatus.NotFree:
                    return NotFree.JoinedWomen(joinedYear);
                case SexStatus.Female | SexStatus.Complex:
                    return Complex.JoinedWomen(joinedYear);
                case SexStatus.None:
                    return Free.JoinedMen(joinedYear) + NotFree.JoinedMen(joinedYear) + Complex.JoinedMen(joinedYear)
                           + Free.JoinedWomen(joinedYear) + NotFree.JoinedWomen(joinedYear) + Complex.JoinedWomen(joinedYear);

            }
            return 0;
        }


        public int Born(ushort birthYear, SexStatus sexStatus)
        {
            switch (sexStatus)
            {
                case SexStatus.Male:
                    return Free.BornMen(birthYear) + NotFree.BornMen(birthYear) + Complex.BornMen(birthYear);
                case SexStatus.Female:
                    return Free.BornWomen(birthYear) + NotFree.BornWomen(birthYear) + Complex.BornWomen(birthYear);

                case SexStatus.Free:
                    return Free.BornMen(birthYear) + Free.BornWomen(birthYear);
                case SexStatus.NotFree:
                    return NotFree.BornMen(birthYear) + NotFree.BornWomen(birthYear);
                case SexStatus.Complex:
                    return Complex.BornMen(birthYear) + Complex.BornWomen(birthYear);


                case SexStatus.Male | SexStatus.Free:
                    return Free.BornMen(birthYear);
                case SexStatus.Male | SexStatus.NotFree:
                    return NotFree.BornMen(birthYear);
                case SexStatus.Male | SexStatus.Complex:
                    return Complex.BornMen(birthYear);


                case SexStatus.Female | SexStatus.Free:
                    return Free.BornWomen(birthYear);
                case SexStatus.Female | SexStatus.NotFree:
                    return NotFree.BornWomen(birthYear);
                case SexStatus.Female | SexStatus.Complex:
                    return Complex.BornWomen(birthYear);
                case SexStatus.None:
                    return Free.BornMen(birthYear) + NotFree.BornMen(birthYear) + Complex.BornMen(birthYear)
                           + Free.BornWomen(birthYear) + NotFree.BornWomen(birthYear) + Complex.BornWomen(birthYear);

            }
            return 0;
        }

        public int Interests(ushort interest, SexStatus sexStatus, ushort joinedYear, ushort birthYear)
        {
            switch (sexStatus)
            {
                case SexStatus.Male:
                    return Free.InterestMen(interest, joinedYear, birthYear) + NotFree.InterestMen(interest, joinedYear, birthYear) + Complex.InterestMen(interest, joinedYear, birthYear);
                case SexStatus.Female:
                    return Free.InterestWomen(interest, joinedYear, birthYear) + NotFree.InterestWomen(interest, joinedYear, birthYear) + Complex.InterestWomen(interest, joinedYear, birthYear);

                case SexStatus.Free:
                    return Free.InterestMen(interest, joinedYear, birthYear) + Free.InterestWomen(interest, joinedYear, birthYear);
                case SexStatus.NotFree:
                    return NotFree.InterestMen(interest, joinedYear, birthYear) + NotFree.InterestWomen(interest, joinedYear, birthYear);
                case SexStatus.Complex:
                    return Complex.InterestMen(interest, joinedYear, birthYear) + Complex.InterestWomen(interest, joinedYear, birthYear);


                case SexStatus.Male | SexStatus.Free:
                    return Free.InterestMen(interest, joinedYear, birthYear);
                case SexStatus.Male | SexStatus.NotFree:
                    return NotFree.InterestMen(interest, joinedYear, birthYear);
                case SexStatus.Male | SexStatus.Complex:
                    return Complex.InterestMen(interest, joinedYear, birthYear);


                case SexStatus.Female | SexStatus.Free:
                    return Free.InterestWomen(interest, joinedYear, birthYear);
                case SexStatus.Female | SexStatus.NotFree:
                    return NotFree.InterestWomen(interest, joinedYear, birthYear);
                case SexStatus.Female | SexStatus.Complex:
                    return Complex.InterestWomen(interest, joinedYear, birthYear);
                case SexStatus.None:
                    return Free.InterestMen(interest, joinedYear, birthYear) + NotFree.InterestMen(interest, joinedYear, birthYear) + Complex.InterestMen(interest, joinedYear, birthYear)
                           + Free.InterestWomen(interest, joinedYear, birthYear) + NotFree.InterestWomen(interest, joinedYear, birthYear) + Complex.InterestWomen(interest, joinedYear, birthYear);
            }
            return 0;
        }

        public IEnumerable<uint> GetListBy(SexStatus sexStatus, PhoneCode phoneCode, UintMultiUnionList union)
        {
            var codeOffset = PhoneCodeOffset(phoneCode.GetSpan().Slice(1));
            switch (sexStatus & SexStatus.AllSex)
            {
                case SexStatus.Male:
                {
                    Free.MenWithPhoneCode(codeOffset, union);
                    NotFree.MenWithPhoneCode(codeOffset, union);
                    Complex.MenWithPhoneCode(codeOffset, union);
                    return union;
                }
                case SexStatus.Female:
                {
                    Free.WomenWithPhoneCode(codeOffset, union);
                    NotFree.WomenWithPhoneCode(codeOffset, union);
                    Complex.WomenWithPhoneCode(codeOffset, union);
                    return union;
                }
                default:
                {
                    Free.MenWithPhoneCode(codeOffset, union);
                    NotFree.MenWithPhoneCode(codeOffset, union);
                    Complex.MenWithPhoneCode(codeOffset, union);
                    Free.WomenWithPhoneCode(codeOffset, union);
                    NotFree.WomenWithPhoneCode(codeOffset, union);
                    Complex.WomenWithPhoneCode(codeOffset, union);
                    return union;
                }
            }
        }

        private static byte PhoneCodeOffset(Span<byte> codeSpan)
        {
            byte offset = (byte) (((codeSpan[0] - '0') * 10) + ((codeSpan[1] - '0')));
            return offset;
        }
    }
}