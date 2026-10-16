using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Interfaces
{
    public interface ICacheService
    {
        Task<T?> GetOrCreateAsync<T>(string area, string key, Func<Task<T?>> factory, TimeSpan ttl) where T : class;
        Task InvalidateAsync(string area);
    }

    public static class CacheAreas
    {
        public const string Catalog = "catalog";
        public static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);
    }
}
