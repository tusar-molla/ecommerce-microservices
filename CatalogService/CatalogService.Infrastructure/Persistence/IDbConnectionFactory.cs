using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CatalogService.Infrastructure.Persistence
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
