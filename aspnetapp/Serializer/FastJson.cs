using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Sys;

namespace aspnetapp.Serializer
{
    public static class FastJson
    {
        private static readonly Encoding Encoding = Encoding.UTF8;

        //{"accounts": [...]}
        public static readonly byte[] EmptyAccounts = Encoding.GetBytes("{\"accounts\":[]}");
        public static readonly byte[] EmptyGroups = Encoding.GetBytes("{\"groups\":[]}");
        private static readonly byte[] AccountsPrefix = Encoding.GetBytes("{\"accounts\":[");
        private static readonly byte[] GroupsPrefix = Encoding.GetBytes("{\"groups\":[");
        private static readonly byte[] Suffix = Encoding.GetBytes("]}");

        private const byte StartObject = (byte)'{';
        private const byte EndObject = (byte)'}';
        private const byte Separator = (byte)',';
        private const byte Quote = (byte)'"';


        public static readonly byte[] idProp = Encoding.GetBytes("\"id\":");
        private static readonly byte[] emailProp = Encoding.GetBytes("\"email\":");

        private static readonly byte[] fnameProp = Encoding.GetBytes("\"fname\":");
        private static readonly byte[] snameProp = Encoding.GetBytes("\"sname\":");
        private static readonly byte[] phoneProp = Encoding.GetBytes("\"phone\":");

        public static readonly byte[] fSex = Encoding.GetBytes("\"sex\":\"f\"");
        public static readonly byte[] mSex = Encoding.GetBytes("\"sex\":\"m\"");
        private static readonly byte[] birthProp = Encoding.GetBytes("\"birth\":");

        private static readonly byte[] countryProp = Encoding.GetBytes("\"country\":");
        private static readonly byte[] cityProp = Encoding.GetBytes("\"city\":");

        private static readonly byte[] joinedProp = Encoding.GetBytes("\"joined\":");

        public static readonly byte[] freeStatus = Encoding.GetBytes($"\"status\":\"{Statuses.Free}\"");
        public static readonly byte[] notfreeStatus = Encoding.GetBytes($"\"status\":\"{Statuses.NotFree}\"");
        public static readonly byte[] complicatedStatus = Encoding.GetBytes($"\"status\":\"{Statuses.Complicated}\"");

        private static readonly byte[] interestsProp = Encoding.GetBytes("\"interests\":");
        private static readonly byte[] premiumProp = Encoding.GetBytes("\"premium\":");
        private static readonly byte[] premiumStartProp = Encoding.GetBytes("\"start\":");
        private static readonly byte[] premiumFinishProp = Encoding.GetBytes("\"finish\":");

        private static readonly byte[] likesProp = Encoding.GetBytes("\"likes\":");
        public static readonly byte[] tsProp = Encoding.GetBytes("\"ts\":");

        private static readonly byte[] countProp = Encoding.GetBytes("\"count\":");

        private static readonly byte[] nullValue = Encoding.GetBytes("null");

        //TODO: Compare it's performance with simple switch.
        private static readonly Serializer[]  Serializers = new Serializer[14];

        private delegate void Serializer(Account account, ref ParseContext stream);

        static FastJson()
        {
            Serializers[Fields.FName] = delegate(Account a, ref ParseContext s)
            {
                WritePropertyAndValue(fnameProp, StringIndexer.FirstNames.GetBytes(a.fnameIndex), ref s);
            };
            Serializers[Fields.SName] = delegate(Account a, ref ParseContext s)
            {
                WritePropertyAndValue(snameProp, StringIndexer.LastNames.GetBytes(a.snameIndex), ref s);
            };
            
            Serializers[Fields.Phone] = delegate(Account a, ref ParseContext s)
            {
                WritePropertyAndValue(phoneProp, a.phone, ref s);
            };

            Serializers[Fields.Sex] = delegate(Account a, ref ParseContext s)
            {
                s.Write(a.SexStatus.IsMale() ? mSex : fSex);
            };
            Serializers[Fields.Birth] = delegate(Account a, ref ParseContext s)
            {
                SerializeInt(birthProp, a.birth, ref s);
            };
            Serializers[Fields.Country] = delegate(Account a, ref ParseContext s)
            {
                WritePropertyAndValue(countryProp, StringIndexer.Countries.GetBytes(a.countryIndex), ref s);
            };
            Serializers[Fields.City] = delegate(Account a, ref ParseContext s)
            {
                WritePropertyAndValue(cityProp, StringIndexer.Cities.GetBytes(a.cityIndex), ref s);
            };

            Serializers[Fields.Joined] = delegate(Account a, ref ParseContext s)
            {
                SerializeInt(joinedProp, a.joined, ref s);
            };
            Serializers[Fields.Status] = delegate(Account a, ref ParseContext s)
            {
                if (a.SexStatus.IsFree())
                {
                    s.Write(freeStatus);
                }
                else if (a.SexStatus.IsNotFree())
                {
                    s.Write(notfreeStatus);
                }
                else
                {
                    s.Write(complicatedStatus);
                }
            };
            Serializers[Fields.Interests] = delegate(Account a, ref ParseContext s)
            {
                SerializeInterests(a, ref s);
            };
            Serializers[Fields.Premium] = delegate(Account a, ref ParseContext s)
            {
                SerializePremium(a, ref s);
            };
        }

