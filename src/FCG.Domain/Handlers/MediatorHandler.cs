using FCG.Domain.Interfaces.Commons;
using MediatR;

namespace FCG.Domain.Handlers
{
    public class MediatorHandler : IMediatorHandler
    {
        private readonly IMediator _mediator;

        public MediatorHandler(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public async Task PublishEvent<T>(T pEvent) where T : Core.Events.Event
        {
            await _mediator.Publish(pEvent);
        }
    }
}