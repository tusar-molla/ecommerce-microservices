using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Interfaces
{
    public interface IProductCatalogClient
    {
        Task<IEnumerable<Guid>> GetAllActiveProductIdsAsync();
    }
}
