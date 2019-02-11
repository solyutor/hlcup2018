using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace aspnetapp.Controllers
{
    public static partial class FilterBuilder
    {
        private static readonly Dictionary<FilterTypes, Predicate> Cache = new Dictionary<FilterTypes, Predicate>();
        private static readonly ConcurrentDictionary<FilterTypes, Predicate> MissesCache = new ConcurrentDictionary<FilterTypes, Predicate>();

        public static Predicate GetFilter(FilterTypes filters)
        {
            if (Cache.TryGetValue(filters, out Predicate result))
            {
                return result;
            }

            return Fallback(filters);
        }

        private static Predicate Fallback(FilterTypes filters)
        {
            if (MissesCache.TryGetValue(filters, out Predicate result))
            {
                return result;
            }

            result = CreateDelegate(filters);


            if (MissesCache.TryAdd(filters, result))
            {
                IEnumerable<string> elements = filters.EnumerateSetFlags().Select(x => $"{nameof(FilterTypes)}.{x}");
                Console.WriteLine(string.Join(" | ", elements) + ",");
            }

            return result;
        }

        //TODO: Consider putting predefined values into array
        [Filter(FilterTypes.sex_eq)]
        private static bool Sex(Account account, AbstractQuery query)
            => (account.SexStatus & SexStatus.AllSex) == query._sexStatus;

        [Filter(FilterTypes.status_eq)]
        private static bool StatusEq(Account account, AbstractQuery query)
            => (account.SexStatus & SexStatus.AllStatus) == query._sexStatus;

        //Query has two statuses set, must be one of them
        [Filter(FilterTypes.status_neq)]
        private static bool StatusNeq(Account account, AbstractQuery query)
            => (account.SexStatus & query._sexStatus) != SexStatus.None;


        [Filter(FilterTypes.sex_eq | FilterTypes.status_eq)]
        private static bool SexStatusEq(Account account, AbstractQuery query)
            => account.SexStatus == query._sexStatus;

        [Filter(FilterTypes.sex_eq | FilterTypes.status_neq)]
        private static bool SexStatusNeq(Account account, AbstractQuery query)
            => (account.SexStatus | query._sexStatus) == query._sexStatus;


        [Filter(FilterTypes.email_domain)]
        private static bool EmailDomain(Account account, AbstractQuery query) => account.Email._domain == query.domainIndex;

        [Filter(FilterTypes.email_lt)]
        private static bool EmailLt(Account account, AbstractQuery query) => account.Email.CompareTo(query.email) < 0;

        [Filter(FilterTypes.email_gt)]
        private static bool EmailGt(Account account, AbstractQuery query) => account.Email.CompareTo(query.email) > 0;

        [Filter(FilterTypes.fname_eq)]
        private static bool FirstNameEq(Account account, AbstractQuery query) => query.fnameIndex != 0 && query.fnameIndex == account.fnameIndex;

        [Filter(FilterTypes.fname_any)]
        private static bool FirstNameAny(Account account, AbstractQuery query) => query.fnamesIndexes.Contains(account.fnameIndex);

        [Filter(FilterTypes.fname_null)]
        private static bool FirstNameNull(Account account, AbstractQuery query) => account.fnameIndex == 0;

        [Filter(FilterTypes.fname_not_null)]
        private static bool FirstNameNotNull(Account account, AbstractQuery query) => account.fnameIndex != 0;

        [Filter(FilterTypes.sname_starts)]
        private static bool LastNameStarts(Account account, AbstractQuery query)
            //TODO: this method sucks currently. Introduce a good index.
            => account.snameIndex > 0 && StringIndexer.LastNames[account.snameIndex].StartsWith(query.snamePrefix, StringComparison.Ordinal);

        [Filter(FilterTypes.sname_eq)]
        private static bool LastNameEq(Account account, AbstractQuery query) => account.snameIndex == query.snameIndex;

        [Filter(FilterTypes.sname_null)]
        private static bool LastNameNull(Account account, AbstractQuery query) => account.snameIndex == 0;

        [Filter(FilterTypes.sname_not_null)]
        private static bool LastNameNotNull(Account account, AbstractQuery query) => account.snameIndex != 0;


        [Filter(FilterTypes.joined)]
        private static bool Joined(Account account, AbstractQuery query) => query.joinedFrom <= account.joined && account.joined < query.joinedTo;


        [Filter(FilterTypes.phone_code)]
        private static bool PhoneCode(Account account, AbstractQuery query) => query.phoneCode.IsPrefixOf(account.phone);

        [Filter(FilterTypes.phone_null)]
        private static bool PhoneNull(Account account, AbstractQuery query) => account.phone.IsEmpty;

        [Filter(FilterTypes.phone_not_null)]
        private static bool PhoneNotNull(Account account, AbstractQuery query) => !account.phone.IsEmpty;


        [Filter(FilterTypes.city_eq)]
        private static bool CityEq(Account account, AbstractQuery query) => query.cityIndex > 0 && query.cityIndex == account.cityIndex;

        [Filter(FilterTypes.city_any)]
        private static bool CityAny(Account account, AbstractQuery query) => query.cities != null && query.cities.Contains(account.cityIndex);

        [Filter(FilterTypes.city_null)]
        private static bool CityNull(Account account, AbstractQuery query) => account.cityIndex == 0;

        [Filter(FilterTypes.city_not_null)]
        private static bool CityNotNull(Account account, AbstractQuery query) => account.cityIndex != 0;


        [Filter(FilterTypes.birth_lt)]
        private static bool BirthLt(Account account, AbstractQuery query) => account.birth < query.birth;

        [Filter(FilterTypes.birth_gt)]
        private static bool BirthGt(Account account, AbstractQuery query) => query.birth < account.birth;

        [Filter(FilterTypes.birth_year)]
        private static bool BirthYear(Account account, AbstractQuery query) => query.birthFrom <= account.birth && account.birth < query.birthTo;


        [Filter(FilterTypes.interests_all)]
        private static bool InterestsAll(Account account, AbstractQuery query) => account.InterestIndexes.Count > 0 && account.InterestIndexes.ContainsAll(query.interestIndexes);

        [Filter(FilterTypes.interests_any)]
        private static bool InterestsAny(Account account, AbstractQuery query) => account.InterestIndexes.Count > 0 && account.InterestIndexes.ContainsAny(query.interestIndexes);


        [Filter(FilterTypes.likes_one)]
        private static bool LikesOne(Account account, AbstractQuery query) => account.LikedOne(query.like);

        [Filter(FilterTypes.likes_all)]
        private static bool LikesAll(Account account, AbstractQuery query) => account.likes != null && account.LikedAll(query.likes);


        [Filter(FilterTypes.premium_now)]
        private static bool PremiumNow(Account account, AbstractQuery query) => account.HasPremium();

        [Filter(FilterTypes.premium_null)]
        private static bool PremiumNull(Account account, AbstractQuery query) => account.premium.start == 0;

        [Filter(FilterTypes.premium_not_null)]
        private static bool PremiumNotNull(Account account, AbstractQuery query) => account.premium.start != 0;


        [Filter(FilterTypes.country_eq)]
        private static bool CountryEq(Account account, AbstractQuery query) => query.countryIndex > 0 && query.countryIndex == account.countryIndex;

        [Filter(FilterTypes.country_null)]
        private static bool CountryNull(Account account, AbstractQuery query) => account.countryIndex == 0;

        [Filter(FilterTypes.country_not_null)]
        private static bool CountryNotNull(Account account, AbstractQuery query) => account.countryIndex != 0;
    }
}