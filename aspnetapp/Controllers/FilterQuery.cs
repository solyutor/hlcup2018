using System.Collections.Generic;
using System.Linq;
using aspnetapp.Collections;

namespace aspnetapp.Controllers
{
    public class FilterQuery : AbstractQuery
    {
        private readonly HList<Account> _result = new HList<Account>(50);

        public HList<Account> ExecuteFilter()
        {
            IEnumerable<uint> indexToScan = GetAccounts();

            Predicate predicate = FilterBuilder.GetFilter(_filters);

            _result.Clear();

            foreach (var accountId in indexToScan)
            {
                var account = Database.GetAccount(accountId);
                if (account != null && predicate(account, this))
                {
                    _result.Add(account);

                    if (_result.Count == limit)
                    {
                        break;
                    }
                }

            }

            return _result;
        }
    }
}