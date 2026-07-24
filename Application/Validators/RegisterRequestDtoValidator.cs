using FluentValidation;
using ApiBase.Application.DTOs;

namespace ApiBase.Application.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("El email no es válido.");
        RuleFor(x => x.Password).Password();
    }
}
