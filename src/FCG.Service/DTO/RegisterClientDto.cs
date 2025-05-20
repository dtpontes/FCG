using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO
{
    public class RegisterClientDto : BaseDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }

        public override bool IsValid()
        {
            ValidationResult = new RegisterClientDtoValidator().Validate(this);

            return ValidationResult.IsValid;
        }
    }
}
