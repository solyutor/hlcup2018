using aspnetapp.Controllers;

namespace aspnetapp
{
    public static class SexStatusExtensions
    {
        private static readonly string[] strings;

        static SexStatusExtensions()
        {
            strings = new string[17];
            strings[(int)SexStatus.Male] = "m";
            strings[(int)SexStatus.Female] = "f";
            strings[(int)SexStatus.Free] = Statuses.Free;
            strings[(int)SexStatus.NotFree] = Statuses.NotFree;
            strings[(int)SexStatus.Complex] = Statuses.Complicated;
        }

        public static SexStatus Set(this SexStatus self, SexStatus value) => self | value;


        public static bool IsMale(this SexStatus self) => self.HasFlags(SexStatus.Male);
        public static bool IsFemale(this SexStatus self) => self.HasFlags(SexStatus.Female);

        public static bool IsFree(this SexStatus self) => self.HasFlags(SexStatus.Free);
        public static bool IsNotFree(this SexStatus self) => self.HasFlags(SexStatus.NotFree);
        public static bool IsComplex(this SexStatus self) => self.HasFlags(SexStatus.NotFree);

        public static bool HasFlags(this SexStatus self, SexStatus value) => (self & value) == value;

        public static string GetSex(this Account self) => self.SexStatus.IsMale() ? "m" : "f";


        public static string GetStatus(this Account self)
        {
            if (self.SexStatus.IsFree())
            {
                return Statuses.Free;
            }

            if (self.SexStatus.IsNotFree())
            {
                return Statuses.NotFree;
            }

            return Statuses.Complicated;
        }


        /*/// <summary>
        /// Must work correctly for both status_eq and status_neq
        /// </summary>
        public static bool Matches(this SexStatus self, SexStatus mask)
        {
            //Put predefined values into array and look up by index.
            SexStatus sexMask = mask & SexStatus.AllSex;
            if ((sexMask & self) != sexMask)
            {
                return false;
            }

            SexStatus maskStatus = SexStatus.AllStatus & mask;
            SexStatus selfStatus = SexStatus.AllStatus & self;

            var equal = (maskStatus & self) == maskStatus;
            var notEqual = (selfStatus | maskStatus) == maskStatus;
            return equal || notEqual;
        }*/

        public static string ToNullableSexString(this SexStatus sexStatus)
        {
            var index = (int)(sexStatus & SexStatus.AllSex);
            return strings[index];
        }

        public static string ToNullableStatusString(this SexStatus sexStatus)
        {
            var index = (int)(sexStatus & SexStatus.AllStatus);
            return strings[index];
        }
    }
}