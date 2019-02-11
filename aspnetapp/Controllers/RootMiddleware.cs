using System;
using System.Threading.Tasks;
using aspnetapp.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    public class RootMiddleware
    {
        private readonly HOrdinalDict<Action<HttpContext>> handlers = new HOrdinalDict<Action<HttpContext>>(4)
        {
            {FilterHandler.Path.Value, FilterHandler.Process},
            {GroupHandler.Path.Value, GroupHandler.Process},
            {NewHandler.Path.Value, NewHandler.Process},
            {LikesHandler.Path.Value, LikesHandler.Process}
        };

        public void InvokeAsync(HttpContext context)
        {
            try
            {
                var path = context.Request.Path.Value;

                if (context.Request.Headers.TryGetValue("Connection", out StringValues connection))
                {
                    context.Response.Headers["Connection"] = connection[0];
                }

                if (handlers.TryGetValue(path, out Action<HttpContext> handler))
                {
                    handler(context);
                    return;
                }

                const string accounts = "/accounts/";
                if (path.StartsWith(accounts, StringComparison.Ordinal))
                {
                    var startIndex = accounts.Length;
                    var endIndex = path.IndexOf('/', startIndex);
                    ReadOnlySpan<char> idSpan = path.AsSpan(startIndex, endIndex - startIndex);

                    if (!uint.TryParse(idSpan, out var accountId))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    if (Database.NotExists(accountId))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }

                    if (path.EndsWith("/recommend/", StringComparison.Ordinal))
                    {
                        RecommendHandler.Process(context, accountId);
                        return;
                    }

                    if (path.EndsWith("/suggest/", StringComparison.Ordinal))
                    {
                        SuggestHandler.Process(context, accountId);
                        return;
                    }

                    UpdateHandler.Process(context, accountId);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status404NotFound;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}