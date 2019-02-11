using System;
using System.Runtime.InteropServices;
using System.Text;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    public static unsafe class NewHandler
    {
        public static readonly PathString Path = new PathString("/accounts/new/");

        public static readonly byte[] Empty = Encoding.UTF8.GetBytes("{}");

        public static void Process(HttpContext context)
        {
            var contentLength = (int)context.Request.ContentLength;
            var pctx = new ParseContext(Buffer.My, contentLength);
            context.ReadBytes(pctx);

            var stub = UpdatePool.RentStub();

            var statusCode = TryParseAccount(ref pctx, stub);
            context.Response.StatusCode = statusCode;
            if (statusCode == 201)
            {
                context.Response.ContentLength = Empty.Length;
                context.Response.Body.Write(Empty);
                UpdateWorker.AddNew(Database.InsertFromPost(stub));
            }
            else
            {
                UpdatePool.Return(stub);
            }
        }

        public static int TryParseAccount(ref ParseContext ctx, AccountStub stub)
        {
            const byte quote = (byte)'"';
            while (ctx.Length > 0)
            {
                var propStart = ctx.IndexOf(quote) + 1; //+1 to remove quote
                if (propStart < 1)
                {
                    break;
                }

                ctx.Move(propStart);
                var length = ctx.IndexOf(quote); //

                ReadOnlySpan<byte> property = ctx.Span.Slice(0, length);

                if (!JsonParser.ParseProperty(property, out JsonValueParser parse))
                {
                    return StatusCodes.Status400BadRequest;
                }

                ctx.Move(length);
                var valueStart = ctx.IndexOf((byte)':');

                ctx.Move(valueStart + 1);

                if (!parse(ref ctx, stub))
                {
                    return StatusCodes.Status400BadRequest;
                }
            }

            if (stub.id == 0 || stub.email.IsEmpty)
            {
                return StatusCodes.Status400BadRequest;
            }

            return StatusCodes.Status201Created;
        }
    }
}