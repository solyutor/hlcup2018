using aspnetapp.Controllers;

namespace aspnetapp.Serializer
{
    public delegate bool JsonValueParser(ref ParseContext pctx, AccountStub stub);
}