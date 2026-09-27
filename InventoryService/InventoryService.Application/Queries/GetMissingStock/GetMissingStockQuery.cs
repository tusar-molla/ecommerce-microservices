using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Queries.GetMissingStock
{
    public class GetMissingStockQuery : IRequest<IEnumerable<Guid>>
    {
    }
}
