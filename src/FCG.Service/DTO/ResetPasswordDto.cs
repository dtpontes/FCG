using FCG.Service.DTO.Validator;
using FluentValidation;

namespace FCG.Service.DTO
{
    /// <summary>
    /// DTO para redefinição de senha.
    /// </summary>
    public class ResetPasswordDto : BaseDto
    {
        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Nova senha.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Token de redefinição de senha.
        /// </summary>
        public required string Token { get; set; }

        /// <summary>
        /// Valida os dados do DTO usando FluentValidation.
        /// </summary>
        /// <returns>True se válido, senão false.</returns>
        public override bool IsValid()
        {
            ValidationResult = new ResetPasswordDtoValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
