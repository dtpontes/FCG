using FCG.Service.DTO.Validator;

namespace FCG.Service.DTO.Request
{
    /// <summary>
    /// DTO para cadastro de jogos.
    /// </summary>
    public class GameRequestDto : BaseDto
    {
        /// <summary>
        /// Nome do jogo.
        /// <example>Super Mario Bros</example>
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Descrição do jogo.
        /// <example>Um jogo de aventura clássico.</example>
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Data de lançamento do jogo.
        /// <example>2023-05-28</example>
        /// </summary>
        public required DateTime DateRelease { get; set; }

        /// <summary>
        /// Valida os dados do DTO usando FluentValidation.
        /// </summary>
        /// <returns>True se válido, senão false.</returns>
        public override bool IsValid()
        {
            ValidationResult = new GameRequestDtoValidator().Validate(this);
            return ValidationResult.IsValid;
        }
    }
}