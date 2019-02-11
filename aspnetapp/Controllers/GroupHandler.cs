using System;
using System.Collections.Generic;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace aspnetapp.Controllers
{
    public class GroupHandler
    {
        public static readonly PathString Path = new PathString("/accounts/group/");

        [ThreadStatic] private static GroupQuery _query;

        public static void Process(HttpContext context)
        {
            var queryCollection = (QueryCollection)context.Request.Query;
            if (GroupQueryParser.TryParse(queryCollection, ref _query))
            {
                context.Response.ContentType = "application/json";
                if (_query.WillYieldZeroResults)
                {
                    context.Response.ContentLength = FastJson.EmptyGroups.Length;
                    context.Response.Body.Write(FastJson.EmptyGroups);
                    return;
                }

                HList<KeyValuePair<GroupKey, int>> result = _query.ExecuteGroup();


                ParseContext stream = Buffer.Context;
                FastJson.WriteGroupResponse(result, _query, ref stream);

                context.Response.ContentLength = stream.WrittenLength;
                context.Response.Body.Write(stream.WrittenSpan);


                return;
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}