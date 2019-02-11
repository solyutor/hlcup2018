using System;
using System.Collections.Concurrent;
using System.Data.SqlTypes;

namespace aspnetapp.Controllers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class FilterAttribute : Attribute
    {
        public FilterTypes Type { get; }

        public FilterAttribute(FilterTypes type)
        {
            Type = type;
        }
    }
}