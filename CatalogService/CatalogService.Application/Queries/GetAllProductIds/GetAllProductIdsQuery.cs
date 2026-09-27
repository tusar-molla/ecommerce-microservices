using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Queries.GetAllProductIds
{
    public class GetAllProductIdsQuery : IRequest<IEnumerable<Guid>>
    {
    }
}
