namespace aspnetapp.Collections
{
    public class HInt32Dict<TValue> : HDict<int, TValue, Int32Comparer>
    {
        public HInt32Dict(int capacity) : base(capacity, Int32Comparer.Instance)
        {
        }
    }

    public class HUint32Dict<TValue> : HDict<uint, TValue, Uint32Comparer>
    {
        public HUint32Dict(int capacity) : base(capacity, Uint32Comparer.Instance)
        {
        }
    }
}