        public static void WriteFilterResponse(HList<Account> accounts, HashSet<int> fields, ref ParseContext stream, Func<Account, int, bool> fieldFilter = null)
        {
            //TODO: Move the check to the handler.
            if (accounts.Count == 0)
            {
                stream.Write(EmptyAccounts);
                return;
            }

            stream.Write(AccountsPrefix);

            var writeSeparator = false;
            foreach (Account account in accounts)
            {
                //Get rid of this if and use enumerator by hand
                if (writeSeparator)
                {
                    stream.WriteByte(Separator);
                }

                stream.WriteByte(StartObject);

                SerializeIdEmail(account, ref stream);
                SerializeFields(account, fields, ref stream, fieldFilter);


                stream.WriteByte(EndObject);

                writeSeparator = true;
            }


            stream.Write(Suffix);
        }


        private static void SerializeIdEmail(Account account, ref ParseContext stream)
        {
            stream.Write(idProp);
            WriteInt(ref stream, (int) account.id);

            stream.WriteByte(Separator);

            stream.Write(emailProp);

            stream.WriteByte(Quote);

            stream.Write(account.Email._email.Span);
            stream.WriteByte((byte) '@');
            stream.Write(StringIndexer.Domains.GetBytes(account.Email._domain).Span);
            stream.WriteByte(Quote);
        }

        private static void SerializeFields(Account account, HashSet<int> fields, ref ParseContext stream, Func<Account, int, bool> fieldFilter)
        {
            foreach (var field in fields)
            {
                if (fieldFilter != null && !fieldFilter(account, field))
                {
                    continue;
                }

                //id and email already serialized.
                stream.WriteByte(Separator);

                Serializers[field](account, ref stream);
            }
        }

        private static void SerializeInterests(Account account, ref ParseContext stream)
        {
            stream.Write(interestsProp);
            stream.WriteByte((byte)'[');
            var writeSeparator = false;
            foreach (var index in account.InterestIndexes)
            {
                var interest = StringIndexer.Interests.GetBytes(index);
                if (writeSeparator)
                {
                    stream.WriteByte(Separator);
                }

                stream.Write(interest.Span);
                writeSeparator = true;
            }

            stream.WriteByte((byte)']');
        }

        private static void SerializePremium(Account account, ref ParseContext stream)
        {
            stream.Write(premiumProp);
            stream.WriteByte(StartObject);

            SerializeInt(premiumStartProp, account.premium.start, ref stream);

            stream.WriteByte(Separator);

            SerializeInt(premiumFinishProp, account.premium.finish, ref stream);

            stream.WriteByte(EndObject);
        }


        public static void WriteGroupResponse(HList<KeyValuePair<GroupKey, int>> result, GroupQuery query, ref ParseContext stream)
        {
            if (result.Count == 0)
            {
                stream.Write(EmptyGroups);
                return;
            }

            stream.Write(GroupsPrefix);
            var shouldWriteSeparator = false;
            foreach (KeyValuePair<GroupKey, int> kvp in result)
            {
                if (shouldWriteSeparator)
                {
                    stream.WriteByte(Separator);
                }

                stream.WriteByte(StartObject);

                SerializeGroupKey(ref stream, query._first, kvp.Key);
                SerializeGroupKey(ref stream, query._second, kvp.Key);

                stream.Write(countProp);
                WriteInt(ref stream, kvp.Value);

                stream.WriteByte(EndObject);
                shouldWriteSeparator = true;
            }

            stream.Write(Suffix);
        }

