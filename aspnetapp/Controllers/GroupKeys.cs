using System;

namespace aspnetapp.Controllers
{
    [Flags]
    public enum GroupKeys : byte
    {
        None = 0,
        Sex = 1 << 0,
        Status = 1 << 1,
        Interests = 1 << 2,
        City = 1 << 3,
        Country = 1 << 4,

        Ascending = 1 << 5,
        Descending = 1 << 6
    }
}