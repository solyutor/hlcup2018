using System.Collections.Generic;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    //TODO: merge suggest and recommend handlers
    public static class SuggestQueryParser
    {
        public static bool TryParse(QueryCollection query, ref SuggestQuery suggestQuery)
        {
            if (suggestQuery == null)
            {
                suggestQuery = new SuggestQuery();
            }
            else
            {
                suggestQuery.Reset();
            }

            foreach (KeyValuePair<string, StringValues> kvp in query)
            {
                if (!Parameters.TryGetValue(kvp.Key, out Parser<SuggestQuery> parser) || !parser(suggestQuery, kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static readonly HOrdinalDict<Parser<SuggestQuery>> Parameters = new HOrdinalDict<Parser<SuggestQuery>>(5)
        {
            {"country", (f, v) => f.AddCountry(v[0])}, //eq - everyone who lives in a particular country;
            {"city", (f, v) => f.AddCity(v[0])}, //eq - everyone who lives in a particular city;

            {"limit", (f, v) => int.TryParse(v.ToString(), out f.limit) && f.limit > 0},
            {"query_id", (f, v) => true}
        };
    }
}