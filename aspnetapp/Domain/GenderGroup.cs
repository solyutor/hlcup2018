using System;
using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Controllers;

namespace aspnetapp.Domain
{
    public readonly struct GenderGroup
    {
        private static readonly HList<uint> Empty = new HList<uint>();
        private readonly HList<uint>[] _joined;
        private readonly HList<uint>[] _birth;
        private readonly HList<uint> _all;

        private readonly ushort[] _joinedInterests;
        private readonly ushort[] _birthInterests;
        private readonly ushort[] _interestTotal;
        private readonly HList<uint>[] _phoneCode;
        public int Count => _all?.Count ?? 0;

        private const int MaxInterestCount = StringIndexer.MaxInterestCount;

        public GenderGroup(int size)
        {
            _all = new HList<uint>(size);
            _joined = Initialize(10);
            _birth = Initialize(60);
            //consider changing to byte
            _joinedInterests = new ushort[10 * MaxInterestCount]; //maxjoined * max interests
            _birthInterests = new ushort[60 * MaxInterestCount]; // max birth * max interests
            _interestTotal = new ushort[MaxInterestCount];
            _phoneCode = new HList<uint>[100];
        }

        public bool IsEmpty => _all == null;
        public HList<uint> All => _all ?? Empty;

        public void TrimExcess()
        {
            _all?.TrimExcess();
            foreach (var list in _joined ?? Array.Empty<HList<uint>>())
            {
                list?.TrimExcess();
            }
            foreach (var list in _birth ?? Array.Empty<HList<uint>>())
            {
                list?.TrimExcess();
            }
            foreach (var list in _phoneCode ?? Array.Empty<HList<uint>>())
            {
                list?.TrimExcess();
            }
        }

        public void Index(Account account)
        {
            if (_all.InsertDescending(account.id))
            {
                var joinedOffset = account.GetJoinedOffset();
                var birthOffset = account.GetBirthOffset();
                _joined[joinedOffset].InsertDescending(account.id);
                _birth[birthOffset].InsertDescending(account.id);

                foreach (var interest in account.InterestIndexes)
                {
                    _joinedInterests[joinedOffset * MaxInterestCount + interest]++;
                    _birthInterests[birthOffset * MaxInterestCount + interest]++;
                    _interestTotal[interest]++;
                }

                if (!account.phone.IsEmpty)
                {
                    var codeOffset = PhoneCodeOffset(account.phone.Span.Slice(4, 2));
                    ref HList<uint> byCodes = ref _phoneCode[codeOffset];
                    if (byCodes == null)
                    {
                        byCodes = new HList<uint>();
                    }

                    byCodes.InsertDescending(account.id);
                }


            }
        }
        public void Remove(Account account)
        {
            if (_all.RemoveDescending(account.id))
            {
                var joinedOffset = account.GetJoinedOffset();
                var birthOffset = account.GetBirthOffset();

                _joined[joinedOffset].RemoveDescending(account.id);
                _birth[birthOffset].RemoveDescending(account.id);

                foreach (var interest in account.InterestIndexes)
                {
                    _joinedInterests[joinedOffset * MaxInterestCount + interest]--;
                    _birthInterests[birthOffset * MaxInterestCount + interest]--;
                    _interestTotal[interest]--;
                }

                if (!account.phone.IsEmpty)
                {
                    var codeOffset = PhoneCodeOffset(account.phone.Span.Slice(4, 2));
                    ref HList<uint> byCodes = ref _phoneCode[codeOffset];
                    byCodes.RemoveDescending(account.id);
                }
            }
        }

        public int JoinedCount(ushort joinedYear) => _joined[joinedYear.GetJoinedOffset()]?.Count ?? 0;

        public int BornCount(ushort birthYear) => _birth[birthYear.GetBirthOffset()]?.Count ?? 0;
        public int InterestsCount(ushort interest, ushort joinedYear, ushort birthYear)
        {
            if (_joinedInterests == null)
            {
                return 0;
            }

            if (joinedYear > 0)
            {
                var joinedOffset = joinedYear.GetJoinedOffset();
                return _joinedInterests[joinedOffset * MaxInterestCount + interest];
            }

            if (birthYear > 0)
            {
                var birthOffset = birthYear.GetBirthOffset();
                return _birthInterests[birthOffset * MaxInterestCount + interest];
            }

            return _interestTotal[interest];
        }


        private static HList<uint>[] Initialize(int size)
        {
            var result = new HList<uint>[size];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new HList<uint>();
            }

            return result;
        }


        public HList<uint> Born(ushort birthYear)
        {
            return _birth[birthYear.GetBirthOffset()];
        }

        private static byte PhoneCodeOffset(Span<byte> codeSpan)
        {
            byte offset = (byte) (((codeSpan[0] - '0') * 10) + ((codeSpan[1] - '0')));
            return offset;
        }

        public HList<uint> ByPhoneCode(byte codeOffset)
        {
            return _phoneCode[codeOffset] ?? HList<uint>.Empty;
        }
    }
}