using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Extensions;
using aspnetapp.Serializer;
using aspnetapp.Sys;

namespace aspnetapp
{
    public class Account : IEquatable<Account>, IComparable<Account>
    {
        public override int GetHashCode() => id.GetHashCode();

        public static bool operator ==(Account left, Account right) => Equals(left, right);

        public static bool operator !=(Account left, Account right) => !Equals(left, right);

        private HList<Like> _likes;

        public uint id;

        public Premimum premium;

        public SexStatus SexStatus;

        public Email Email;

        //For loading from db
        public string sex
        {
            set => SexStatus |= value == "m" ? SexStatus.Male : SexStatus.Female;
        }

        //For loading from db
        public string status
        {
            set => SexStatus |= Statuses.GetStatus(value);
        }

        public ushort fnameIndex;
        public string fname
        {
            set => fnameIndex = StringIndexer.FirstNames.GetOrAdd(value);
        }

        public ushort snameIndex;
        public string sname
        {
            set => snameIndex = StringIndexer.LastNames.GetOrAdd(value);
        }

        public Utf8String phone;

        public int birth;


        public ushort countryIndex;

        public string country
        {
            set => countryIndex = StringIndexer.Countries.GetOrAdd(value);
        }

        public ushort cityIndex;

        public string city
        {
            set => cityIndex = StringIndexer.Cities.GetOrAdd(value);
        }

        public int joined;

        public Interests InterestIndexes;

        public HList<string> interests
        {
            set
            {
                if (value != null)
                {
                    var shortInterests = value
                        .Select(x => StringIndexer.Interests.GetOrAdd(x))
                        .ToArray();

                    InterestIndexes = new Interests(shortInterests);
                }
            }
        }

        public HList<Like> likes
        {
            get => _likes;
            set
            {
                _likes = value;
                _likes?.Sort();
            }
        }


        public bool HasPremium() => premium.start <= Database.Now && Database.Now <= premium.finish;

        public void ReplaceLikes(List<LikeStub> newLikes, bool trim)
        {
            likes?.ClearFast();

            if (newLikes == null)
            {
                return;
            }

            foreach (var like in newLikes)
            {
                AddLike(like, out _);
            }

            if (trim)
            {
                likes?.TrimExcess();
            }

/*            if (newLikes == null)
            {
                likes?.ClearFast();;
                return;
            }

            if (likes == null)
            {
                likes = new HList<Like>(newLikes.Count);
            }
            else
            {
                likes.ClearFast();
            }

            newLikes.Sort((x,y) => y.id.CompareTo(x.id));
            LikeStub previous = default;
            foreach (var current in newLikes)
            {
                if (current.id != previous.id)
                {
                    likes.Add(new Like(current.id, current.ts));
                    previous = current;
                }
                else
                {
                    ref Like last = ref likes.GetByRef(likes.Count - 1);
                    last = last.Add(current.ts);
                }
            }*/

        }

        public bool AddLike(LikeStub newLike, out Like result)
        {
            if (likes == null)
            {
                likes = new HList<Like>(40);
            }

            var likeeIndex = likes.BinarySearchAscending(Like.IdLookUp(newLike.id));
            if (likeeIndex >= 0)
            {
                ref Like like = ref likes.GetByRef(likeeIndex);
                 like = like.Add(newLike.ts);
                 result = like;
                return false;
            }
            else
            {
                var insertAt = ~likeeIndex;
                var like = new Like(newLike.id, newLike.ts);
                likes.Insert(insertAt, like);
                result = like;
                return true;
            }
        }

        public bool LikedAll(HList<uint> queryLikes)
        {
            Like[] meLikes = _likes.Items;
            var otherCount = queryLikes.Count;
            if (meLikes == null || _likes.Count < otherCount)
            {
                return false;
            }

            uint meIndex = 0;

            //TODO: Consider bin search if meLikes is much bigger than query likes.
            foreach (var candidate in queryLikes)
            {
                while (true)
                {
                    ref Like meLike = ref meLikes[meIndex];
                    meIndex++;

                    if (candidate == meLike.Id)
                    {
                        break;
                    }

                    if (candidate > meLike.Id || meIndex == meLikes.Length)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool LikedOne(uint accountId)
        {
            if (_likes == null || _likes.Count == 0)
            {
                return false;
            }

            var minLikeeId = _likes[(uint) (_likes.Count - 1)].Id; // this is max possible id;
            if (accountId < minLikeeId)
            {
                return false;
            }

            var maxLikeeId = _likes[0].Id;

            if (maxLikeeId < accountId)
            {
                return false;
            }
            //TODO: Why does it work for an array sorted in a descending way?
            return _likes.BinarySearchAscending(Like.IdLookUp(accountId)) >= 0;

        }

        public bool Equals(Account other) => id == other.id;

        public override bool Equals(object obj) => obj is Account other &&  Equals(other);

        public override string ToString() => id.ToString(CultureInfo.InvariantCulture);

        public int CompareTo(Account other) => other.id.CompareTo(id);

        public static bool operator <(Account left, Account right) => Comparer<Account>.Default.Compare(left, right) < 0;

        public static bool operator >(Account left, Account right) => Comparer<Account>.Default.Compare(left, right) > 0;

        public static bool operator <=(Account left, Account right) => Comparer<Account>.Default.Compare(left, right) <= 0;

        public static bool operator >=(Account left, Account right) => Comparer<Account>.Default.Compare(left, right) >= 0;
    }
}