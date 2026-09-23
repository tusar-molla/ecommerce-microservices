using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderService.Application.Commands.PlaceOrder
{
    public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
    {
        public PlaceOrderCommandValidator()
        {
            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required.")
                .MaximumLength(500);
        }
    }
}
