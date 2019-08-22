﻿using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        private static bool Exist<TServiceType, TImplementationType>(this IServiceCollection service)
        {
            return service.Any(p =>
                p.ServiceType == typeof(TServiceType) && p.ImplementationType == typeof(TImplementationType));
        }

        public static IServiceCollection AddDapperCachingForRedis(this IServiceCollection service, RedisConfiguration config)
        {
            if (!service.Exist<ICacheKeyBuilder, DefaultCacheKeyBuilder>())
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                service.AddSingleton(new CacheConfiguration
                {
                    Enable = config.Enable,
                    Expire = config.Expire
                });
                RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            }
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingForPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            if (!service.Exist<ICacheKeyBuilder, DefaultCacheKeyBuilder>())
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                service.AddSingleton(config);
                RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections), config.Connections));
            }
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
