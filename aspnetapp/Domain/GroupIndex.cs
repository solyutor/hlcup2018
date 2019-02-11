using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public static class GroupIndex
    {
        private static readonly StatusGroup[] ByCountries = new StatusGroup[StringIndexer.MaxCountryCount+1];
        private static readonly StatusGroup[] ByCities = new StatusGroup[StringIndexer.MaxCityCount+1];

        public static int TotalMen { get; private set; }
        public static int TotalWomen { get; private set; }

        public static int TotalFree { get; private set; }
        public static int TotalNotFree { get; private set; }
        public static int TotalComplex { get; private set; }

        public static ref StatusGroup SexGroupByCountry(ushort country) => ref ByCountries[country];

        public static ref StatusGroup SexGroupByCity(ushort city) => ref ByCities[city];



        private static readonly int[] _joinedByStatus = new int[30];
        private static readonly int[] _joinedBySex = new int[20];

        private static readonly int[] _bornByStatus = new int[3 * 60];
        private static readonly int[] _bornBySex = new int[2 * 60];

        public static int JoinedFree(ushort joinedYear)
        {
            return _joinedByStatus[joinedYear.GetJoinedOffset()];
        }
        public static int JoinedNotFree(ushort joinedYear)
        {
            return _joinedByStatus[joinedYear.GetJoinedOffset()+10];
        }

        public static int JoinedComplex(ushort joinedYear)
        {
            return _joinedByStatus[joinedYear.GetJoinedOffset()+20];
        }

        public static void JoinedBySex(ushort joinedYear, out int countMale, out int countFemale)
        {
            var offset = joinedYear.GetJoinedOffset();
            countMale = _joinedBySex[offset];
            countFemale = _joinedBySex[offset + 10];
        }

        public static void BornBySex(ushort birthYear, out int countMale, out int countFemale)
        {
            var offset = birthYear.GetBirthOffset();
            countMale = _bornBySex[offset];
            countFemale = _bornBySex[offset + 60];
        }

        public static void BornByStatus(ushort birthYear, out int countFree, out int countNotFree, out int countComplex)
        {
            var offset = birthYear.GetBirthOffset();
            countFree = _bornByStatus[offset];
            countNotFree = _bornByStatus[offset + 60];
            countComplex = _bornByStatus[offset + 120];
        }


        public static void Index(Account account)
        {
            IndexByCountry(account);
            IndexByCity(account);
        }

        private static void IndexByCountry(Account account)
        {
            checked
            {
                SexGroup group = GetSexGroupByCountry(account.countryIndex, account.SexStatus);
                group.Index(account);

                switch (account.SexStatus & SexStatus.AllStatus)
                {
                    case SexStatus.Free:
                        TotalFree++;
                        _joinedByStatus[account.GetJoinedOffset()]++;
                        _bornByStatus[account.GetBirthOffset()]++;
                        break;
                    case SexStatus.NotFree:
                        TotalNotFree++;
                        _joinedByStatus[account.GetJoinedOffset()+10]++;
                        _bornByStatus[account.GetBirthOffset()+60]++;
                        break;
                    case SexStatus.Complex:
                        TotalComplex++;
                        _joinedByStatus[account.GetJoinedOffset()+20]++;
                        _bornByStatus[account.GetBirthOffset()+120]++;
                        break;
                }

                if (account.SexStatus.IsMale())
                {
                    TotalMen++;
                    _joinedBySex[account.GetJoinedOffset()]++;
                    _bornBySex[account.GetBirthOffset()]++;
                }
                else
                {
                    TotalWomen++;
                    _joinedBySex[account.GetJoinedOffset()+10]++;
                    _bornBySex[account.GetBirthOffset()+60]++;
                }

            }

            if (account.countryIndex > 0)
            {
                SexGroup notNullGroup = GetSexGroupByCountry(StringIndexer.MaxCountryCount, account.SexStatus);
                notNullGroup.Index(account);
            }
        }

        private static void IndexByCity(Account account)
        {
            SexGroup group = GetSexGroupByCity(account.cityIndex, account.SexStatus);
            group.Index(account);

            if (account.cityIndex > 0)
            {
                SexGroup notNullGroup = GetSexGroupByCity(StringIndexer.MaxCityCount, account.SexStatus);
                notNullGroup.Index(account);
            }
        }

        public static void Remove(Account account)
        {
            RemoveByCountry(account);
            RemoveByCity(account);
        }

        private static void RemoveByCountry(Account account)
        {
            checked
            {
                SexGroup group = GetSexGroupByCountry(account.countryIndex, account.SexStatus);
                group.Remove(account);

                if (account.SexStatus.IsMale())
                {
                    TotalMen--;
                    _joinedBySex[account.GetJoinedOffset()]--;
                    _bornBySex[account.GetBirthOffset()]--;
                }
                else
                {
                    TotalWomen--;
                    _joinedBySex[account.GetJoinedOffset() + 10]--;
                    _bornBySex[account.GetBirthOffset()+60]--;
                }

                checked
                {
                    switch (account.SexStatus & SexStatus.AllStatus)
                    {
                        case SexStatus.Free:
                            TotalFree--;
                            _joinedByStatus[account.GetJoinedOffset()]--;
                            _bornByStatus[account.GetBirthOffset()]--;
                            break;
                        case SexStatus.NotFree:
                            TotalNotFree--;
                            _joinedByStatus[account.GetJoinedOffset()+10]--;
                            _bornByStatus[account.GetBirthOffset()+60]--;
                            break;
                        case SexStatus.Complex:
                            TotalComplex--;
                            _joinedByStatus[account.GetJoinedOffset()+20]--;
                            _bornByStatus[account.GetBirthOffset()+120]--;
                            break;
                    }
                }
            }
            if (account.countryIndex > 0)
            {
                SexGroup notNullGroup = GetSexGroupByCountry(StringIndexer.MaxCountryCount, account.SexStatus);
                notNullGroup.Remove(account);
            }
        }

        private static void RemoveByCity(Account account)
        {
            SexGroup group = GetSexGroupByCity(account.cityIndex, account.SexStatus);
            group.Remove(account);

            if (account.cityIndex > 0)
            {
                SexGroup notNullGroup = GetSexGroupByCity(StringIndexer.MaxCityCount, account.SexStatus);
                notNullGroup.Remove(account);
            }
        }


        public static void Trim()
        {
            foreach (var statusGroup in ByCities)
            {
                statusGroup.Trim();
            }

            foreach (var statusGroup in ByCountries)
            {
                statusGroup.Trim();
            }
        }

        private static SexGroup GetSexGroupByCountry(ushort countryIndex, SexStatus sexStatus)
        {
            return GetSexGroup(ByCountries, countryIndex, sexStatus);
        }

        private static SexGroup GetSexGroupByCity(ushort cityIndex, SexStatus sexStatus)
        {
            return GetSexGroup(ByCities, cityIndex, sexStatus);
        }

        private static SexGroup GetSexGroup(StatusGroup[] statusGroups, ushort index, SexStatus sexStatus)
        {
            ref StatusGroup statusGroup = ref statusGroups[index];

            if (statusGroup.IsEmpty)
            {
                statusGroup = new StatusGroup(100);
            }

            switch (sexStatus & SexStatus.AllStatus)
            {
                case SexStatus.Free:
                    return statusGroup.Free;
                case SexStatus.NotFree:
                    return statusGroup.NotFree;
                case SexStatus.Complex:
                    return statusGroup.Complex;
            }

            throw new InvalidOperationException("This should never happen");
        }
    }
}