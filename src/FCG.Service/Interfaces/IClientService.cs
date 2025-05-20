using FCG.Domain.Entities;
using FCG.Service.DTO;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service.Interfaces
{
    public interface IClientService
    {
        Task<Client?> CreateClientAsync(RegisterClientDto clientDto, IdentityUser user);
    }
}
