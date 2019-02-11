using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using aspnetapp.Collections;
using aspnetapp.Controllers;
using aspnetapp.Sys;

namespace aspnetapp.Serializer
{
    public static class JsonParser
    {
        private static readonly Dictionary<int, JsonValueParser> properties =
            new Dictionary<int, JsonValueParser>
            {
                {GetInt("id"), ParseIdValue},
                {GetInt("email"), ParseEmailValue},

                {GetInt("fname"), ParseFnameValue},
                {GetInt("sname"), ParseSnameValue},

                {GetInt("phone"), ParsePhoneValue},
                {GetInt("sex"), ParseSexValue},
                {GetInt("birth"), ParseBirthValue},

                {GetInt("country"), ParseCountryValue},
                {GetInt("city"), ParseCityValue},

                {GetInt("joined"), ParseJoinedValue},
                {GetInt("status"), ParseStatusValue},

                {GetInt("likes"), ParseLikesValue},

                {GetInt("interests"), ParseInterestsValue},
                {GetInt("premium"), ParsePremiumValue}
            };

        private static int GetInt(string s)
        {
            if (s.Length >= 4)
            {
                return (byte)s[0] + ((byte)s[1] << 8) + (s[2] << 16) + (s[3] << 24);
            }

            return s[0] + (s[1] << 8);
        }

        public static bool ParseProperty(ReadOnlySpan<byte> property, out JsonValueParser parser)
        {
            var hash = 0;
            if (property.Length >= 4)
            {
                hash = BitConverter.ToInt32(property.Slice(0, 4));
            }
            else if (property.Length >= 2)
            {
                hash = BitConverter.ToInt16(property.Slice(0, 2));
            }
            else
            {
                parser = null;
                return false;
            }

            return properties.TryGetValue(hash, out parser);
        }

        private static bool ParsePremiumValue(ref ParseContext pctx, AccountStub stub)
        {
            if (pctx[0] != '{')
            {
                return false;
            }

            pctx.Move(1);


            if (!ReadPremiumProperty(ref pctx, ref stub.premium)
                || !ReadPremiumProperty(ref pctx, ref stub.premium)
                || !TimeStamp.IsValidPremium(stub.premium.start, stub.premium.finish))
            {
                return false;
            }

            pctx.Move(1);

            stub.Fields |=UpdatedFields.Premium;
            return true;
        }

        private static bool ReadPremiumProperty(ref ParseContext m, ref Premimum p)
        {
            if (TryReadString(ref m, out var name) && TryReadInt(ref m, out var value))
            {
                if (name == nameof(p.start))
                {
                    p.start = value;
                    return true;
                }

                if (name == nameof(p.finish))
                {
                    p.finish = value;
                    return true;
                }
            }

            return false;
        }

        private static bool ParseLikesValue(ref ParseContext pctx, AccountStub stub)
        {
            if (pctx[0] != '[')
            {
                return false;
            }

            pctx.Move(1);

            //TODO: check if it's not a object start
            //TODO: check whether liked account exists
            while (pctx[0] == '{')
            {
                if (!TryParseLike(ref pctx, out LikeStub like))
                {
                    return false;
                }

                stub.likes.Add(like);
                pctx.Move(1); // comma
            }

            pctx.Move(1); // +1 for end array +1 for comma or anything else.
            stub.Fields |=UpdatedFields.Likes;
            return true;
        }

        private static bool TryParseLike(ref ParseContext pctx, out LikeStub like)
        {
            pctx.Move(1);
            like = default;


            return ReadLikeProperty(ref pctx, ref like) && ReadLikeProperty(ref pctx, ref like);
        }

        private static bool ReadLikeProperty(ref ParseContext pctx, ref LikeStub l)
        {
            if (pctx.StartsWith(FastJson.idProp))
            {
                pctx.Move(5); // skip "id":
                return TryReadUInt(ref pctx, out l.id) && !Database.NotExists(l.id);
            }

            if (pctx.StartsWith(FastJson.tsProp))
            {
                pctx.Move(5); // skip "id":
                return TryReadInt(ref pctx, out l.ts);
            }

            return false;
        }

        private static bool ParseInterestsValue(ref ParseContext pctx, AccountStub stub)
        {
            if (pctx[0] != '[')
            {
                return false;
            }

            pctx.Move(1);
            while (pctx[0] == '"') //begin of the string
            {
                if (!TryReadString(ref pctx, out var value))
                {
                    return false;
                }

                stub.interests.Add(value);
            }

            pctx.Move(1);
            stub.Fields |=UpdatedFields.Interests;
            return true;
        }

        private static readonly byte[] statusFree = Encoding.UTF8.GetBytes(@"\u0441\u0432\u043e\u0431\u043e\u0434\u043d\u044b");
        private static readonly byte[] statusNotFree = Encoding.UTF8.GetBytes(@"\u0437\u0430\u043d\u044f\u0442\u044b");
        private static readonly byte[] statusComplex = Encoding.UTF8.GetBytes(@"\u0432\u0441\u0451 \u0441\u043b\u043e\u0436\u043d\u043e");

