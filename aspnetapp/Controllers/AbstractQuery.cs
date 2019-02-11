using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Domain;
using aspnetapp.Extensions;

namespace aspnetapp.Controllers
{
    public  abstract  unsafe class AbstractQuery
    {
        public static readonly HList<Account> Empty = new HList<Account>();
        public readonly HashSet<int> Fields = new HashSet<int>();

        protected FilterTypes _filters;
        public uint like;
        public SexStatus _sexStatus;
        public int birth;
        public HashSet<ushort> cities;
        public ushort countryIndex;

        public ushort domainIndex;

        public Prefix email;

        public HList<ushort> fnamesIndexes;


        public Interests interestIndexes;
        public HList<uint> likes;
        public int limit = 50;
        public PhoneCode phoneCode;

        public string snamePrefix;
        public ushort cityIndex;


        public bool WillYieldZeroResults;
        public ushort snameIndex;
        public ushort fnameIndex;
        public int joinedFrom;
        public int joinedTo;
        public int birthFrom;
        public int birthTo;

        private readonly UintMultiUnionList _union = new UintMultiUnionList();
        private readonly UintIntersectList _intersect = new UintIntersectList();

        private void AddFilter(FilterTypes type)
        {
            _filters |= type;
        }

        public virtual void Reset()
        {
            //Do not clear other fields as they will be ignored actually
            Fields.Clear();
            cities?.Clear();
            likes?.Clear();
            fnamesIndexes?.Clear();
            WillYieldZeroResults = false;
            _filters = FilterTypes.empty;
            _sexStatus = SexStatus.None;
            cityIndex = 0;
            countryIndex = 0;
            joinedTo = 0;
            joinedFrom = 0;
            birth = 0;
            _union.Clear();
            _intersect.Clear();
        }

        public bool AddNeqStatus(string value)
        {
            if (value.OrdinalEqualsTo(Statuses.Free))
            {
                AddSexStatusFilter(SexStatus.NotFree | SexStatus.Complex, FilterTypes.status_neq);
            }
            else if (value.OrdinalEqualsTo(Statuses.NotFree))
            {
                AddSexStatusFilter(SexStatus.Free | SexStatus.Complex, FilterTypes.status_neq);
            }
            else if (value.OrdinalEqualsTo(Statuses.Complicated))
            {
                AddSexStatusFilter(SexStatus.Free | SexStatus.NotFree, FilterTypes.status_neq);
            }
            else
            {
                return false;
            }

            Fields.Add(Serializer.Fields.Status);

            return true;
        }

        public bool AddEmailDomain(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("."))
            {
                return false;
            }

            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.Domains.TryGetIndex(value, out domainIndex);

            AddFilter(FilterTypes.email_domain);
            return true;
        }

        public bool AddEmailLt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            email = new Prefix(value);
            AddFilter(FilterTypes.email_lt);

            WillYieldZeroResults = WillYieldZeroResults || Database.MinEmail.CompareTo(email) > 0;

