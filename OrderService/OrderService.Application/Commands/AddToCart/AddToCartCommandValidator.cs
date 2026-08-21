using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.AddToCart
{
    public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
    {
        public AddToCartCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }
}
