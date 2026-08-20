using CatalogService.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace CatalogService.Application.Commands.DeleteImage
{
    public class DeleteImageCommandHandler : IRequestHandler<DeleteImageCommand, Unit>
    {
        private readonly IFileRepository _fileRepository;

        public DeleteImageCommandHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }

        public async Task<Unit> Handle(DeleteImageCommand request, CancellationToken cancellationToken)
        {
            var files = await _fileRepository.GetByEntityAsync("Product", request.ProductId);
            var targetFile = files.FirstOrDefault(f => f.Id == request.ImageId);

            if (targetFile is null)
            {
                throw new InvalidOperationException("Image not found for this product.");
            }

            await _fileRepository.DeleteAsync(request.ImageId);

            return Unit.Value;
        }
    }
}
