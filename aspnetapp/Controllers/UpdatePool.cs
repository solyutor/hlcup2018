using System.Linq;
using aspnetapp.Collections;
using Microsoft.Extensions.ObjectPool;

namespace aspnetapp.Controllers
{
    public static class UpdatePool
    {
        private const int MaximumRetained = 1000;

        private class LikesPoolPolicy : IPooledObjectPolicy<HList<NewLike>>
        {
            public HList<NewLike> Create() => new HList<NewLike>(100);

            public bool Return(HList<NewLike> obj)
            {
                obj.ClearFast();
                return true;
            }
        }

        private class AccountStubPolicy : IPooledObjectPolicy<AccountStub>
        {
            public AccountStub Create() => new AccountStub();

            public bool Return(AccountStub obj)
            {
                obj.Clear();
                return true;
            }
        }

        private static readonly DefaultObjectPool<HList<NewLike>> likesPool;
        private static DefaultObjectPool<AccountStub> _accountStubPool;

        static UpdatePool()
        {
            var likesPolicy = new LikesPoolPolicy();
            likesPool = new DefaultObjectPool<HList<NewLike>>(likesPolicy, MaximumRetained);
            var accountPolicy = new AccountStubPolicy();
            _accountStubPool = new DefaultObjectPool<AccountStub>(accountPolicy, MaximumRetained);
        }

        public static void Initialize()
        {
            //Initialize pool eagerly
            var likes = Enumerable.Range(1, MaximumRetained).Select(x => likesPool.Get()).ToArray();
            foreach (var list in likes)
            {
                likesPool.Return(list);
            }

            var stubs = Enumerable.Range(1, MaximumRetained).Select(x => _accountStubPool.Get()).ToArray();
            foreach (var stub in stubs)
            {
                _accountStubPool.Return(stub);
            }
        }

        public static HList<NewLike> RentLikes() => likesPool.Get();

        public static void Return(HList<NewLike> list) => likesPool.Return(list);

        public static AccountStub RentStub() => _accountStubPool.Get();

        public static void Return(AccountStub stub) => _accountStubPool.Return(stub);
    }
}