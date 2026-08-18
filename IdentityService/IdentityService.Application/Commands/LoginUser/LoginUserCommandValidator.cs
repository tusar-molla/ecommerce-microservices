using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityService.Application.Commands.LoginUser
{
    public class LoginUserCommandValidator :AbstractValidator<LoginUserCommand>
    {
        public LoginUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
