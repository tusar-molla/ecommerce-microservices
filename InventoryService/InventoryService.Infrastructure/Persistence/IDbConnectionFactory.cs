using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InventoryService.Infrastructure.Persistence
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
