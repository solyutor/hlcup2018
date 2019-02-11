using System;

namespace aspnetapp
{
    [Flags]
    public enum SexStatus : byte
    {
        None = 0,
        Male = 1,
        Female = 2,
        NotFree = 4,
        Complex = 8,
        Free = 16,

        AllSex = Male | Female,

        AllStatus = Free | NotFree | Complex
    }
}