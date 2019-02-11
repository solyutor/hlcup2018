using Microsoft.Extensions.Primitives;

namespace aspnetapp.Controllers
{
    public delegate bool Parser<TQuery>(TQuery query, StringValues values);
}