        private static bool ParseStatusValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Status;
            if (pctx[0] != '"')
            {
                return false;
            }

            pctx.Move(1);
            var endString = pctx.IndexOf((byte)'"');

            if (pctx.StartsWith(statusFree.AsSpan(0,6)))
            {
                stub.sexStatus |= SexStatus.Free;
                pctx.Move(endString+2);
                return true;
            }

            if (pctx.StartsWith(statusNotFree.AsSpan(0,6)))
            {
                stub.sexStatus |= SexStatus.NotFree;
                pctx.Move(endString+2);
                return true;
            }

            if (pctx.StartsWith(statusComplex.AsSpan(0,6)))
            {
                stub.sexStatus |= SexStatus.Complex;
                pctx.Move(endString+2);
                return true;
            }
            return false;
        }

        private static bool ParseJoinedValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Joined;
            return TryReadInt(ref pctx, out stub.joined) && TimeStamp.IsValidJoined(stub.joined);
        }

        private static bool ParseCityValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.City;
            return TryReadString(ref pctx, out stub.city);
        }

        private static bool ParseCountryValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Country;
            return TryReadString(ref pctx, out stub.country);
        }

        private static bool ParseBirthValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Birth;
            return TryReadInt(ref pctx, out stub.birth) && TimeStamp.IsValidBirth(stub.birth);
        }

        private static bool ParseSexValue(ref ParseContext pctx, AccountStub stub)
        {
            if (pctx[0] != '"' || pctx[2] != '"')
            {
                return false;
            }

            var symbol = pctx[1];
            if (symbol == 'm')
            {
                stub.sexStatus |= SexStatus.Male;
            }
            else if (symbol == 'f')
            {
                stub.sexStatus |= SexStatus.Female;
            }
            else
            {
                return false;
            }

            pctx.Move(4); //+4 for "m", or "f", (include comma)
            stub.Fields |=UpdatedFields.Sex;
            return true;
        }

        private static bool ParsePhoneValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Phone;
            return TryReadString(ref pctx, out stub.phone);
        }


        private static bool ParseIdValue(ref ParseContext pctx, AccountStub stub)
            => TryReadUInt(ref pctx, out stub.id)
               && stub.id > 0
               && Database.NotExists(stub.id);

        private static unsafe bool ParseEmailValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.Email;
            pctx.Move(1); // skip quote

            var separatorIndex = pctx.IndexOfAny((byte)'"', (byte)'@');

            if (pctx[separatorIndex] == '"')
            {
                return false;
            }

            var p = pctx.CurrentPointer;

            pctx.Move(separatorIndex + 1);

            var quoteIndex = pctx.IndexOf((byte)'"');

            var domainString = new Utf8String(pctx.CurrentPointer, quoteIndex);

            ushort domainIndex = StringIndexer.Domains.Find(domainString);

            var spanEmail = new SpanEmail(p, (byte)separatorIndex, domainIndex);

            if (!Database.IsUniqueEmail(spanEmail))
            {
                return false;
            }

            stub.email = new Email(UnsafeStringContainer.Clone(new ReadOnlySpan<byte>(p, separatorIndex)), domainIndex);

            pctx.Move(quoteIndex+2);

            return true;

        }

        private static bool ParseFnameValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |= UpdatedFields.FName;
            return TryReadString(ref pctx, out stub.fname);
        }

        private static bool ParseSnameValue(ref ParseContext pctx, AccountStub stub)
        {
            stub.Fields |=UpdatedFields.SName;
            return TryReadString(ref pctx, out stub.sname);
        }

        public static bool TryReadUInt(ref ParseContext pctx, out uint value)
        {
            value = 0;
            var index = 0;

            byte symbol;
            while ((symbol = pctx[index]) != ',' && symbol != '}')
            {
                if (!('0' <= symbol && symbol <= '9'))
                {
                    return false;
                }

                value = (uint)(value * 10 + (symbol - '0'));
                index++;
            }


            pctx.Move(index + 1); // +1 to remove comma
            return true;
        }

        public static bool TryReadInt(ref ParseContext pctx, out int value)
        {
            value = 0;
            var sign = 1;
            var index = 0;
            if (pctx[0] == '-')
            {
                sign = -1;
                index++;
            }

            byte symbol;
            while ((symbol = pctx[index]) != ',' && symbol != '}')
            {
                if (!('0' <= symbol && symbol <= '9'))
                {
                    return false;
                }

                value = value * 10 + (symbol - '0');
                index++;
            }


            pctx.Move(index + 1); // +1 to remove comma
            value *= sign;
            return true;
        }


        private static bool TryReadString(ref ParseContext pctx, out string s)
        {
            if (pctx[0] != '"')
            {
                s = null;
                return false;
            }

            pctx.Move(1);
            //TODO: Move the code to decoder to avoid double walk over span
            var endString = pctx.IndexOf((byte)'"');
            s = UDecoder.GetString(pctx.Span.Slice(0, endString));

            pctx.Move(endString + 2); // +2 for quotes +1 comma or end object or end array

            return true;
        }
    }
}