using FCG.Domain.Entities;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;

namespace FCG.Service.Interfaces
{
    public interface IClientService
    {
        /// <summary>
        /// Serviço responsável pelas operações de clientes, incluindo cadastro e associação de usuário.
        /// </summary>
        Task<RegisterClientResponseDto?> CreateClient(RegisterClientDto registerClientDto);
    }
}
