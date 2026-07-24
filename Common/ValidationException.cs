using FluentValidation.Results;

namespace ApiBase.Common;

public class ValidationException : Exception
{
    public IEnumerable<ValidationFailure> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> errors)
        : base("Ocurrieron uno o más errores de validación")
    {
        Errors = errors;
    }

    public ValidationException(string property, string message)
        : base(message)
    {
        Errors = new[] { new ValidationFailure(property, message) };
    }
}
