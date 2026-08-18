using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
              .NotEmpty().WithMessage("Email is required.")
              .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.Password)
         .NotEmpty().WithMessage("Password is required.")
         .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.");
        }
    }
}
