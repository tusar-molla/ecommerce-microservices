using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
    {

        private readonly ICategoryRepository _categoryRepository;
        private readonly ICacheService _cache;
        public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository, ICacheService cache)
        {
            _categoryRepository = categoryRepository;
            _cache = cache;
        }
        public async Task<IEnumerable<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _cache.GetOrCreateAsync<List<CategoryDto>>(CacheAreas.Catalog, "categories", async () =>
            {
                var all = await _categoryRepository.GetAllAsync();
                return all.Select(c => new CategoryDto { Id = c.Id, Name = c.Name, Description = c.Description }).ToList();
            }, CacheAreas.DefaultTtl);

            return categories ?? new List<CategoryDto>();
        }
    }
}
