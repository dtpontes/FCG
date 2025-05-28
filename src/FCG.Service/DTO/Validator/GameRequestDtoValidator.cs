using FCG.Service.DTO.Request;
using FluentValidation;

namespace FCG.Service.DTO.Validator
{
    public class GameRequestDtoValidator : AbstractValidator<GameRequestDto>
    {
        public GameRequestDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O nome do jogo é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do jogo não pode exceder 100 caracteres.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("A descrição do jogo é obrigatória.")
                .MaximumLength(500).WithMessage("A descrição do jogo não pode exceder 500 caracteres.");

            RuleFor(x => x.DateRelease)
                .NotEmpty().WithMessage("A data de lançamento do jogo é obrigatória.");
        }
    }
}
