using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
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
        /// Teste de atualização
        /// </summary>        
        /// <returns>Cliente criado ou erros de validação</returns>
        [HttpGet("apresentacao-teste")]
        [ProducesResponseType(typeof(RegisterClientResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApresentacaoTeste()
        {
            // Cria o cliente associado ao usuário            

            return Response(true);
        }

        /// <summary>
        /// Teste de atualização
        /// </summary>        
        /// <returns>Cliente criado ou erros de validação</returns>
        [HttpGet("apresentacao-teste_atual")]
        [ProducesResponseType(typeof(RegisterClientResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ApresentacaoTesteAtual()
        {
            // Cria o cliente associado ao usuário            

            return Response(true);
        }




    }
}
