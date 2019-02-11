using System;

namespace aspnetapp.Serializer
{
    [Flags]
    public enum UpdatedFields : ushort
    {
        None = 0, // For indexing new accounts.
        Email = 1 << 0,

        FName = 1 << 1,
        SName = 1 << 2,
        Phone = 1 << 3,
        Sex = 1 << 4,
        Birth = 1 << 5,

        Country = 1 << 6,
        City = 1 << 7,

        Joined = 1 << 8,
        Status = 1 << 9,

        Interests = 1 << 10,
        Premium = 1 << 11,
        Likes = 1 << 12,

        GroupIndexFlags = Sex | Status | City | Country | Birth | Joined | Interests | Phone,
        RecommendIndexFlags = Sex | Status | City | Country | Premium | Interests,
        LikesIndexFlags = Sex | Status | Likes

    }

    public static class UpdatedFieldsExtensions
    {
        public static bool ContainsAny(this UpdatedFields self, UpdatedFields flags)
        {
            return (self & flags) != UpdatedFields.None;
        }
    }
}