﻿using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using FreeRedis;


namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingInRedis(this IServiceCollection service, CacheConfiguration config, RedisClient client)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (client == null)
                throw new ArgumentNullException(nameof(client));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            });
            service.AddSingleton(client);
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInRedis(this IServiceCollection service, RedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            return service.AddDapperCachingInRedis(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }, new RedisClient(config.ConnectionString));
        }

        public static IServiceCollection AddDapperCachingInRedis<TCacheKeyBuilder>(this IServiceCollection service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            return service.AddDapperCachingInRedis(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }, new RedisClient(config.ConnectionString));
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            });
            service.AddSingleton(config.PartitionPolicy != null
                ? new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), config.PartitionPolicy)
                : new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), null));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this IServiceCollection service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            });
            service.AddSingleton(config.PartitionPolicy != null
                ? new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), config.PartitionPolicy)
                : new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), null));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
