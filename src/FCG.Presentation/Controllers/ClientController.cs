using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : BaseController
    {

        private readonly IUserService _userService;
        private readonly IClientService _clientService;

        public ClientController(IUserService userService,
                                IClientService clientService,
                                IMediatorHandler mediator,
                                INotificationHandler<DomainNotification> notifications): base(notifications, mediator)
        {
            _userService = userService;
            _clientService = clientService;
        }

        /// <summary>
        /// Registra um novo cliente e usuário associado.
        /// </summary>
        /// <param name="registerClientDto">Dados do cliente</param>
        /// <returns>Cliente criado ou erros de validação</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(Client), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterClient(RegisterClientDto registerClientDto)
        {      

            // Cria o usuário e adiciona ao papel "Client"
            var user = await _userService.CreateUserAsync(new RegisterUserDto{ Email = registerClientDto.Email, Password = registerClientDto.Password}, "user");
            if (user == null)
                return Response();

            // Cria o cliente associado ao usuário
            var client = await _clientService.CreateClientAsync(registerClientDto, user);

            return client != null? Response(client) : Response();
        }        
    }
}
