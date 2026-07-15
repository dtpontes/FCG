using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCG.Domain.Entities;
using FCG.Domain.Repositories;
using FCG.Infrastructure.Repositories;

namespace FCG.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IClientRepository? _clients;
        private IRepository<Game>? _games;
        // Add other repositories as needed

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IClientRepository Clients => _clients ??= new ClientRepository(_context);

        public IRepository<Game> Games => _games ??= new Repository<Game>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
