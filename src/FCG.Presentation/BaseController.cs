using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Presentation.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Presentation
{
    public class BaseController : ControllerBase
    {
        private readonly DomainNotificationHandler _notifications;
        private readonly IMediatorHandler _mediator;
        protected BaseController(INotificationHandler<DomainNotification> notificationHandler, IMediatorHandler mediator)
        {
            _notifications = (DomainNotificationHandler)notificationHandler;
            _mediator = mediator;
        }

        protected bool IsValidTransaction()
        {
            return !_notifications.HasNotifications();
        }

        private IActionResult ValidationErrors()
        {
            var errors = _notifications.GetNotifications().Select(n => n.Value);
            return BadRequest(ResponseObject<object>.Fail(errors));
        }

        protected new IActionResult Response()
        {
            if (!IsValidTransaction())
            {
                return ValidationErrors();
            }

            return Ok(ResponseObject<object>.Succeed());
        }

        protected IActionResult PageResponse(dynamic page)
        {
            if (page is null)
            {
                if (!IsValidTransaction())
                {
                    return ValidationErrors();
                }

                return NotFound(ResponseObject<object>.Fail(new List<string> { "Page Not Found" }));
            }

            // Ensure `page.Content` is not null before dereferencing
            if (page.Content is null)
            {
                return BadRequest(ResponseObject<object>.Fail(new List<string> { "Page content is null" }));
            }

            return Ok(ResponseObject<dynamic>.Succeed(page.Content))
                .WithHeaders(page.GeneratePaginationHttpHeaders());
        }

        protected new IActionResult Response(object entity)
        {
            if (entity is null)
            {
                if (!IsValidTransaction()) return ValidationErrors();

                return NotFound(ResponseObject<object>.Fail(new List<string> { "Not Found" }));
            }

            return Ok(ResponseObject<object>.Succeed(entity));
        }

    }
}   
    
