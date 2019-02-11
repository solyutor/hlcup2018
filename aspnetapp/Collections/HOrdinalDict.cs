namespace aspnetapp.Collections
{
    public class HOrdinalDict<TValue> : HDict<string, TValue, HOrdinalComparer>
    {
        public HOrdinalDict(int capacity) : base(capacity, HOrdinalComparer.Instance)
        {
        }
    }
}