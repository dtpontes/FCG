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

        /// <summary>
        /// Lista todos os clientes cadastrados.
        /// </summary>
        Task<IEnumerable<RegisterClientResponseDto>> GetAllClientsAsync();

        /// <summary>
        /// Consulta um cliente específico por id.
        /// </summary>
        Task<RegisterClientResponseDto?> GetClientByIdAsync(long id);

        /// <summary>
        /// Atualiza o nome de um cliente existente.
        /// </summary>
        Task<RegisterClientResponseDto?> UpdateClientNameAsync(long id, UpdateClientDto updateClientDto);
    }
}
