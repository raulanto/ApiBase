using FluentValidation;

namespace ApiBase.Common.Extensions;

public static class ValidatorExtensions
{
    public static async Task ValidateAndThrowApiAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);

        if (!result.IsValid)
            throw new Common.ValidationException(result.Errors);
    }
}
