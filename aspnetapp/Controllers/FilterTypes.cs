using System;

namespace aspnetapp.Controllers
{
    [Flags]
    public enum FilterTypes : ulong
    {
        empty = 0,

        //For /filter
        sex_eq = 1UL << 1,

        status_eq = 1UL << 2,
        status_neq = 1UL << 3,

        email_domain = 1UL << 4,
        email_lt = 1UL << 5,
        email_gt = 1UL << 6,

        fname_eq = 1UL << 7,
        fname_any = 1UL << 8,
        fname_null = 1UL << 9,
        fname_not_null = 1UL << 10,

        sname_eq = 1UL << 11,
        sname_starts = 1UL << 12,
        sname_null = 1UL << 13,
        sname_not_null = 1UL << 14,

        phone_code = 1UL << 15,
        phone_null = 1UL << 16,
        phone_not_null = 1UL << 17,

        country_eq = 1UL << 18,
        country_null = 1UL << 19,
        country_not_null = 1UL << 20,
        countries_all_types = country_eq | country_null | country_not_null,

        city_eq = 1UL << 21,
        city_any = 1UL << 22,
        city_null = 1UL << 23,
        city_not_null = 1UL << 24,
        cities_all_types = city_eq | city_null | city_not_null,


        birth_lt = 1UL << 25,
        birth_gt = 1UL << 26,
        birth_year = 1UL << 27,


        interests_any = 1UL << 28,

        likes_all = 1UL << 29,

        premium_now = 1UL << 30,
        premium_null = 1UL << 31,
        premium_not_null = 1UL << 32,

        //For /group
        joined = 1UL << 33,
        likes_one = 1UL << 34,

        interests_all = 1UL << 35,


        sex_status_all_types = sex_eq | status_eq | status_neq
    }
}