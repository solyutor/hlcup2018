using System;
using System.Collections.Generic;
using aspnetapp.Extensions;

namespace aspnetapp.Controllers
{
    public static class Statuses
    {
        public static readonly HashSet<string> All = new HashSet<string>(StringComparer.Ordinal) {Free, NotFree, Complicated};
        public const string Free = "свободны";
        public const string NotFree = "заняты";
        public const string Complicated = "всё сложно";


        public static SexStatus GetStatus(string value)
        {
            if (value.OrdinalEqualsTo(Free))
            {
                return SexStatus.Free;
            }

            if (value.OrdinalEqualsTo(NotFree))
            {
                return SexStatus.NotFree;
            }

            if (value.OrdinalEqualsTo(Complicated))
            {
                return SexStatus.Complex;
            }

            return SexStatus.None;
        }
    }
}