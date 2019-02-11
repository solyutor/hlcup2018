using System.Collections.Generic;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    public static class FilterQueryParser
    {
        public static bool TryParse(IQueryCollection query, ref FilterQuery filterQuery)
        {
            if (filterQuery == null)
            {
                filterQuery = new FilterQuery();
            }
            else
            {
                filterQuery.Reset();
            }

            foreach (KeyValuePair<string, StringValues> kvp in query)
            {
                if (!Parameters.TryGetValue(kvp.Key, out Parser<AbstractQuery> parser) || !parser(filterQuery, kvp.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static readonly HOrdinalDict<Parser<AbstractQuery>> Parameters = new HOrdinalDict<Parser<AbstractQuery>>(30)
        {
            //eq - matching specific gender - "m" или "f";
            {"sex_eq", (f, v) => f.AddSex(v[0])},

            {"email_domain", (f, v) => f.AddEmailDomain(v[0])}, //domain - select all whose emails have the specified domain;
            {"email_lt", (f, v) => f.AddEmailLt(v[0])}, // select all whose emails are lexicographically earlier;
            {"email_gt", (f, v) => f.AddEmailGt(v[0])}, //  same but lexicographically later;

            // matching specific status;
            {"status_eq", (f, v) => f.AddEqStatus(v[0])},
            //select all whose status not matching specified one;
            {"status_neq", (f, v) => f.AddNeqStatus(v[0])},

            {"fname_eq", (f, v) => f.AddFirstName(v[0])}, //eq - matching specific first name;
            {"fname_any", (f, v) => f.AddFirstNames(v[0])}, //any - in any of the names listed separated by commas;
            {"fname_null", (f, v) => f.HasFirstName(v[0])}, //null - select all those who have a first name specified (if 0) or not (if 1);

            {"sname_eq", (f, v) => f.AddLastName(v[0])}, //eq - matching specific last name;
            {"sname_starts", (f, v) => f.AddLastNamePrefix(v[0])}, //starts - select all whose last names begin with the transmitted prefix;
            {"sname_null", (f, v) => f.HasLastName(v[0])}, //null - select all those who have a last name specified (if 0) or not (if 1);

            {"phone_code", (f, v) => f.AddPhoneCode(v[0])}, //code - select everyone who has a specific code on the phone (three digits in brackets);
            {"phone_null", (f, v) => f.HasPhone(v[0])}, //null - similar to the rest of the fields;

            {"country_eq", (f, v) => f.AddCountry(v[0])}, //eq - everyone who lives in a particular country;
            {"country_null", (f, v) => f.HasCountry(v[0])}, //null - similar to the rest of the fields;

            {"city_eq", (f, v) => f.AddCity(v[0])}, //eq - everyone who lives in a particular city;
            {"city_any", (f, v) => f.AddCities(v[0])}, //any - in any of the cities listed separated by commas;
            {"city_null", (f, v) => f.HasCity(v[0])}, //null - similar;

            {"birth_lt", (f, v) => f.AddBirthLt(v[0])}, //lt - select anyone born before the specified date;
            {"birth_gt", (f, v) => f.AddBirthGt(v[0])}, //gt - select anyone born after the specified date;
            {"birth_year", (f, v) => f.AddBirthYear(v[0])}, //year - select anyone who was born at that year

            {"interests_contains", (f, v) => f.AddInterestsAll(v[0])}, //contains - select everyone who has all the listed interests;
            {"interests_any", (f, v) => f.AddInterestsAny(v[0])}, //any - select everyone who has any of the listed interests;

            {"likes_contains", (f, v) => f.AddAllLikes(v[0])}, //contains - select everyone who likes all listed users (in the value - comma-separated ids);

            {"premium_now", (f, v) => f.HasPremiumNow(v[0])}, //all who have a premium for the current date;;
            {"premium_null", (f, v) => f.HasPremium(v[0])}, //year - select anyone who was born at that year

            {"limit", (f, v) => int.TryParse(v.ToString(), out f.limit)},
            {"query_id", (f, v) => true}
        };
    }
}