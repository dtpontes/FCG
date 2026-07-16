using FCG.Service.DTO.Request;
using FluentValidation;

namespace FCG.Service.DTO.Validator
{
    public class UpdateClientDtoValidator : AbstractValidator<UpdateClientDto>
    {
        public UpdateClientDtoValidator()
        {
            RuleFor(x => x.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .Must(name => name.Trim().Length >= 2).WithMessage("O nome deve ter no mínimo 2 caracteres.")
                .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");
        }
    }
}
