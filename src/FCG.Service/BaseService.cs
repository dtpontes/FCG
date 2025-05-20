using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using FluentValidation.Results;
using MediatR;

namespace FCG.Service
{
    public abstract class BaseService
    {
        private readonly IMediatorHandler _mediator;        
        private bool _transactionValidation = true;

        private bool _hasError = false;

        private CancellationToken _cancellationToken = default;

        public void DisableTransactionValidation() => _transactionValidation = false;

        protected BaseService(            
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator)
        {            
            _mediator = mediator;

        }               

        protected bool IsValidTransaction(BaseDto obj)
        {
            if (!_transactionValidation) return true;

            var result = obj.IsValid();

            if (!result)
            {
                NotifyValidationError(obj.ValidationResult);
                _hasError = true;
            }


            return result;
        }

        protected void NotifyValidationError(ValidationResult validationResult)
        {
            foreach (var error in validationResult.Errors)
            {
                _mediator.PublishEvent(new DomainNotification(error.PropertyName, error.ErrorMessage));
            }
        }

        public bool HasValidationError()
        {
            return _hasError;
        }       

        public virtual void SetCancellationToken(CancellationToken cancellationToken) => _cancellationToken = cancellationToken;
        public virtual CancellationToken GetCancellationToken() => _cancellationToken;
    }
}
