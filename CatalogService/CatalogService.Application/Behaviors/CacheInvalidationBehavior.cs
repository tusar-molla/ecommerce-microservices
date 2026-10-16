using CatalogService.Application.Commands;
using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Behaviors
{
    public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ICacheService _cache;

        public CacheInvalidationBehavior(ICacheService cache)
        {
            _cache = cache;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next(cancellationToken);

            if (request is ICatalogWrite)
            {
                await _cache.InvalidateAsync(CacheAreas.Catalog);
            }

            return response;
        }
    }
}
