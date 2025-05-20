using FluentValidation.Results;

namespace FCG.Service.DTO
{
    public abstract class BaseDto
    {
        public abstract bool IsValid();
        public ValidationResult? ValidationResult { get; set; }
    }
}
