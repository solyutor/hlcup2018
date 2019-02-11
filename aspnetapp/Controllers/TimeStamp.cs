using System;
using System.Collections.Generic;

namespace aspnetapp.Controllers
{
    public static class TimeStamp
    {
        /*
Max Interests: 9
Max Email: 24
Max Phone: 14
Min like ts: 1452384000
Max like ts: 1541807999

        */

        public const int MinBirth = -631152000;
        public const ushort MinBirthYear = 1950;

        public const int MaxBirth = 1104537600;
        public const ushort MaxBirthYear = 2005;
        public static bool IsValidBirth(int value) => MinBirth <= value && value <= MaxBirth;

        public const int MaxAgeDiff = MaxBirth - MinBirth;

        public const int MinJoined = 1293840000;
        public const int MinJoinedYear = 2011;
        public const int MaxJoined = 1514764800;
        public static bool IsValidJoined(int value) => MinJoined <= value && value <= MaxJoined;

        public const int MinPremium = 1293840000;
        public static bool IsValidPremium(int start, int finish) => MinPremium <= start && start < finish;


        public static ushort GetBirthYear(this Account self) => self.birth.GetYear();
        public static ushort GetBirthOffset(this Account self) => (ushort) (self.birth.GetYear() - MinBirthYear);
        public static ushort GetBirthOffset(this ushort self) => (ushort) (self - MinBirthYear);
        public static ushort GetJoinedYear(this Account self) => self.joined.GetYear();
        public static ushort GetYear(this int timestamp) => (ushort) DateTime.UnixEpoch.AddSeconds(timestamp).Year;
    }
}