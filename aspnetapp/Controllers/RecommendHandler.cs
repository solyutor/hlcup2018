using System;
using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    public static class RecommendHandler
    {
        private static readonly HashSet<int> Fields = new HashSet<int>
        {
            Serializer.Fields.Premium,
            Serializer.Fields.Status,
            Serializer.Fields.SName,
            Serializer.Fields.FName,
            Serializer.Fields.Birth
        };

        [ThreadStatic] private static RecommendQuery _query;

        public static void Process(HttpContext context, uint accountId)
        {
            if (!Database.TryGetAccount(accountId, out Account account))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "application/json";

            if (RecommendQueryParser.TryParse(context.Request.Query, ref _query))
            {
                HList<Account> result;
                if (_query.WillYieldZeroResults || account.InterestIndexes.Count == 0 || (result = _query.Execute(account)).Count == 0)
                {
                    context.Response.ContentLength = FastJson.EmptyAccounts.Length;
                    context.Response.Body.Write(FastJson.EmptyAccounts);
                    return;
                }


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