using CatalogService.Application.Interfaces;
using CatalogService.Application.Models;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public FileRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public async Task<Guid> CreateAsync(FileAsset file)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Files (Id, EntityType, EntityId, FileUrl, FileName, ContentType, IsPrimary, CreatedAt)
            VALUES (@Id, @EntityType, @EntityId, @FileUrl, @FileName, @ContentType, @IsPrimary, @CreatedAt)";

            await connection.ExecuteAsync(sql, file);
            return file.Id;
        }

        public async Task<IEnumerable<FileAsset>> GetByEntityAsync(string entityType, Guid entityId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
            SELECT Id, EntityType, EntityId, FileUrl, FileName, ContentType, IsPrimary, CreatedAt
            FROM Files
            WHERE EntityType = @EntityType AND EntityId = @EntityId
            ORDER BY IsPrimary DESC, CreatedAt ASC";

            return await connection.QueryAsync<FileAsset>(sql, new { EntityType = entityType, EntityId = entityId });
        }

        public async Task SetPrimaryAsync(string entityType, Guid entityId, Guid fileId)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string clearSql = @"
            UPDATE Files SET IsPrimary = 0
            WHERE EntityType = @EntityType AND EntityId = @EntityId";

            const string setSql = @"
            UPDATE Files SET IsPrimary = 1
            WHERE Id = @FileId";

            await connection.ExecuteAsync(clearSql, new { EntityType = entityType, EntityId = entityId });
            await connection.ExecuteAsync(setSql, new { FileId = fileId });
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM Files WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<FileAsset>> GetByEntityIdsAsync(string entityType, IEnumerable<Guid> entityIds)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
        SELECT Id, EntityType, EntityId, FileUrl, FileName, ContentType, IsPrimary, CreatedAt
        FROM Files
        WHERE EntityType = @EntityType AND EntityId IN @EntityIds
        ORDER BY IsPrimary DESC, CreatedAt ASC";

            return await connection.QueryAsync<FileAsset>(sql, new { EntityType = entityType, EntityIds = entityIds });
        }
    }
}
