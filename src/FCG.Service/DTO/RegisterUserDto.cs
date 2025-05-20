using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO
{
    public class RegisterUserDto : BaseDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }       

        public override bool IsValid()
        {
            ValidationResult = new RegisterUserDtoValidator().Validate(this);

            return ValidationResult.IsValid;
        }
    }
}
