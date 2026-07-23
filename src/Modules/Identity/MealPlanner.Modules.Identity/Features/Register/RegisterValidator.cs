using FluentValidation;

namespace MealPlanner.Modules.Identity.Features.Register;

internal sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    // Politique alignée NIST SP 800-63B : la longueur prime, aucune règle de composition
    // (majuscule/chiffre/spécial) qui pousse vers des mots de passe prévisibles. On accepte tous
    // les caractères (espaces, Unicode) et les mots de passe longs ; la borne haute évite seulement
    // les abus (déni de service par condensat démesuré).
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 256;

    public RegisterValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("L'adresse email est obligatoire.")
            .EmailAddress().WithMessage("Cette adresse email n'est pas valide.");

        RuleFor(command => command.Password)
            .NotEmpty().WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(MinPasswordLength)
            .WithMessage($"Le mot de passe doit contenir au moins {MinPasswordLength} caractères.")
            .MaximumLength(MaxPasswordLength)
            .WithMessage($"Le mot de passe ne peut pas dépasser {MaxPasswordLength} caractères.");

        RuleFor(command => command.DisplayName)
            .MaximumLength(100).WithMessage("Le nom affiché ne peut pas dépasser 100 caractères.");
    }
}
