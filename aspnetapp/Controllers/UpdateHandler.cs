using System;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    public static class UpdateHandler
    {
        public static unsafe void Process(HttpContext context, uint accountId)
        {
            var contentLength = (int)context.Request.ContentLength;
            var pctx = new ParseContext(Buffer.My, contentLength);
            context.ReadBytes(pctx);

            AccountStub stub = UpdatePool.RentStub();

            stub.id = accountId;

            if (!TryParseAccountUpdate(ref pctx, stub))
            {
                BadRequest(context, stub);
                return;
            }

            if (!stub.email.IsEmpty)
            {
                Account account = Database.GetAccount(accountId);
                Email previous = account.Email;
                Database.Replace(previous, stub.email);
            }

            UpdateWorker.Update(stub);

            context.Response.StatusCode = StatusCodes.Status202Accepted;
            context.Response.ContentLength = 2;
            context.Response.Body.Write(NewHandler.Empty);
        }

        private static void BadRequest(HttpContext context, AccountStub stub)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            UpdatePool.Return(stub);
        }

        private static bool TryParseAccountUpdate(ref ParseContext pctx, AccountStub stub)
        {
            const byte quote = (byte)'"';
            while (pctx.Length > 0)
            {
                var propStart = pctx.IndexOf(quote) + 1; //+1 to remove quote
                if (propStart < 1)
                {
                    break;
                }

                pctx.Move(propStart);
                var length = pctx.IndexOf(quote); //

                ReadOnlySpan<byte> property = pctx.Span.Slice(0, length);

                if (!JsonParser.ParseProperty(property, out JsonValueParser parse))
                {
                    return false;
                }

                pctx.Move(length);
                //Parsing values.
                var valueStart = pctx.IndexOf((byte)':');

                pctx.Move(valueStart + 1);

                if (!parse(ref pctx, stub))
                {
                    return false;
                }
            }

            return true;
        }
    }
}