using System.Runtime.InteropServices;
using aspnetapp.Collections;

namespace aspnetapp.Domain
{
    [StructLayout(LayoutKind.Auto, Pack = 1)]
    public readonly struct LikeeIndexEntry
    {
        public readonly byte Women;
        public readonly byte Free;
        public readonly byte NotFree;
        public readonly byte Complex;
        public readonly byte Men;
        public readonly HList<uint> Likers;

        public LikeeIndexEntry(HList<uint> likers, byte men, byte women, byte free, byte notFree, byte complex)
        {
            this.Women = women;
            Free = free;
            NotFree = notFree;
            Complex = complex;
            Men = men;
            Likers = likers;

        }

        public void Trim() => Likers?.TrimExcess();

        public LikeeIndexEntry Add(Account liker)
        {
            byte men = Men;
            byte women = Women;
            byte free = Free;
            byte notFree = NotFree;
            byte complex = Complex;

            if (Likers.InsertDescending(liker.id))
            {
                if (liker.SexStatus.IsMale())
                {
                    men += 1;
                }
                else
                {
                    women += 1;
                }

                switch (liker.SexStatus & SexStatus.AllStatus)
                {
                    case SexStatus.Free:
                        free++;
                        break;
                    case SexStatus.NotFree:
                        notFree++;
                        break;
                    case SexStatus.Complex:
                        complex++;
                        break;
                }
            }

            return new LikeeIndexEntry(Likers, men, women, free, notFree, complex);
        }

        public LikeeIndexEntry Remove(Account liker)
        {

            byte men = Men;
            byte women = Women;
            byte free = Free;
            byte notFree = NotFree;
            byte complex = Complex;

            if (Likers.RemoveDescending(liker.id))
            {
                if (liker.SexStatus.IsMale())
                {
                    men -= 1;
                }
                else
                {
                    women -= 1;
                }

                switch (liker.SexStatus & SexStatus.AllStatus)
                {
                    case SexStatus.Free:
                        free--;
                        break;
                    case SexStatus.NotFree:
                        notFree--;
                        break;
                    case SexStatus.Complex:
                        complex--;
                        break;
                }
            }

            return new LikeeIndexEntry(Likers, men, women, free, notFree, complex);
        }
    }
}