using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO
{
    /// <summary>
    /// DTO para cadastro de cliente.
    /// </summary>
    public class RegisterClientDto : BaseDto
    {
        /// <summary>
        /// E-mail do cliente.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Senha do cliente.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Nome do cliente.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Valida os dados do DTO usando FluentValidation.
        /// </summary>
        /// <returns>True se válido, senão false.</returns>
        public override bool IsValid()
        {
            ValidationResult = new RegisterClientDtoValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
