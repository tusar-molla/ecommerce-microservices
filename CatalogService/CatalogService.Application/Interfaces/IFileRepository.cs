using CatalogService.Application.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Interfaces
{
    public interface IFileRepository
    {
        Task<Guid> CreateAsync(FileAsset file);
        Task<IEnumerable<FileAsset>> GetByEntityAsync(string entityType, Guid entityId);
        Task SetPrimaryAsync(string entityType, Guid entityId, Guid fileId);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<FileAsset>> GetByEntityIdsAsync(string entityType, IEnumerable<Guid> entityIds);
    }
}
