using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiCake.Authentication.MiniProgram.WeChat
{
    public class DefaultSessionInfoStore : IWeChatSessionInfoStore
    {
        private readonly MemoryCache _memCache;
        private const string keyPrefix = "mc-auth-wechat-";

        public DefaultSessionInfoStore(ILoggerFactory loggerFactory)
        {
            // create default memory cache.
            _memCache = new MemoryCache(Options.Create(new MemoryDistributedCacheOptions()), loggerFactory);
        }

        public Task<string> Store(WeChatSessionInfo sessionInfo, WeChatMiniProgramOptions currentOption, CancellationToken cancellationToken = default)
        {
            var key = keyPrefix + Guid.NewGuid().ToString();

            var memoryCacheEntryOptions = new MemoryCacheEntryOptions();
            memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow = currentOption.CacheExpiration;

            _memCache.Set(key, sessionInfo, memoryCacheEntryOptions);

            return Task.FromResult(key);
        }

        public Task Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            _memCache.Remove(key);

            return Task.CompletedTask;
        }

        public Task<WeChatSessionInfo?> GetSession(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = _memCache.Get<WeChatSessionInfo>(key);

            return Task.FromResult(value);
        }

        public Task<WeChatSessionInfo?> GetAndRemoveSession(string key, CancellationToken cancellationToken = default)
        {
            var data = GetSession(key, cancellationToken);

            _memCache.Remove(key);

            return data;
        }
    }
}
