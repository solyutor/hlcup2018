using System;
using System.Collections.Generic;
using System.Linq;

namespace aspnetapp.Controllers
{
    public static class FilterTypeExtensions
    {
        private static readonly ulong Max = Enum.GetValues(typeof(FilterTypes)).Cast<ulong>().Max();

        public static IEnumerable<FilterTypes> EnumerateSetFlags(this FilterTypes self)
        {

            for (ulong flag = 1; flag <= Max; flag = flag << 1)
            {
                if ((flag & (ulong)self) == flag)
                {
                    yield return (FilterTypes)flag;
                }
            }
        }

        public static bool ContainsAll(this FilterTypes self, FilterTypes value)
        {
            return (self & value) == value;
        }

        public static bool ContainsAny(this FilterTypes self, FilterTypes value)
        {
            return (self & value) != FilterTypes.empty;
        }

        public static FilterTypes ResetFlags(this FilterTypes self, FilterTypes value)
        {
            return (self & (~value));
        }
    }
}