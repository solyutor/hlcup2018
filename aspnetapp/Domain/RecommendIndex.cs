using System.Collections.Generic;
using aspnetapp.Collections;

namespace aspnetapp
{
    public static class RecommendIndex
    {
        private static RecommendIndexGroup _all;
        private static RecommendIndexGroup[] _byCity;
        private static RecommendIndexGroup[] _byCountry;

        static RecommendIndex()
        {
            _all = new RecommendIndexGroup(StringIndexer.MaxInterestCount);
            _byCity = new RecommendIndexGroup[StringIndexer.MaxCityCount];
            _byCountry = new RecommendIndexGroup[StringIndexer.MaxCountryCount];

            for (int i = 0; i < _byCity.Length; i++)
            {
                _byCity[i] = new RecommendIndexGroup(StringIndexer.MaxInterestCount);
            }

            for (int i = 0; i < _byCountry.Length; i++)
            {
                _byCountry[i] = new RecommendIndexGroup(StringIndexer.MaxInterestCount);
            }
        }

        public static void GetUnionForRecommendQuery(Account account, RecMultiUnionList union, ushort cityIndex, ushort countryIndex)
        {
            if (0 < cityIndex && cityIndex < ushort.MaxValue)
            {
                _byCity[cityIndex].GetUnionForRecommendQuery(account, union);
                return;
            }

            if (0 < countryIndex && countryIndex < ushort.MaxValue)
            {
                _byCountry[countryIndex].GetUnionForRecommendQuery(account, union);
                return;
            }
            _all.GetUnionForRecommendQuery(account, union);
        }

        public static void Index(Account account)
        {
            _all.Index(account);
            if (account.cityIndex > 0)
            {
                ref RecommendIndexGroup group = ref _byCity[account.cityIndex];
                if (group.IsEmpty)
                {
                    group = new RecommendIndexGroup(StringIndexer.MaxInterestCount);
                }

                group.Index(account);
            }

            if (account.countryIndex > 0)
            {
                ref RecommendIndexGroup group = ref _byCountry[account.countryIndex];
                if (group.IsEmpty)
                {
                    group = new RecommendIndexGroup(StringIndexer.MaxInterestCount);
                }

                group.Index(account);
            }
        }

        public static void Remove(Account account)
        {
            _all.Remove(account);
            if (account.cityIndex > 0)
            {
                _byCity[account.cityIndex].Remove(account);
            }

            if (account.countryIndex > 0)
            {
                _byCountry[account.countryIndex].Remove(account);
            }
        }

        public static int GetCountByCountry(ushort countryIndex, ushort interest)
        {
            return _byCountry[countryIndex].CountAllBy(interest);
        }
        public static int GetCountByCity(ushort cityIndex, ushort interest)
        {
            return _byCity[cityIndex].CountAllBy(interest);
        }
    }
}