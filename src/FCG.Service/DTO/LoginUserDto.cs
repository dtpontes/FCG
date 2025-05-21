using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO
{
    /// <summary>
    /// DTO para cadastro de usuário administrador.
    /// </summary>
    public class LoginUserDto : BaseDto
    {
        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        /// <example>usuario@dominio.com</example>
        public required string Email { get; set; }

        /// <summary>
        /// Senha do usuário.
        /// </summary>
        /// <example>SenhaForte123!</example>
        public required string Password { get; set; }

        /// <summary>
        /// Valida os dados do DTO usando FluentValidation.
        /// </summary>
        /// <returns>True se válido, senão false.</returns>
        public override bool IsValid()
        {
            ValidationResult = new LoginUserDtoValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
