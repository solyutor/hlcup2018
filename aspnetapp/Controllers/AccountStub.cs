using System.Collections.Generic;
using System.Linq;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using aspnetapp.Sys;
using Newtonsoft.Json;

namespace aspnetapp.Controllers
{
    public class AccountStub
    {
        public AccountStub()
        {
            likes = new List<LikeStub>(50);
            interests = new HList<string>(10);
        }
        public uint id;
        public string sname;
        public string fname;

        public string sex;
        public string status;

        public string phone;
        [JsonConverter(typeof(EmailConverter))]
        public Email email;


        public SexStatus sexStatus;
        public string country;
        public Premimum premium;
        public readonly List<LikeStub> likes;

        public readonly HList<string> interests;
        public int joined;
        public string city;
        public int birth;

        public UpdatedFields Fields;

        public void FillAccount(Account account)
        {
            account.id = id;
            account.Email = email;

            account.fname = fname;
            account.sname = sname;

            account.phone = UnsafeStringContainer.GetString(phone, true);

            account.country = country;
            account.premium = premium;
            account.interests = interests;
            account.SexStatus = sexStatus == SexStatus.None ? GetSexStatus() : sexStatus;


            account.birth = birth;
            account.city = city;
            account.likes?.Clear();
            account.ReplaceLikes(likes, false);
            account.joined = joined;
        }

        private SexStatus GetSexStatus()
        {
            SexStatus result = Statuses.GetStatus(status);

            return result | (sex == "m" ? SexStatus.Male : SexStatus.Female);

        }

        public void Clear()
        {
            id = 0;
            sname = null;
            fname = null;

            sex = null;
            status = null;

            phone = null;
            email = default;


            sexStatus = SexStatus.None;
            country = null;
            premium = default;
            likes.Clear();

            interests.Clear();
            joined = 0;
            city =  null;
            birth = 0;

            Fields = UpdatedFields.None;
        }
    }
}