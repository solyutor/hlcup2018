using System;
using System.Collections.Generic;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Domain;
using Microsoft.Extensions.ObjectPool;

namespace aspnetapp.Controllers
{
    public class GroupQuery : AbstractQuery, IComparer<KeyValuePair<GroupKey, int>>
    {
        private static readonly Func<Account, ushort, GroupKey>[] Generators = new Func<Account, ushort, GroupKey>[(byte)(GroupKeys.City | GroupKeys.Country) + 1];

        static GroupQuery()
        {
            Generators[(byte) GroupKeys.Status] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllStatus,0,0);
            Generators[(byte) GroupKeys.Sex] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllSex,0,0);
            Generators[(byte) GroupKeys.Interests] = (a,i) => new GroupKey(SexStatus.None,i,0);
            Generators[(byte) GroupKeys.Country] = (a,i) => new GroupKey(SexStatus.None,0,a.countryIndex, 0);
            Generators[(byte) GroupKeys.City] = (a,i) => new GroupKey(SexStatus.None,0,0, a.cityIndex);

            Generators[(byte) (GroupKeys.Country | GroupKeys.Status)] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllStatus,0,a.countryIndex, 0);
            Generators[(byte) (GroupKeys.City | GroupKeys.Status)] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllStatus,0,0, a.cityIndex);
            Generators[(byte) (GroupKeys.Country | GroupKeys.Sex)] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllSex,0,a.countryIndex, 0);
            Generators[(byte) (GroupKeys.City | GroupKeys.Sex)] = (a,i) => new GroupKey(a.SexStatus & SexStatus.AllSex,0,0, a.cityIndex);
        }

        public GroupKeys _first;
        public GroupKeys _second;

        public sbyte order;
        private Func<Account, ushort, GroupKey> _getKey;
        private readonly HGroupDict _grouped = new HGroupDict(100);
        private readonly HList<KeyValuePair<GroupKey, int>> _results = new HList<KeyValuePair<GroupKey, int>>(100);

        public override void Reset()
        {
            _first = GroupKeys.None;
            _second = GroupKeys.None;
            _grouped.Clear();
            _results.Clear();
            base.Reset();
        }

        public int Compare(KeyValuePair<GroupKey, int> x, KeyValuePair<GroupKey, int> y)
        {
            var result = x.Value.CompareTo(y.Value);

            if (result == 0)
            {
                result = HOrdinalComparer.Instance.Compare(x.Key[_first], y.Key[_first]);
                if (result == 0 && _second != GroupKeys.None)
                {
                    result = HOrdinalComparer.Instance.Compare(x.Key[_second], y.Key[_second]);
                }

            }

            return order * result;
        }

        public bool AddOrder(string value)
        {
            if (value == "1")
            {
                order = 1;
                return true;
            }

            if (value == "-1")
            {
                order = -1;
                return true;
            }

            return false;
        }

        public bool AddKeys(string value)
        {
            if (value == null)
            {
                return false;
            }

            ReadOnlySpan<char> span = value.AsSpan();
            int commaIndex = span.IndexOf(',');
            if (commaIndex < 0)
            {
                _first = GetKey(span);
                _getKey = Generators[(uint)_first];
                return _first != GroupKeys.None;
            }

            _first = GetKey(span.Slice(0, commaIndex));
            if (_first == GroupKeys.None)
            {
                return false;
            }


            _second = GetKey(span.Slice(commaIndex + 1));
            _getKey = Generators[(uint)(_first | _second)];
            return _second != GroupKeys.None;
        }

        private GroupKeys GetKey(in ReadOnlySpan<char> span)
        {
            if (span.SequenceEqual("status"))
            {
                return GroupKeys.Status;
            }

            if(span.SequenceEqual("sex"))
            {
                return GroupKeys.Sex;
            }

            if (span.SequenceEqual("interests"))
            {
                return GroupKeys.Interests;
            }

            if (span.SequenceEqual("country"))
            {
                return GroupKeys.Country;
            }

            if (span.SequenceEqual("city"))
            {
                return GroupKeys.City;
            }


            return GroupKeys.None;
        }


        private GroupKey GetKey(Account account, ushort interestIndex)
        {
            return _getKey(account, interestIndex);
        }

        public HList<KeyValuePair<GroupKey, int>> ExecuteGroup()
        {
            if (!TryGroupFast())
            {
                AccumulateGroups();
                foreach (var kvp in _grouped)
                {
                    Add(kvp.Key, kvp.Value);
                }
            }

            if (_results.Count < limit)
            {
                Sort();
            }

            return _results;
        }

        private bool TryGroupFast()
        {
            //TODO: try merging it with existent indexes
            switch (_first | _second)
            {
                case GroupKeys.Sex:
                    return GroupBySex();
                case GroupKeys.Status:
                    return GroupByStatus();
                case GroupKeys.City:
                    return GroupByCity();
                case GroupKeys.Country:
                    return GroupByCountry();
                case GroupKeys.Interests:
                    return GroupByInterests();

                case GroupKeys.Country | GroupKeys.Sex:
                    return GroupByCountryAndSex();
                case GroupKeys.Country | GroupKeys.Status:
                    return GroupByCountryAndStatus();
                case GroupKeys.City | GroupKeys.Sex:
                    return GroupByCityAndSex();
                case GroupKeys.City | GroupKeys.Status:
                    return GroupByCityAndStatus();
            }

            return false;
        }

        private bool GroupBySex()
        {
            //TODO: Save this data at index level ??
            int countMale = 0, countFemale = 0;
            switch (_filters)
            {
                case FilterTypes.likes_one:
                {
                    LikeeIndexEntry entry = LikesIndexer.GetAllWhoLikes2(like);
                    countMale = entry.Men;
                    countFemale = entry.Women;
                    break;
                }
                case FilterTypes.city_eq:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCity(cityIndex);
                    countMale = group.TotalMen;
                    countFemale = group.TotalWomen;
                    break;
                }
                case FilterTypes.country_eq:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCountry(countryIndex);
                    countMale = group.TotalMen;
                    countFemale = group.TotalWomen;
                    break;
                }
                case FilterTypes.joined:
                {
                    var joinedYear = joinedFrom.GetYear();
                    GroupIndex.JoinedBySex(joinedYear, out countMale, out countFemale);
                    break;
                }
                case FilterTypes.birth_year:
                {
                    var birthYear = (ushort)birth;
                    GroupIndex.BornBySex(birthYear, out countMale, out countFemale);
                    break;
                }
                case FilterTypes.country_eq | FilterTypes.joined:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCountry(countryIndex);
                    countMale =  group.Joined(JoinedYear(), SexStatus.Male);
                    countFemale =  group.Joined(JoinedYear(), SexStatus.Female);
                    break;
                }
                case FilterTypes.city_eq | FilterTypes.joined:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCity(cityIndex);
                    countMale =  group.Joined(JoinedYear(), SexStatus.Male);
                    countFemale =  group.Joined(JoinedYear(), SexStatus.Female);
                    break;
                }
                case FilterTypes.country_eq | FilterTypes.birth_year:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCountry(countryIndex);
                    countMale =  group.Born(BirthYear(), SexStatus.Male);
                    countFemale =  group.Born(BirthYear(), SexStatus.Female);
                    break;
                }
                case FilterTypes.city_eq | FilterTypes.birth_year:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCity(cityIndex);
                    countMale =  group.Born(BirthYear(), SexStatus.Male);
                    countFemale =  group.Born(BirthYear(), SexStatus.Female);
                    break;
                }
                case FilterTypes.empty:
                {
                    countMale = GroupIndex.TotalMen;
                    countFemale = GroupIndex.TotalWomen;
                    break;
                }
                default:
                {
                    var predicate = FilterBuilder.GetFilter(_filters);
                    foreach (var accountId in GetAccounts())
                    {
                        Account account = Database.GetAccount(accountId);

                        if (!predicate(account, this))
                        {
                            continue;
                        }

                        if (account.SexStatus.IsMale())
                        {
                            countMale++;
                        }
                        else
                        {
                            countFemale++;
                        }

                    }
                    break;
                }
            }

            Add(new GroupKey(SexStatus.Male, 0, 0), countMale);
            Add(new GroupKey(SexStatus.Female, 0, 0), countFemale);
            return true;
        }

        private bool GroupByStatus()
        {
            int countFree = 0,countNotFree = 0, countComplex = 0;
            switch (_filters)
            {
                case FilterTypes.likes_one:
                {
                    LikeeIndexEntry entry = LikesIndexer.GetAllWhoLikes2(like);
                    countFree = entry.Free;
                    countNotFree = entry.NotFree;
                    countComplex = entry.Complex;
                    break;
                }
                case FilterTypes.city_eq:
                {
                    countFree = GroupIndex.SexGroupByCity(cityIndex).TotalFree;
                    countNotFree = GroupIndex.SexGroupByCity(cityIndex).TotalNotFree;
                    countComplex = GroupIndex.SexGroupByCity(cityIndex).TotalComplex;
                    break;
                }
                case FilterTypes.country_eq:
                {
                    countFree = GroupIndex.SexGroupByCountry(countryIndex).TotalFree;
                    countNotFree = GroupIndex.SexGroupByCountry(countryIndex).TotalNotFree;
                    countComplex = GroupIndex.SexGroupByCountry(countryIndex).TotalComplex;
                    break;
                }
                case FilterTypes.joined:
                {
                    var joinedYear = joinedFrom.GetYear();
                    countFree = GroupIndex.JoinedFree(joinedYear);
                    countNotFree = GroupIndex.JoinedNotFree(joinedYear);
                    countComplex = GroupIndex.JoinedComplex(joinedYear);;
                    break;
                }
                case FilterTypes.birth_year:
                {
                    var joinedYear = (ushort)birth;
                    GroupIndex.BornByStatus(joinedYear, out countFree, out countNotFree, out countComplex);
                    break;
                }
                case FilterTypes.country_eq | FilterTypes.joined:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCountry(countryIndex);
                    countFree =  group.Joined(JoinedYear(), SexStatus.Free);
                    countNotFree =  group.Joined(JoinedYear(), SexStatus.NotFree);
                    countComplex =  group.Joined(JoinedYear(), SexStatus.Complex);
                    break;
                }
                case FilterTypes.city_eq | FilterTypes.joined:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCity(cityIndex);
                    countFree =  group.Joined(JoinedYear(), SexStatus.Free);
                    countNotFree =  group.Joined(JoinedYear(), SexStatus.NotFree);
                    countComplex =  group.Joined(JoinedYear(), SexStatus.Complex);
                    break;
                }
                case FilterTypes.country_eq | FilterTypes.birth_year:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCountry(countryIndex);
                    countFree =  group.Born(BirthYear(), SexStatus.Free);
                    countNotFree =  group.Born(BirthYear(), SexStatus.NotFree);
                    countComplex =  group.Born(BirthYear(), SexStatus.Complex);
                    break;
                }
                case FilterTypes.city_eq | FilterTypes.birth_year:
                {
                    ref StatusGroup group = ref GroupIndex.SexGroupByCity(cityIndex);
                    countFree =  group.Born(BirthYear(), SexStatus.Free);
                    countNotFree =  group.Born(BirthYear(), SexStatus.NotFree);
                    countComplex =  group.Born(BirthYear(), SexStatus.Complex);
                    break;
                }
                case FilterTypes.empty:
                {
                    countFree = GroupIndex.TotalFree;
                    countNotFree = GroupIndex.TotalNotFree;
                    countComplex = GroupIndex.TotalComplex;
                    break;
                }

                default:
                {
                    IEnumerable<uint> accountIds = GetAccounts();
                    var predicate = FilterBuilder.GetFilter(_filters);
                    foreach (var accountId in accountIds)
                    {
                        Account account = Database.GetAccount(accountId);

                        if (!predicate(account, this))
                        {
                            continue;
                        }

                        switch (account.SexStatus & SexStatus.AllStatus)
                        {
                            case SexStatus.Free:
                                countFree++;
                                break;
                            case SexStatus.NotFree:
                                countNotFree++;
                                break;
                            case SexStatus.Complex:
                                countComplex++;
                                break;
                        }
                    }
                    break;
                }
            }

            Add(new GroupKey(SexStatus.Free, 0, 0), countFree);
            Add(new GroupKey(SexStatus.NotFree, 0, 0), countNotFree);
            Add(new GroupKey(SexStatus.Complex, 0, 0), countComplex);
            return true;
        }

        private bool GroupByCity()
        {
            //TODO: Add filters by sex and status.
            StringIndexer indexer = StringIndexer.Cities;
            HList<uint>[] lists = indexer.GetAllLists();
            if (_filters == FilterTypes.empty)
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, 0, index);
                    Add(group, lists[index].Count);
                }
            }
            else if(_filters == FilterTypes.status_eq || _filters == FilterTypes.sex_eq)
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, 0, index);
                    Add(group,GroupIndex.SexGroupByCity(index)[_sexStatus]);
                }
            }
            else if(_filters == FilterTypes.joined || _filters == (FilterTypes.joined | FilterTypes.status_eq) || _filters == (FilterTypes.joined | FilterTypes.sex_eq))
            {
                var joinedYear = joinedFrom.GetYear();
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, 0, index);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(index);
                    Add(group, statusGroup.Joined(joinedYear, _sexStatus));
                }
            }
            else if(_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year | FilterTypes.status_eq) || _filters == (FilterTypes.birth_year | FilterTypes.sex_eq))
            {
                ushort birthYear = (ushort) birth;
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, 0, index);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(index);
                    Add(group, statusGroup.Born(birthYear, _sexStatus));
                }
            }
            else if(_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                var interest = interestIndexes.First;
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, 0, index);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(index);

                    Add(group, statusGroup.Interests(interest, _sexStatus, JoinedYear(), BirthYear()));
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool GroupByCountry()
        {
            StringIndexer indexer = StringIndexer.Countries;
            HList<uint>[] lists = indexer.GetAllLists();
            //TODO: Add filters by sex and status.
            if (_filters == FilterTypes.empty)
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, index, 0);
                    Add(group, lists[index].Count);
                }
            }
            else if (_filters == FilterTypes.status_eq || _filters == FilterTypes.sex_eq)
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, index, 0);
                    Add(group, GroupIndex.SexGroupByCountry(index)[_sexStatus]);
                }
            }
            else if (_filters == FilterTypes.joined || _filters == (FilterTypes.joined | FilterTypes.status_eq) || _filters == (FilterTypes.joined | FilterTypes.sex_eq))
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, index, 0);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(index);
                    var joinedYear = joinedFrom.GetYear();
                    var count = statusGroup.Joined(joinedYear, _sexStatus);
                    Add(group, count);
                }
            }
            else if (_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year | FilterTypes.status_eq) || _filters == (FilterTypes.birth_year | FilterTypes.sex_eq))
            {
                ushort birthYear = (ushort) birth;

                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, index, 0);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(index);
                    var count = statusGroup.Born(birthYear, _sexStatus);
                    Add(group, count);
                }
            }
            else if (_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                for (ushort index = 0; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, 0, index, 0);
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(index);
                    var count = statusGroup.Interests(interestIndexes.First, _sexStatus, JoinedYear(), BirthYear());
                    Add(group, count);
                }
            }
            else
            {
                Span<int> countries = stackalloc int[(int)indexer.Count + 1];
                IEnumerable<uint> accountIds = GetAccounts();

                var predicate = FilterBuilder.GetFilter(_filters);

                foreach (var accountId in accountIds)
                {
                    Account account = Database.GetAccount(accountId);
                    if (!predicate(account, this))
                    {
                        continue;
                    }

                    countries[account.countryIndex]++;
                }

                for (ushort country = 0; country < countries.Length; country++)
                {
                    Add(new GroupKey(SexStatus.None,0,country,0 ), countries[country]);
                }
            }

            return true;
        }

        private bool GroupByInterests()
        {
            StringIndexer indexer = StringIndexer.Interests;
            var lists = indexer.GetAllLists();

            if (_filters == FilterTypes.empty)
            {
                for (ushort index = 1; index <= indexer.Count; index++)
                {
                    var group = new GroupKey(SexStatus.None, index, 0, 0);
                    Add(group, lists[index].Count);
                }
            }
            else if (_filters == FilterTypes.joined)
            {
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, JoinedInterestIndex.GetInterestsJoinedCount(joinedFrom.GetYear(), interest));
                }
            }
            else if (_filters == FilterTypes.birth_year)
            {
                var birthYear = (ushort) birth;
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, JoinedInterestIndex.GetInterestsBirthCount(birthYear, interest));
                }
            }
            else if(_filters == FilterTypes.country_eq)
            {
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, RecommendIndex.GetCountByCountry(countryIndex, interest));
                }
            }
            else if(_filters == FilterTypes.city_eq)
            {
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, RecommendIndex.GetCountByCity(cityIndex, interest));
                }
            }
            else if (_filters.ContainsAll(FilterTypes.country_eq) && _filters.ContainsAny(FilterTypes.birth_year | FilterTypes.joined))
            {
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(countryIndex);

                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, statusGroup.Interests(interest, SexStatus.None, JoinedYear(), BirthYear()));
                }
            }

            else if (_filters.ContainsAll(FilterTypes.city_eq) && _filters.ContainsAny(FilterTypes.birth_year | FilterTypes.joined))
            {
                for (ushort interest = 1; interest <= indexer.Count; interest++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(cityIndex);

                    var group = new GroupKey(SexStatus.None, interest, 0, 0);
                    Add(group, statusGroup.Interests(interest, SexStatus.None, JoinedYear(), BirthYear()));
                }
            }
            else
            {
                Span<int> interests = stackalloc int[(int)indexer.Count + 1];

                IEnumerable<uint> accountIds = GetAccounts();

                var predicate = FilterBuilder.GetFilter(_filters);

                foreach (var accountId in accountIds)
                {
                    Account account = Database.GetAccount(accountId);
                    if (account.InterestIndexes.Count == 0 || !predicate(account, this))
                    {
                        continue;
                    }

                    foreach (ushort index in account.InterestIndexes)
                    {
                        interests[index]++;
                    }
                }

                for (ushort interest = 1; interest < interests.Length; interest++)
                {
                    Add(new GroupKey(SexStatus.None,interest,0 ), interests[interest]);
                }
            }

            return true;
        }

        private bool GroupByCountryAndSex()
        {
            if (_filters == FilterTypes.empty || _filters == FilterTypes.status_eq || _filters == FilterTypes.sex_eq)
            {
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var menGroup = new GroupKey(SexStatus.Male, 0, country, 0);
                    Add(menGroup, statusGroup[_sexStatus | SexStatus.Male]);

                    var womenGroup = new GroupKey(SexStatus.Female, 0, country, 0);
                    Add(womenGroup, statusGroup[_sexStatus | SexStatus.Female]);
                }
            }
            else if (_filters == FilterTypes.joined || _filters == (FilterTypes.joined | FilterTypes.status_eq)  || _filters == (FilterTypes.joined | FilterTypes.sex_eq))
            {
                var joinedYear = joinedFrom.GetYear();
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var menGroup = new GroupKey(SexStatus.Male, 0, country, 0);
                    Add(menGroup, statusGroup.Joined(joinedYear, _sexStatus | SexStatus.Male));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, country, 0);
                    Add(womenGroup, statusGroup.Joined(joinedYear, _sexStatus |SexStatus.Female));
                }
            }
            else if (_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year | FilterTypes.status_eq)  || _filters == (FilterTypes.birth_year | FilterTypes.sex_eq))
            {
                var birthYear = (ushort) birth;
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var menGroup = new GroupKey(SexStatus.Male, 0, country, 0);
                    Add(menGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Male));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, country, 0);
                    Add(womenGroup, statusGroup.Born(birthYear, _sexStatus |SexStatus.Female));
                }
            }
            else if (_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                var interest = interestIndexes.First;
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var menGroup = new GroupKey(SexStatus.Male, 0, country, 0);
                    Add(menGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Male, JoinedYear(), BirthYear()));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, country, 0);
                    Add(womenGroup, statusGroup.Interests(interest, _sexStatus |SexStatus.Female, JoinedYear(), BirthYear()));
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool GroupByCountryAndStatus()
        {
            if (_filters == FilterTypes.empty || _filters == FilterTypes.sex_eq || _filters == FilterTypes.status_eq)
            {
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, country, 0);
                    Add(freeGroup, statusGroup[_sexStatus | SexStatus.Free]);

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, country, 0);
                    Add(notFreeGroup, statusGroup[_sexStatus | SexStatus.NotFree]);

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, country, 0);
                    Add(complexGroup, statusGroup[_sexStatus | SexStatus.Complex]);
                }
            }
            else if (_filters == FilterTypes.joined || _filters == (FilterTypes.joined |FilterTypes.sex_eq) || _filters == (FilterTypes.joined |FilterTypes.status_eq))
            {
                var joined = joinedFrom.GetYear();
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, country, 0);
                    Add(freeGroup, statusGroup.Joined(joined, _sexStatus | SexStatus.Free));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, country, 0);
                    Add(notFreeGroup, statusGroup.Joined(joined, _sexStatus | SexStatus.NotFree));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, country, 0);
                    Add(complexGroup, statusGroup.Joined(joined, _sexStatus | SexStatus.Complex));
                }
            }
            else if (_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year |FilterTypes.sex_eq) || _filters == (FilterTypes.birth_year |FilterTypes.status_eq))
            {
                ushort birthYear = (ushort) birth;
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, country, 0);
                    Add(freeGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Free));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, country, 0);
                    Add(notFreeGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.NotFree));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, country, 0);
                    Add(complexGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Complex));
                }
            }
            else if(_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                var interest = interestIndexes.First;
                for (ushort country = 0; country <= StringIndexer.Countries.Count; country++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCountry(country);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, country, 0);

                    Add(freeGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Free, JoinedYear(), BirthYear()));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, country, 0);
                    Add(notFreeGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.NotFree, JoinedYear(), BirthYear()));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, country, 0);
                    Add(complexGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Complex, JoinedYear(), BirthYear()));
                }
            }

            else
            {
                return false;
            }

            return true;
        }

        private bool GroupByCityAndSex()
        {
            if (_filters == FilterTypes.empty || _filters == FilterTypes.status_eq || _filters == FilterTypes.sex_eq)
            {
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var menGroup = new GroupKey(SexStatus.Male, 0, 0, city);
                    Add(menGroup, statusGroup[_sexStatus | SexStatus.Male]);

                    var womenGroup = new GroupKey(SexStatus.Female, 0, 0, city);
                    Add(womenGroup, statusGroup[_sexStatus | SexStatus.Female]);
                }
            }
            else if (_filters == FilterTypes.joined || _filters == (FilterTypes.joined | FilterTypes.status_eq) || _filters == (FilterTypes.joined | FilterTypes.sex_eq))
            {
                var joinedYear = joinedFrom.GetYear();
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var menGroup = new GroupKey(SexStatus.Male, 0, 0, city);
                    Add(menGroup, statusGroup.Joined(joinedYear, _sexStatus | SexStatus.Male));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, 0, city);
                    Add(womenGroup, statusGroup.Joined(joinedYear, _sexStatus | SexStatus.Female));
                }
            }
            else if (_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year | FilterTypes.status_eq) || _filters == (FilterTypes.birth_year | FilterTypes.sex_eq))
            {
                ushort birthYear = (ushort) birth;
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var menGroup = new GroupKey(SexStatus.Male, 0, 0, city);
                    Add(menGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Male));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, 0, city);
                    Add(womenGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Female));
                }
            }
            else if (_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                var interest = interestIndexes.First;
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var menGroup = new GroupKey(SexStatus.Male, 0, 0, city);
                    Add(menGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Male, JoinedYear(), BirthYear()));

                    var womenGroup = new GroupKey(SexStatus.Female, 0, 0, city);
                    Add(womenGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Female, JoinedYear(), BirthYear()));
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool GroupByCityAndStatus()
        {
            if (_filters == FilterTypes.empty || _filters == FilterTypes.sex_eq || _filters == FilterTypes.status_eq)
            {
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, 0, city);
                    Add(freeGroup, statusGroup[_sexStatus | SexStatus.Free]);

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, 0, city);
                    Add(notFreeGroup,statusGroup[_sexStatus | SexStatus.NotFree]);

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, 0, city);
                    Add(complexGroup, statusGroup[_sexStatus | SexStatus.Complex]);
                }
            }
            else if(_filters == FilterTypes.joined || _filters == (FilterTypes.joined | FilterTypes.status_eq) || _filters == (FilterTypes.joined | FilterTypes.sex_eq))
            {
                var joined = joinedFrom.GetYear();
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, 0, city);
                    Add(freeGroup, statusGroup.Joined(joined, _sexStatus | SexStatus.Free));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, 0, city);
                    Add(notFreeGroup,statusGroup.Joined(joined, _sexStatus | SexStatus.NotFree));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, 0, city);
                    Add(complexGroup, statusGroup.Joined(joined, _sexStatus | SexStatus.Complex));
                }
            }
            else if(_filters == FilterTypes.birth_year || _filters == (FilterTypes.birth_year | FilterTypes.status_eq) || _filters == (FilterTypes.birth_year | FilterTypes.sex_eq))
            {
                ushort birthYear = (ushort) birth;
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, 0, city);
                    Add(freeGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Free));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, 0, city);
                    Add(notFreeGroup,statusGroup.Born(birthYear, _sexStatus | SexStatus.NotFree));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, 0, city);
                    Add(complexGroup, statusGroup.Born(birthYear, _sexStatus | SexStatus.Complex));
                }
            }
            else if(_filters == FilterTypes.interests_any || _filters.ContainsAny(FilterTypes.interests_any) && _filters.ContainsAny(FilterTypes.joined | FilterTypes.birth_year))
            {
                var interest = interestIndexes.First;
                for (ushort city = 0; city <= StringIndexer.Cities.Count; city++)
                {
                    ref StatusGroup statusGroup = ref GroupIndex.SexGroupByCity(city);

                    var freeGroup = new GroupKey(SexStatus.Free, 0, 0, city);
                    Add(freeGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Free, JoinedYear(), BirthYear()));

                    var notFreeGroup = new GroupKey(SexStatus.NotFree, 0, 0, city);
                    Add(notFreeGroup,statusGroup.Interests(interest, _sexStatus | SexStatus.NotFree, JoinedYear(), BirthYear()));

                    var complexGroup = new GroupKey(SexStatus.Complex, 0, 0, city);
                    Add(complexGroup, statusGroup.Interests(interest, _sexStatus | SexStatus.Complex, JoinedYear(), BirthYear()));
                }
            }

            else
            {
                return false;
            }

            return true;
        }

        private ushort BirthYear() => (ushort) (birth == 0 ? 0 : birth);

        private ushort JoinedYear() => joinedFrom == 0 ? (ushort)0 :  joinedFrom.GetYear();


        private void AccumulateGroups()
        {
            IEnumerable<uint> accountIds = GetAccounts();
            Predicate predicate = FilterBuilder.GetFilter(_filters);

            foreach (var accountId in accountIds)
            {
                Account account = Database.GetAccount(accountId);
                if (!predicate(account, this))
                {
                    continue;
                }

                GroupKey key = GetKey(account, 0);
                _grouped.AddOrUpdate(key, 1);
            }

        }

        private void Add(in GroupKey groupKey, int count)
        {
            if (count > 0)
            {
                var item = new KeyValuePair<GroupKey, int>(groupKey, count);
                if (_results.Count < (limit-1))
                {
                    _results.Add(item);
                    return;
                }
                if (_results.Count == limit - 1)
                {
                    _results.Add(item);
                    Sort();
                    return;
                }

                KeyValuePair<GroupKey,int> last = _results[(uint) (_results.Count-1)];
                if (Compare(last, item) > 0)
                {
                    _results.RemoveAt(_results.Count - 1);
                    var insertAt = ~_results.BinarySearch(0, _results.Count, item, this);
                    _results.Insert(insertAt, item);
                }
            }
        }

        private void Sort() => _results.Sort(this);
    }
}