using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO.Request
{
    /// <summary>
    /// DTO para atualização do nome de um cliente.
    /// </summary>
    public class UpdateClientDto : BaseDto
    {
        /// <summary>
        /// Novo nome do cliente.
        /// </summary>
        /// <example>João da Silva</example>
        public required string Name { get; set; }

        /// <summary>
        /// Valida os dados do DTO usando FluentValidation.
        /// </summary>
        /// <returns>True se válido, senão false.</returns>
        public override bool IsValid()
        {
            ValidationResult = new UpdateClientDtoValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}
