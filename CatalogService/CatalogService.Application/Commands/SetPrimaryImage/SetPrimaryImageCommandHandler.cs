using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.SetPrimaryImage
{
    public class SetPrimaryImageCommandHandler : IRequestHandler<SetPrimaryImageCommand, Unit>
    {
        private readonly IFileRepository _fileRepository;

        public SetPrimaryImageCommandHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        public async Task<Unit> Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
        {
            var files = await _fileRepository.GetByEntityAsync("Product", request.ProductId);
            var targetFile = files.FirstOrDefault(f => f.Id == request.ImageId);

            if (targetFile is null)
            {
                throw new InvalidOperationException("Image not found for this product.");
            }

            await _fileRepository.SetPrimaryAsync("Product", request.ProductId, request.ImageId);

            return Unit.Value;
        }
    }
}
