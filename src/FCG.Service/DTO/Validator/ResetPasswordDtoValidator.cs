using FluentValidation;

namespace FCG.Service.DTO.Validator
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[!@#$%^&*(),.?""{}|<>]).+$")
                .WithMessage("Password must contain letters, numbers, and special characters.");

            RuleFor(x => x.Token)
               .NotEmpty().WithMessage("token is required.");


        }
    }
}
