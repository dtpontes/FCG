using System.Text.Json.Serialization;
using FluentValidation.Results;

namespace FCG.Service.DTO
{
    public abstract class BaseDto
    {
        public abstract bool IsValid();

        [JsonIgnore] // System.Text.Json
        [Newtonsoft.Json.JsonIgnore] // Newtonsoft.Json
        public ValidationResult? ValidationResult { get; set; }
    }
      
}
