using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryService.Application.Commands.SetInitialStock
{
    public class SetInitialStockCommandValidator : AbstractValidator<SetInitialStockCommand>
    {
        public SetInitialStockCommandValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0)
                .WithMessage("Initial quantity cannot be negative.");
        }
    }
}
