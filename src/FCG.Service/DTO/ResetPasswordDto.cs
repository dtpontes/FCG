using FCG.Service.DTO.Validator;
using FluentValidation;

namespace FCG.Service.DTO
{
    public class ResetPasswordDto : BaseDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Token { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new ResetPasswordDtoValidator().Validate(this);

            return ValidationResult.IsValid;
        }
    }
}
