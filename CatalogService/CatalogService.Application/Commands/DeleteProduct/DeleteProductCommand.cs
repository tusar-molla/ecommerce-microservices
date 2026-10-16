using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<Unit>, ICatalogWrite
    {
        public Guid Id { get; set; }
    }
}
