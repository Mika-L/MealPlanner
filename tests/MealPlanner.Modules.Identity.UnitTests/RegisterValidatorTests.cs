using FluentValidation.TestHelper;

using MealPlanner.Modules.Identity.Features.Register;

namespace MealPlanner.Modules.Identity.UnitTests;

public sealed class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Should_accept_a_long_lowercase_passphrase_without_composition_rules()
    {
        // NIST : pas d'exigence majuscule/chiffre/spécial — une phrase de passe suffit.
        var result = _validator.TestValidate(
            new RegisterCommand("chef@example.com", "correct horse battery staple", "Chef"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_reject_a_password_shorter_than_the_minimum()
    {
        var result = _validator.TestValidate(new RegisterCommand("chef@example.com", "court12", null));

        result.ShouldHaveValidationErrorFor(command => command.Password)
            .WithErrorMessage("Le mot de passe doit contenir au moins 8 caractères.");
    }

    [Fact]
    public void Should_reject_a_password_longer_than_the_maximum()
    {
        var tooLong = new string('a', RegisterValidator.MaxPasswordLength + 1);

        var result = _validator.TestValidate(new RegisterCommand("chef@example.com", tooLong, null));

        result.ShouldHaveValidationErrorFor(command => command.Password);
    }

    [Fact]
    public void Should_reject_an_invalid_email()
    {
        var result = _validator.TestValidate(new RegisterCommand("pas-un-email", "correct horse battery", null));

        result.ShouldHaveValidationErrorFor(command => command.Email);
    }

    [Fact]
    public void Should_accept_a_64_character_password()
    {
        // NIST : les mots de passe longs (au moins 64 caractères) doivent être acceptés.
        var result = _validator.TestValidate(
            new RegisterCommand("chef@example.com", new string('x', 64), null));

        result.ShouldNotHaveValidationErrorFor(command => command.Password);
    }
}
