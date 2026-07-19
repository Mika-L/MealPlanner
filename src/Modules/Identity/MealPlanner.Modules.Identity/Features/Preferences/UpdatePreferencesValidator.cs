using FluentValidation;

namespace MealPlanner.Modules.Identity.Features.Preferences;

internal sealed class UpdatePreferencesValidator : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesValidator()
    {
        RuleFor(command => command.Theme).NotEmpty().MaximumLength(50);
    }
}
