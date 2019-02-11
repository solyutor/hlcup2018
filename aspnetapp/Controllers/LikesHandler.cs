using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aspnetapp.Collections;
using aspnetapp.Serializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace aspnetapp.Controllers
{
    internal class LikesHandler
    {
        public static readonly PathString Path = new PathString("/accounts/likes/");

        public static readonly byte[] tsBytes = Encoding.UTF8.GetBytes("\"ts\":");
        public static readonly byte[] likeeBytes = Encoding.UTF8.GetBytes("\"likee\":");
        public static readonly byte[] likerBytes = Encoding.UTF8.GetBytes("\"liker\":");

        public static unsafe void Process(HttpContext context)
        {
            var contentLength = (int)context.Request.ContentLength;
            var pctx = new ParseContext(Buffer.My, contentLength);
            context.ReadBytes(pctx);

            if (!TryParseLikes(ref pctx, out HList<NewLike> result))
            {
                if (result != null)
                {
                    UpdatePool.Return(result);
                }
                BadRequest(context);
                return;
            }

            if (result != null)
            {
                UpdateWorker.AddLikes(result);
            }

            context.Response.StatusCode = StatusCodes.Status202Accepted;
            context.Response.ContentLength = 2;
            context.Response.Body.Write(NewHandler.Empty);
        }

        private static bool TryParseLikes(ref ParseContext pctx, out HList<NewLike> newLikes)
        {
            var arrayStartIndex = pctx.IndexOf((byte)'[');
            if (pctx[arrayStartIndex+1] == ']')
            {
                newLikes = null;
                return true;
            }

            pctx.Move(arrayStartIndex + 2); //skip start array + start object

            newLikes = UpdatePool.RentLikes();
            NewLike like = default;

            while (pctx.Length > 0)
            {

                for (int i = 0; i < 3; i++)
                {
                    if (pctx.StartsWith(likerBytes))
                    {
                        pctx.Move(likerBytes.Length);
                        if (!JsonParser.TryReadUInt(ref pctx, out like.Liker) || Database.NotExists(like.Liker))
                        {
                            return false;
                        }
                    }
                    else if (pctx.StartsWith(likeeBytes))
                    {
                        pctx.Move(likeeBytes.Length);

                        if (!JsonParser.TryReadUInt(ref pctx, out like.Likee) || Database.NotExists(like.Likee))
                        {
                            return false;
                        }
                    }
                    else if (pctx.StartsWith(tsBytes))
                    {
                        pctx.Move(tsBytes.Length);

                        if (!JsonParser.TryReadInt(ref pctx, out like.Ts))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                pctx.Move(2); // skip ",{" or "]}" in the end
                newLikes.Add(like);
            }

            return true;
        }

        private static void BadRequest(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}