using System;
using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace aspnetapp.Controllers
{
    public static class SuggestHandler
    {
        private static readonly HashSet<int> Fields = new HashSet<int>
        {
            Serializer.Fields.Status,
            Serializer.Fields.SName,
            Serializer.Fields.FName
        };

        [ThreadStatic] private static SuggestQuery _query;

        public static void Process(HttpContext context, uint accountId)
        {
            if (!Database.TryGetAccount(accountId, out Account account))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            if (SuggestQueryParser.TryParse((QueryCollection)context.Request.Query, ref _query))
            {
                context.Response.ContentType = "application/json";
                if (_query.WillYieldZeroResults)
                {
                    context.Response.ContentLength = FastJson.EmptyAccounts.Length;
                    context.Response.Body.Write(FastJson.EmptyAccounts);
                    return;
                }

                HList<Account> result = _query.Execute(account);

                ParseContext stream = Buffer.Context;
                FastJson.WriteFilterResponse(result, Fields, ref stream, (a, f) => Filter(a, f));

                context.Response.ContentLength = stream.WrittenLength;
                context.Response.Body.Write(stream.WrittenSpan);

                return;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        private static bool Filter(Account account, int field)
        {
            if (field == Serializer.Fields.Premium)
            {
                return account.premium.start != 0;
            }

            if (field == Serializer.Fields.SName)
            {
                return account.snameIndex != 0;
            }

            if (field == Serializer.Fields.FName)
            {
                return account.fnameIndex != 0;
            }

            return true;
        }
    }
}