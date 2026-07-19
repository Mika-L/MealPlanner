using FluentValidation;

namespace MealPlanner.Modules.Identity.Features.Register;

internal sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(command => command.Email).NotEmpty().EmailAddress();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
        RuleFor(command => command.DisplayName).MaximumLength(100);
    }
}
