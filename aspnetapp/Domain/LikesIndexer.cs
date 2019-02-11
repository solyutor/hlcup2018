using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class LikesIndexer
    {
        private static readonly HList<uint> Empty = new HList<uint>();

        private static readonly LikeeIndexEntry[] Likee2Likers = new LikeeIndexEntry[Constants.MaxAccountNumber];

        public static void Index(Account liker)
        {
            if (liker.likes == null)
            {
                return;
            }

            foreach (Like like in liker.likes)
            {
                IndexLike(liker, like);
            }
        }

        public static void IndexLike(Account liker, Like like)
        {
            ref LikeeIndexEntry likee2Liker = ref Likee2Likers[like.Id];

            if (likee2Liker.Likers == null)
            {
                likee2Liker = new LikeeIndexEntry(new HList<uint>(100), 0, 0, 0, 0, 0);
            }

            likee2Liker = likee2Liker.Add(liker);
        }

        public static void Remove(Account liker)
        {
            if (liker.likes == null)
            {
                return;
            }
            foreach (Like like in liker.likes)
            {
                ref LikeeIndexEntry likee2Liker = ref Likee2Likers[like.Id];
                likee2Liker = likee2Liker.Remove(liker);
            }
        }

        public static HList<uint> GetAllWhoLikes(uint likee) => Likee2Likers[likee].Likers ?? Empty;

        public static LikeeIndexEntry GetAllWhoLikes2(uint likee) => Likee2Likers[likee];

        public static void Trim()
        {
            foreach (var list in Likee2Likers)
            {
                list.Trim();
            }
        }


        public static bool TryGetIndex(uint meId, out HList<uint> hList)
        {
            throw new NotImplementedException();
        }

    }
}