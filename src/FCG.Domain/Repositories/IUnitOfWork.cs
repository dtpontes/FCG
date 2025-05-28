using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCG.Domain.Entities;

namespace FCG.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Client> Clients { get; }

        IRepository<Game> Games { get; }        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
