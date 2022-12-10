﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Security.Cryptography;

namespace Dapper.Extensions.Caching
{
    public class DefaultCacheKeyBuilder : ICacheKeyBuilder, IDisposable
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> ParamProperties = new();

        private static readonly char[] Digitals = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        private CacheConfiguration CacheConfiguration { get; }

        [ThreadStatic] private static MD5 _md5Hasher;

        public DefaultCacheKeyBuilder(CacheConfiguration configuration)
        {
            CacheConfiguration = configuration;
        }
        public string Generate(string sql, object param, string customKey, int? pageIndex = default, int? pageSize = default)
        {
            if (!string.IsNullOrWhiteSpace(customKey))
                return $"{CacheConfiguration.KeyPrefix}{(string.IsNullOrWhiteSpace(CacheConfiguration.KeyPrefix) ? "" : ":")}{customKey}";

            if (string.IsNullOrWhiteSpace(sql))
                throw new ArgumentNullException(nameof(sql));

            var builder = new StringBuilder();
            builder.AppendFormat("{0}:", sql);

            if (param == null)
                return $"{CacheConfiguration.KeyPrefix}{(string.IsNullOrWhiteSpace(CacheConfiguration.KeyPrefix) ? "" : ":")}{HashCacheKey(builder.ToString().TrimEnd(':'))}";

            var prop = GetProperties(param);
            foreach (var item in prop)
            {
                builder.AppendFormat("{0}={1}&", item.Name, item.GetValue(param));
            }
            if (pageIndex.HasValue)
            {
                builder.AppendFormat("pageindex={0}&", pageIndex.Value);
            }
            if (pageSize.HasValue)
            {
                builder.AppendFormat("pagesize={0}&", pageSize.Value);
            }
            return $"{CacheConfiguration.KeyPrefix}{(string.IsNullOrWhiteSpace(CacheConfiguration.KeyPrefix) ? "" : ":")}{HashCacheKey(builder.ToString().TrimEnd('&'))}";
        }

        private static IEnumerable<PropertyInfo> GetProperties(object param)
        {
            return ParamProperties.GetOrAdd(param.GetType(), key =>
            {
                return key.GetProperties().Where(p => p.CanRead).ToList();
            });
        }

        private static string HashCacheKey(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            _md5Hasher ??= MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(source);
            var hash = _md5Hasher.ComputeHash(bytes);
            return ToString(hash);
        }

        private static string ToString(byte[] bytes)
        {
            const int byteLen = 2;
            var chars = new char[byteLen * bytes.Length];
            var index = 0;
            foreach (var item in bytes)
            {
                chars[index] = Digitals[item >> 4/* byte high */]; ++index;
                chars[index] = Digitals[item & 15/* byte low  */]; ++index;
            }
            return new string(chars);
        }

        public void Dispose()
        {
            _md5Hasher?.Dispose();
        }
    }
}
