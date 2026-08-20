using CatalogService.Application.Commands.CreateProduct;
using CatalogService.Application.Commands.DeleteProduct;
using CatalogService.Application.Commands.UpdateProduct;
using CatalogService.Application.Commands.UploadProductImage;
using CatalogService.Application.Queries.GetAllProducts;
using CatalogService.Application.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string name,[FromForm] string description,[FromForm] decimal price,[FromForm] string sku,[FromForm] Guid categoryId,[FromForm] List<IFormFile>? images)
        {
            var command = new CreateProductCommand
            {
                Name = name,
                Description = description,
                Price = price,
                Sku = sku,
                CategoryId = categoryId
            };

            if (images is not null)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                const long maxFileSize = 5 * 1024 * 1024;

                foreach (var image in images)
                {
                    if (!allowedTypes.Contains(image.ContentType))
                    {
                        return BadRequest(new { Message = $"'{image.FileName}' is not a valid image type." });
                    }
                    if (image.Length > maxFileSize)
                    {
                        return BadRequest(new { Message = $"'{image.FileName}' exceeds the 5 MB limit." });
                    }

                    command.Images.Add(new UploadedFileData
                    {
                        FileStream = image.OpenReadStream(),
                        FileName = image.FileName,
                        ContentType = image.ContentType
                    });
                }
            }

            var productId = await _mediator.Send(command);
            return Ok(new { ProductId = productId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest(new { Message = "Route Id and body Id do not match." });
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteProductCommand { Id = id });
            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery { Id = id });
            if (product is null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 20,[FromQuery] Guid? categoryId = null)
        {
            var query = new GetAllProductsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                CategoryId = categoryId
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/image")]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { Message = "No file was uploaded." });
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
            {
                return BadRequest(new { Message = "Only JPEG, PNG, and WEBP images are allowed." });
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { Message = "File size must not exceed 5 MB." });
            }

            using var stream = file.OpenReadStream();

            var command = new UploadProductImageCommand
            {
                ProductId = id,
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            };

            var imageUrl = await _mediator.Send(command);
            return Ok(new { ImageUrl = imageUrl });
        }

    }
}
