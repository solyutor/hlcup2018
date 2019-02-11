using System.Collections.Generic;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    internal class GroupQueryParser
    {
        public static bool TryParse(IQueryCollection query, ref GroupQuery groupQuery)
        {
            if (groupQuery == null)
            {
                groupQuery = new GroupQuery();
            }
            else
            {
                groupQuery.Reset();
            }

            foreach (KeyValuePair<string, StringValues> kvp in query)
            {
                if (!Parameters.TryGetValue(kvp.Key, out Parser<GroupQuery> parser) || !parser(groupQuery, kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static readonly HOrdinalDict<Parser<GroupQuery>> Parameters
            = new HOrdinalDict<Parser<GroupQuery>>(15)
            {
                {"limit", (q, v) => int.TryParse(v[0], out q.limit)},
                {"order", (q, v) => q.AddOrder(v[0])}, //1 or -1
                {"keys", (q, v) => q.AddKeys(v[0])},

                {"sex", (q, v) => q.AddSex(v[0])},
                {"status", (q, v) => q.AddEqStatus(v[0])},
                //All of the account fields.

                {"fname", (q, v) => q.AddFirstName(v[0])},
                {"sname", (q, v) => q.AddLastName(v[0])},

                {"birth", (q, v) => q.AddBirthYear(v[0])},
                {"joined", (q, v) => q.AddJoined(v[0])},

                {"city", (q, v) => q.AddCity(v[0])},
                {"country", (q, v) => q.AddCountry(v[0])},

                {"likes", (q, v) => q.AddLike(v[0])},
                {"interests", (q, v) => q.AddInterestsAny(v[0])},

                {"query_id", (q, v) => true}
            };
    }
}