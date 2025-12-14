using FluentValidation;

namespace GoldPriceTracker.Application.Features.Auth.Login;

/// <summary>
/// Validator for LoginCommand using FluentValidation.
/// </summary>
public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty()
            .WithMessage("ایمیل یا نام کاربری الزامی است")
            .MaximumLength(255)
            .WithMessage("ایمیل یا نام کاربری نمی‌تواند بیشتر از 255 کاراکتر باشد");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("رمز عبور الزامی است")
            .MinimumLength(6)
            .WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد");
    }
}