        private static void SerializeGroupKey(ref ParseContext stream, GroupKeys groupKey, in GroupKey group)
        {
            switch (groupKey)
            {
                case GroupKeys.Sex:
                    stream.Write((@group.SexStatus & SexStatus.AllSex) == SexStatus.Male ? mSex : fSex);
                    stream.WriteByte(Separator);
                    return;

                case GroupKeys.Status:
                    switch (group.SexStatus & SexStatus.AllStatus)
                    {
                        case SexStatus.Free:
                            stream.Write(freeStatus);
                            stream.WriteByte(Separator);
                            return;
                        case SexStatus.NotFree:
                            stream.Write(notfreeStatus);
                            stream.WriteByte(Separator);
                            return;
                        default:
                            stream.Write(complicatedStatus);
                            stream.WriteByte(Separator);
                            return;

                    }

                case GroupKeys.Interests:
                {
                    var value = group.InterestIndex;
                    if (value == 0)
                    {
                        return;
                    }
                    stream.Write(interestsProp);
                    stream.Write(StringIndexer.Interests.GetBytes(value).Span);
                    stream.WriteByte(Separator);
                    return;
                }

                case GroupKeys.City:
                {
                    var value = group.CityIndex;
                    if (value == 0)
                    {
                        return;
                    }
                    stream.Write(cityProp);
                    stream.Write(StringIndexer.Cities.GetBytes(value).Span);
                    stream.WriteByte(Separator);
                    return;
                }

                default:
                {
                    var value = group.CountryIndex;
                    if (value == 0)
                    {
                        return;
                    }
                    stream.Write(countryProp);
                    stream.Write(StringIndexer.Countries.GetBytes(value).Span);
                    stream.WriteByte(Separator);
                    return;
                }
            }
        }

        private static void SerializeString(byte[] propName, string value, ref ParseContext stream)
        {
            stream.Write(propName);
            WriteQuotedString(ref stream, value);
        }

        private static void WritePropertyAndValue(byte[] propName, in Utf8String value, ref ParseContext stream)
        {
            stream.Write(propName);
            stream.Write(value.Span);
        }

/*        private static void WritePropertyAndValue(byte[] propName, byte[] value, Stream stream)
        {
            stream.Write(propName);
            stream.Write(value);
        }*/


        private static void WriteQuotedString(ref ParseContext stream, string value)
        {
            if (value == null)
            {
                stream.Write(nullValue);
                return;
            }


            stream.WriteByte(Quote);

            WriteString(ref stream, value);
            stream.WriteByte(Quote);
        }

        private static void WriteString(ref ParseContext stream, string value)
        {
            Span<byte> buffer = stream.Span;
            var written = Encoding.GetBytes(value, buffer);
            stream.Move(written);
        }

        private static void SerializeInt(byte[] propName, int value, ref ParseContext stream)
        {
            stream.Write(propName);
            WriteInt(ref stream, value);
        }

        private static void WriteInt(ref ParseContext stream, int value)
        {
            const int offset = '0';

            int length;
            if (value < 10)
            {
                length = 1;
            }
            else if (value < 100)
            {
                length = 2;
            }
            else if (value < 1_000)
            {
                length = 3;
            }
            else if (value < 10_000)
            {
                length = 4;
            }
            else if (value < 100_000)
            {
                length = 5;
            }
            else if (value < 1_000_000)
            {
                length = 6;
            }
            else if (value < 10_000_000)
            {
                length = 7;
            }
            else if (value < 100_000_000)
            {
                length = 8;
            }
            else if (value < 1_000_000_000)
            {
                length = 9;
            }
            else
            {
                length = 10;
            }

            Span<byte> buffer = stream.Span;

            for (var i = length - 1; i >= 0; --i)
            {
                buffer[i] = (byte)(offset + value % 10);
                value /= 10;
            }
            stream.Move(length);
        }
    }
}