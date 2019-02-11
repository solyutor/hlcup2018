using aspnetapp.Collections;
using aspnetapp.Domain;

namespace aspnetapp
{
    public struct RecommendIndexGroup
    {
        private readonly HList<RecommendIndexEntry>[] _men;
        private readonly HList<RecommendIndexEntry>[] _women;

        public RecommendIndexGroup(int arraySize)
        {
            _men = new HList<RecommendIndexEntry>[arraySize];
            _women = new HList<RecommendIndexEntry>[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                _men[i] = new HList<RecommendIndexEntry>();
                _women[i] = new HList<RecommendIndexEntry>();
            }
        }

        public bool IsEmpty => _men == null;

        public void Index(Account account)
        {
            foreach (var interest in account.InterestIndexes)
            {
                var entry = new RecommendIndexEntry(account);
                if (account.SexStatus.IsMale())
                {
                    _men[interest].InsertDescending(entry);
                }
                else
                {
                    _women[interest].InsertDescending(entry);
                }
            }

        }

        public void Remove(Account account)
        {
            foreach (var interest in account.InterestIndexes)
            {
                var entry = new RecommendIndexEntry(account);
                if (account.SexStatus.IsMale())
                {
                    _men[interest].RemoveDescending(entry);
                }
                else
                {
                    _women[interest].RemoveDescending(entry);
                }
            }
        }

        public void GetUnionForRecommendQuery(Account account, RecMultiUnionList union)
        {
            HList<RecommendIndexEntry>[] source = account.SexStatus.IsMale() ? _women : _men;
            foreach (var interest in account.InterestIndexes)
            {
                HList<RecommendIndexEntry> entries = source[interest];
                union.Add(entries);
            }
        }

        public int CountAllBy(ushort interest)
        {
            return _men[interest].Count + _women[interest].Count;
        }
    }
}