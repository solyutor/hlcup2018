using System;
using aspnetapp.Collections;

namespace aspnetapp.Extensions
{
    public static class StringExtensions
    {
        public static string SafeIntern(this string self) => self == null ? null : string.Intern(self);

        public static bool OrdinalEqualsTo(this string self, string other) => HOrdinalComparer.Instance.Equals(self, other);

        public static bool AssignIfNotEmpty(this string self, ref string dest)
        {
            if (string.IsNullOrWhiteSpace(self))
            {
                return false;
            }

            dest = self;
            return true;
        }
    }
}