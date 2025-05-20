using FCG.Domain.Core.Events;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FCG.Domain.Interfaces.Commons
{
    public interface IMediatorHandler
    {
        Task PublishEvent<T>(T pEvent) where T : Event;        
    }
}
