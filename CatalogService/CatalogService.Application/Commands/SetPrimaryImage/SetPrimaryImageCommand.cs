using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.SetPrimaryImage
{
    public class SetPrimaryImageCommand : IRequest<Unit>
    {
        public Guid ProductId { get; set; }
        public Guid ImageId { get; set; }
    }
}
