using FluentValidation;
using System.Linq;

namespace ApiBase.Application.Validators;

public static class CustomRules
{
    // Validar contraseña segura estándar (reutilizable)
    public static IRuleBuilderOptions<T, string> Password<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe contener al menos una minúscula")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Debe contener al menos un carácter especial");
    }



    private static bool ValidarDigitoVerificadorCLABE(string clabe)
    {
        if (string.IsNullOrEmpty(clabe) || clabe.Length != 18) return false;
        int[] pesos = { 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7, 1, 3, 7 };
        int suma = pesos.Select((p, i) => p * int.Parse(clabe[i].ToString()) % 10).Sum();
        int digitoEsperado = (10 - suma % 10) % 10;
        return digitoEsperado == int.Parse(clabe[17].ToString());
    }
}
