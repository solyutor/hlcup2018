using System;
using System.Collections.Generic;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    public static class RecommendQueryParser
    {
        public static bool TryParse(IQueryCollection query, ref RecommendQuery recommendQuery)
        {
            if (recommendQuery == null)
            {
                recommendQuery = new RecommendQuery();
            }
            else
            {
                recommendQuery.Reset();
            }
            foreach (KeyValuePair<string, StringValues> kvp in query)
            {
                if (!Parameters.TryGetValue(kvp.Key, out Parser<RecommendQuery> parser) || !parser(recommendQuery, kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static readonly HOrdinalDict<Parser<RecommendQuery>> Parameters = new HOrdinalDict<Parser<RecommendQuery>>(5)
        {
            {"country", (f, v) => f.AddCountry(v[0])}, //eq - everyone who lives in a particular country;
            {"city", (f, v) => f.AddCity(v[0])}, //eq - everyone who lives in a particular city;

            {"limit", (f, v) => int.TryParse(v.ToString(), out f.limit) && f.limit > 0},
            {"query_id", (f, v) => true}
        };
    }
}