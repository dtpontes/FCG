using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : BaseController
    {        
        private readonly IClientService _clientService;

        public ClientController(IClientService clientService,
                                IMediatorHandler mediator,
                                INotificationHandler<DomainNotification> notifications): base(notifications, mediator)
        {            
            _clientService = clientService;
        }

        /// <summary>
        /// Registra um novo cliente e usuário associado
        /// </summary>
        /// <param name="registerClientDto">Dados do cliente</param>
        /// <returns>Cliente criado ou erros de validação</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterClientResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterClient(RegisterClientDto registerClientDto)
        {      
           
            // Cria o cliente associado ao usuário
            var client = await _clientService.CreateClient(registerClientDto);

            return client != null? Response(client) : Response();
        }

        /// <summary>
        /// Lista todos os clientes cadastrados
        /// </summary>
        /// <returns>Lista de clientes (vazia se não houver nenhum cadastrado)</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(IEnumerable<RegisterClientResponseDto>), 200)]
        public async Task<IActionResult> GetAllClients()
        {
            var clients = await _clientService.GetAllClientsAsync();

            return Response(clients);
        }

        /// <summary>
        /// Consulta um cliente específico por id
        /// </summary>
        /// <param name="id">Id do cliente</param>
        /// <returns>Dados do cliente, ou 404 se não encontrado</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RegisterClientResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetClientById(long id)
        {
            var client = await _clientService.GetClientByIdAsync(id);

            return Response(client);
        }

        /// <summary>
        /// Atualiza o nome de um cliente existente
        /// </summary>
        /// <param name="id">Id do cliente</param>
        /// <param name="updateClientDto">Novo nome do cliente</param>
        /// <returns>Dados atualizados do cliente, erro de validação (400) ou 404 se não encontrado</returns>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(RegisterClientResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateClient(long id, UpdateClientDto updateClientDto)
        {
            var client = await _clientService.UpdateClientNameAsync(id, updateClientDto);

            return Response(client);
        }
    }
}
