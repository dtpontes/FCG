using FCG.Domain.Entities;

namespace FCG.Domain.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByIdWithUserAsync(long id);

        Task<IEnumerable<Client>> GetAllWithUserAsync();
    }
}