            return true;
        }

        public bool AddEmailGt(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            email = new Prefix(value);;
            AddFilter(FilterTypes.email_gt);

            WillYieldZeroResults = WillYieldZeroResults || Database.MaxIEmail.CompareTo(email) < 0;
            return true;
        }

        public bool AddFirstName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            Fields.Add(Serializer.Fields.FName);

            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.FirstNames.TryGetIndex(value, out fnameIndex);
            return true;
        }

        public bool AddFirstNames(string value)
        {
            string[] values = value.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var fname in values)
            {
                if (string.IsNullOrWhiteSpace(fname))
                {
                    return false;
                }

                if (StringIndexer.FirstNames.TryGetIndex(fname, out ushort temp))
                {
                    (fnamesIndexes ?? (fnamesIndexes = new HList<ushort>())).Add(temp);
                }
                // do not add the filter twice
            }
            AddFilter(FilterTypes.fname_any);
            Fields.Add(Serializer.Fields.FName);
            return true;
        }

        public bool HasFirstName(string value)
            => CheckForNull(value, Serializer.Fields.FName, FilterTypes.fname_null,  FilterTypes.fname_not_null);

        public bool AddLastNamePrefix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            snamePrefix = value;
            Fields.Add(Serializer.Fields.SName);
            AddFilter(FilterTypes.sname_starts);
            return true;
        }

        public bool HasLastName(string value)
        {
            if (!sbyte.TryParse(value, out var result) || !(result == 0 || result == 1))
            {
                return false;
            }

            return CheckForNull(value, Serializer.Fields.SName, FilterTypes.sname_null, FilterTypes.sname_not_null);
        }

        public bool AddPhoneCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 3 || !char.IsDigit(value[0]) || !char.IsDigit(value[1]) || !char.IsDigit(value[2]))
            {
                return false;
            }

            Fields.Add(Serializer.Fields.Phone);

            phoneCode = PhoneCode.From(value);
            AddFilter(FilterTypes.phone_code);
            return true;
        }

        public bool HasPhone(string value)
            => CheckForNull(value, Serializer.Fields.Phone, FilterTypes.phone_null, FilterTypes.phone_not_null);

        public bool HasCountry(string value)
        {
            if (CheckForNull(value, Serializer.Fields.Country, FilterTypes.country_null, FilterTypes.country_not_null))
            {
                if (_filters.ContainsAny(FilterTypes.country_not_null))
                {
                    countryIndex = StringIndexer.MaxCountryCount;
                }

                return true;
            }

            return false;
        }

        public bool AddCity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }
            //TODO: Short cut if city is not found
            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.Cities.TryGetIndex(value, out cityIndex);

            AddFilter(FilterTypes.city_eq);
            Fields.Add(Serializer.Fields.City);
            return true;
        }

        public bool AddCities(string value)
        {
            string[] values = value.Split(",", StringSplitOptions.RemoveEmptyEntries);
            foreach (var city in values)
            {
                if (string.IsNullOrWhiteSpace(city))
                {
                    return false;
                }
                //TODO : Try shortcut the request in case of all cities are not present in the database
                if (StringIndexer.Cities.TryGetIndex(city, out ushort tempIndex))
                {
                    (cities ?? (cities = new HashSet<ushort>())).Add(tempIndex);
                }
            }

            WillYieldZeroResults = WillYieldZeroResults || cities?.Count == 0;

            AddFilter(FilterTypes.city_any);
            Fields.Add(Serializer.Fields.City);

            return true;
        }

        public bool HasCity(string value)
        {
            if (CheckForNull(value, Serializer.Fields.City, FilterTypes.city_null, FilterTypes.city_not_null))
            {
                if (_filters.ContainsAny(FilterTypes.city_not_null))
                {
                    cityIndex = StringIndexer.MaxCityCount;
                }

                return true;
            }

            return false;
        }

        private bool CheckForNull(string value, int field, FilterTypes isNull,  FilterTypes isNotNull)
        {
            if (!sbyte.TryParse(value, out var result) || !(result == 0 || result == 1))
            {
                return false;
            }

            var not_null = result == 0;
            if (not_null)
            {
                Fields.Add(field);
                AddFilter(isNotNull);
            }
            else
            {
                AddFilter(isNull);
            }

            return true;
        }

        public bool AddBirthLt(string value)
        {
            AddFilter(FilterTypes.birth_lt);
            return AddBirth(value);
        }

        public bool AddBirthGt(string value)
        {
            AddFilter(FilterTypes.birth_gt);
            return AddBirth(value);
        }

        private bool AddBirth(string value)
        {
            Fields.Add(Serializer.Fields.Birth);
            return int.TryParse(value, out birth);
        }

        public bool AddBirthYear(string value)
        {
            if (!int.TryParse(value, out birth))
            {
                return false;
            }
            var unixEpochYear = DateTime.UnixEpoch.Year;
            DateTime startYears = DateTime.UnixEpoch.AddYears((birth - unixEpochYear));
            DateTime endYear = startYears.AddYears(1);
            birthFrom = (int)(startYears - DateTime.UnixEpoch).TotalSeconds;
            birthTo = (int)(endYear - DateTime.UnixEpoch).TotalSeconds;

            Fields.Add(Serializer.Fields.Birth);
            AddFilter(FilterTypes.birth_year);
            return true;
        }

        public bool AddInterestsAll(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] interests = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = new HList<ushort>();

            foreach (var interest in interests)
            {
                if (StringIndexer.Interests.TryGetIndex(interest, out ushort index))
                {
                    list.Add(index);
                }
                else
                {
                    WillYieldZeroResults = true;
                    return true;
                }
            }
            //TODO: Remove ToArray() call and the list completely
            interestIndexes = new Interests(list);

            AddFilter(FilterTypes.interests_all);

            return true;
        }

        public bool AddInterestsAny(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] interests = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var list = new HList<ushort>();

            foreach (var interest in interests)
            {
                if (StringIndexer.Interests.TryGetIndex(interest, out ushort index))
                {
                    list.Add(index);
                }
            }
            interestIndexes = new Interests(list);
            WillYieldZeroResults = WillYieldZeroResults || interestIndexes.Count == 0;
            AddFilter(FilterTypes.interests_any);

            return true;
        }

        public bool AddAllLikes(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] values = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

            likes = new HList<uint>();

            foreach (var s in values)
            {
                if (!uint.TryParse(s, out var id))
                {
                    return false;
                }

                likes.Add(id);
            }
            likes.SortDescending();

            AddFilter(FilterTypes.likes_all);
            return true;
        }

        public bool HasPremiumNow(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 1 || value[0] != '1')
            {
                return false;
            }

            Fields.Add(Serializer.Fields.Premium);
            AddFilter(FilterTypes.premium_now);
            return true;
        }

        public bool HasPremium(string value)
            => CheckForNull(value, Serializer.Fields.Premium, FilterTypes.premium_null, FilterTypes.premium_not_null);


        public bool AddSex(string value)
        {
            if (value == "m")
            {
                AddSexStatusFilter(SexStatus.Male, FilterTypes.sex_eq);
            }
            else if (value == "f")
            {
                AddSexStatusFilter(SexStatus.Female, FilterTypes.sex_eq);
            }
            else
            {
                return false;
            }

            Fields.Add(Serializer.Fields.Sex);
            return true;
        }

        private void AddSexStatusFilter(SexStatus sexStatus, FilterTypes type)
        {
            _sexStatus |= sexStatus;
            _filters |= type;
        }

        public bool AddEqStatus(string value)
        {
            SexStatus status = Statuses.GetStatus(value);
            if (status == SexStatus.None)
            {
                return false;
            }

            Fields.Add(Serializer.Fields.Status);
            AddSexStatusFilter(status, FilterTypes.status_eq);

            return true;
        }

        public bool AddLastName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.LastNames.TryGetIndex(value, out snameIndex);

            Fields.Add(Serializer.Fields.SName);
            AddFilter(FilterTypes.sname_eq);
            return true;
        }

        public bool AddCountry(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }


            WillYieldZeroResults = WillYieldZeroResults || !StringIndexer.Countries.TryGetIndex(value, out countryIndex);

            Fields.Add(Serializer.Fields.Country);
            AddFilter(FilterTypes.country_eq);

            return true;
        }

        public bool AddJoined(string value)
        {
            if (!ushort.TryParse(value, out ushort joined))
            {
                return false;
            }

            var unixEpochYear = DateTime.UnixEpoch.Year;
            DateTime startYears = DateTime.UnixEpoch.AddYears((joined - unixEpochYear));
            DateTime endYear = startYears.AddYears(1);
            joinedFrom = (int)(startYears - DateTime.UnixEpoch).TotalSeconds;
            joinedTo = (int)(endYear - DateTime.UnixEpoch).TotalSeconds;

            AddFilter(FilterTypes.joined);
            return true;
        }

        public bool AddLike(string value)
        {
            if (!uint.TryParse(value, out like))
            {
                return false;
            }

            AddFilter(FilterTypes.likes_one);
            return true;
        }

        protected IEnumerable<uint> GetAccounts()
        {
            IReadOnlyCollection<uint> result = Database.GetAccountIds();

            FilterTypes filteredBy = FilterTypes.empty;

            FilterTypes filteredByUnion = FilterTypes.empty;

            if (_filters.ContainsAll(FilterTypes.interests_any | FilterTypes.likes_all))
            {
                _filters = _filters.ResetFlags(FilterTypes.likes_all);
                if (likes.Count == 1)
                {
                    return LikesIndexer.GetAllWhoLikes(likes[0]);
                }
                foreach (var likee in likes)
                {
                    _intersect.Add(LikesIndexer.GetAllWhoLikes(likee));
                }

                foreach (var interest in interestIndexes)
                {
                    _intersect.Add(StringIndexer.Interests.GetList(interest));
                }

                _filters = _filters.ResetFlags(FilterTypes.interests_any);
                return _intersect;
            }


            if (_filters.ContainsAll(FilterTypes.fname_any | FilterTypes.sex_eq))
            {
                if (fnamesIndexes == null || fnamesIndexes.Count == 0)
                {
                    return Array.Empty<uint>();
                }

                var probeNameList = StringIndexer.FirstNames.GetList(fnamesIndexes[0]);
                var probeSex = Database.GetAccount(probeNameList[0]).SexStatus & SexStatus.AllSex;

                if ((probeSex & _sexStatus) == SexStatus.None)
                {
                    return Array.Empty<uint>();
                }
            }

            if (_filters.ContainsAll(FilterTypes.birth_year)
                && _filters.ContainsAny(FilterTypes.cities_all_types)
                && _filters.ContainsAny(FilterTypes.sex_status_all_types))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCity(cityIndex);
                var birthYear = (ushort)birth;
                var list = filtered.GetListBy(_sexStatus, birthYear, _union);
                _filters = _filters.ResetFlags(FilterTypes.birth_year | FilterTypes.cities_all_types | FilterTypes.sex_status_all_types);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.sex_status_all_types)
                && _filters.ContainsAny(FilterTypes.cities_all_types)
                && !_filters.ContainsAny(FilterTypes.interests_all | FilterTypes.phone_code))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCity(cityIndex);
                var list = filtered.GetListBy(_sexStatus, _union);

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.cities_all_types);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.birth_year)
                && _filters.ContainsAny(FilterTypes.sex_status_all_types)
                && _filters.ContainsAny(FilterTypes.countries_all_types))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCountry(countryIndex);

                var list = filtered.GetListBy(_sexStatus, (ushort)birth, _union);

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.countries_all_types | FilterTypes.birth_year);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.sex_status_all_types)
                && _filters.ContainsAny(FilterTypes.countries_all_types)
                && !_filters.ContainsAny(FilterTypes.interests_all | FilterTypes.phone_code))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCountry(countryIndex);

                var list = filtered.GetListBy(_sexStatus, _union);

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.countries_all_types);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.countries_all_types) && _filters.ContainsAny(FilterTypes.phone_code))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCountry(countryIndex);
                var list = filtered.GetListBy(_sexStatus, phoneCode, _union);

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.countries_all_types | FilterTypes.phone_code);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.cities_all_types) && _filters.ContainsAny(FilterTypes.phone_code))
            {
                ref StatusGroup filtered = ref GroupIndex.SexGroupByCity(cityIndex);
                var list = filtered.GetListBy(_sexStatus, phoneCode, _union);

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.cities_all_types | FilterTypes.phone_code);
                return list;
            }

            if (_filters.ContainsAny(FilterTypes.city_any) && _filters.ContainsAny(FilterTypes.phone_code))
            {
                foreach (var index in cities)
                {
                    ref StatusGroup filtered = ref GroupIndex.SexGroupByCity(index);
                    filtered.GetListBy(_sexStatus, phoneCode, _union);
                }

                _filters = _filters.ResetFlags(FilterTypes.sex_status_all_types | FilterTypes.city_any | FilterTypes.phone_code);
                return _union;
            }


            if (_filters.ContainsAll(FilterTypes.interests_all) && !_filters.ContainsAny(FilterTypes.likes_all | FilterTypes.likes_one))
            {
                var interests = StringIndexer.Interests;
                if (interestIndexes.Count == 1)
                {
                    var byInterest = interests.GetList(interestIndexes.First);
                    if (byInterest.Count < result.Count)
                    {
                        result = byInterest;
                        filteredBy = FilterTypes.interests_all;
                    }
                }
                else
                {
                    foreach (var interest in interestIndexes)
                    {
                        _intersect.Add(interests.GetList(interest));
                    }
                    _filters = _filters.ResetFlags(FilterTypes.interests_all);
                    return _intersect;
                }
            }


            if (_filters.ContainsAll(FilterTypes.city_any))
            {
                StringIndexer index = StringIndexer.Cities;
                if (cities.Count == 1)
                {
                    var byCity = index.GetList(cities.First());
                    if (byCity.Count < result.Count)
                    {
                        result = byCity;
                        filteredBy = FilterTypes.city_any;
                    }
                }
                else
                {
                    foreach (var city in cities)
                    {
                        _union.Add(index.GetList(city));
                    }
                    filteredByUnion = FilterTypes.city_any;
                }
            }
            else if (_filters.ContainsAll(FilterTypes.fname_any))
            {
                StringIndexer firstNames = StringIndexer.FirstNames;
                if (fnamesIndexes.Count == 1)
                {
                    var byFname = firstNames.GetList(fnamesIndexes.First());
                    if (byFname.Count < result.Count)
                    {
                        result = byFname;
                        filteredBy = FilterTypes.fname_any;
                    }
                }
                else
                {
                    foreach (var fname in fnamesIndexes)
                    {
                        _union.Add(firstNames.GetList(fname));
                    }
                    filteredByUnion =  FilterTypes.fname_any;
                }
            }
            else if (_filters.ContainsAll(FilterTypes.interests_any))
            {
                StringIndexer index = StringIndexer.Interests;
                if (interestIndexes.Count == 1)
                {
                    _filters = _filters.ResetFlags(FilterTypes.interests_any);
                    return index.GetList(interestIndexes.First);
                }
                else
                {
                    foreach (var interest in interestIndexes)
                    {
                        _union.Add(index.GetList(interest));
                    }

                    filteredByUnion = FilterTypes.interests_any;
                }
            }


            if (_filters.ContainsAny(FilterTypes.cities_all_types))
            {
                var byCity = StringIndexer.Cities.GetList(cityIndex);
                if (byCity.Count < result.Count)
                {
                    result = byCity;
                    filteredBy = FilterTypes.cities_all_types;
                }
            }


            if (_filters.ContainsAll(FilterTypes.likes_one))
            {
                var byLike = LikesIndexer.GetAllWhoLikes(like);
                if (byLike.Count < result.Count)
                {
                    result = byLike;
                    filteredBy = FilterTypes.likes_one;
                }

            }

            if (_filters.ContainsAll(FilterTypes.likes_all))
            {
                if (likes.Count == 1)
                {
                    _filters = _filters.ResetFlags(FilterTypes.likes_all);
                    return LikesIndexer.GetAllWhoLikes(likes.First());
                }

                foreach (var likee in likes)
                {
                    _intersect.Add(LikesIndexer.GetAllWhoLikes(likee));
                }

                _filters = _filters.ResetFlags(FilterTypes.likes_all);
                return _intersect;
            }


            if (_filters.ContainsAny(FilterTypes.countries_all_types))
            {
                var byCountry = StringIndexer.Countries.GetList(countryIndex);

                if (byCountry.Count < result.Count)
                {
                    result = byCountry;
                    filteredBy = FilterTypes.countries_all_types;
                }
            }

            if (_filters.ContainsAll(FilterTypes.fname_eq))
            {
                var byFName = StringIndexer.FirstNames.GetList(fnameIndex);
                if (byFName.Count < result.Count)
                {
                    result = byFName;
                    filteredBy = FilterTypes.fname_eq;
                }
            }

            if (_filters.ContainsAll(FilterTypes.fname_null))
            {
                var nullName = StringIndexer.FirstNames.GetList(0);
                if (nullName.Count < result.Count)
                {
                    result = nullName;
                    filteredBy = FilterTypes.fname_null;
                }
            }

            if (_filters.ContainsAll(FilterTypes.sname_eq))
            {
                var bySname = StringIndexer.FirstNames.GetList(snameIndex);
                if (bySname.Count < result.Count)
                {
                    result = bySname;
                    filteredBy = FilterTypes.sname_eq;
                }
            }

            if (_filters.ContainsAll(FilterTypes.sname_null))
            {
                var nullName = StringIndexer.LastNames.GetList(0);
                if (nullName.Count < result.Count)
                {
                    result = nullName;
                    filteredBy = FilterTypes.sname_null;
                }
            }

            if (_filters.ContainsAny(FilterTypes.birth_year))
            {
                var byBirth = BirthIndex.GetList((ushort)birth);
                if (byBirth.Count < result.Count)
                {
                    result = byBirth;
                    filteredBy = FilterTypes.birth_year;
                }
            }

            if (_filters.ContainsAny(FilterTypes.joined))
            {
                var byJoined = JoinedIndex.GetList(joinedFrom.GetYear());
                if (byJoined.Count < result.Count)
                {
                    result = byJoined;
                    filteredBy = FilterTypes.joined;
                }
            }

            if (_filters.ContainsAll(FilterTypes.email_domain))
            {
                var byDomain = StringIndexer.Domains.GetList(domainIndex);

                if (byDomain.Count < result.Count)
                {
                    result = byDomain;
                    filteredBy = FilterTypes.email_domain;
                }
            }

            if (filteredByUnion != FilterTypes.empty && filteredBy != FilterTypes.empty)
            {
                _filters = _filters.ResetFlags(filteredBy | filteredByUnion);
                return new IntersectList<uint>(result, _union);
            }

            if (filteredByUnion != FilterTypes.empty)
            {
                _filters = _filters.ResetFlags(filteredByUnion);
                return _union;
            }

            _filters = _filters.ResetFlags(filteredBy);
            return result;
        }

    }
}