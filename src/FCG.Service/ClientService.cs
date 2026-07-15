using AutoMapper;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FCG.Service
{
    /// <summary>
    /// Serviço responsável pelas operações de clientes, incluindo cadastro e associação de usuário.
    /// </summary>
    public class ClientService : BaseService, IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClientService> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="ClientService"/>.
        /// </summary>
        /// <param name="unitOfWork">Gerenciador de transações e repositórios.</param>
        /// <param name="notifications">Handler de notificações de domínio.</param>
        /// <param name="mediator">Handler de eventos do domínio.</param>
        /// <param name="userService">Serviço de usuários para criação e associação.</param>
        /// <param name="mapper">Instância do AutoMapper.</param>
        /// <param name="logger">Logger estruturado para diagnóstico de operações de cliente.</param>
        public ClientService(IUnitOfWork unitOfWork,
                            INotificationHandler<DomainNotification> notifications,
                            IMediatorHandler mediator,
                            IUserService userService,
                            IMapper mapper,
                            ILogger<ClientService> logger): base(notifications, mediator)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo cliente e associa a um usuário do sistema.
        /// </summary>
        /// <param name="registerClientDto">DTO com os dados do cliente.</param>
        /// <returns>DTO de resposta do cliente criado ou null em caso de erro.</returns>

        public async Task<RegisterClientResponseDto?> CreateClient(RegisterClientDto registerClientDto)
        {
            if (!IsValidTransaction(registerClientDto))
            {
                return null;
            }

            // Cria o usuário e adiciona ao papel "Client"
            var user = await _userService.CreateUserAsync(new RegisterUserDto { Email = registerClientDto.Email, Password = registerClientDto.Password }, "user");
            if (user == null)
            {
                NotifyError("UserNotFound", "Não foi possível criar o usuário.");
                return null;
            }                

            var client = new Client
            {
                Name = registerClientDto.Name,
                User = user,
                UserId = user.Id
            };

            await _unitOfWork.Clients.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<RegisterClientResponseDto>(client);

        }

        /// <summary>
        /// Lista todos os clientes cadastrados.
        /// </summary>
        /// <returns>Coleção de clientes cadastrados (vazia se não houver nenhum).</returns>
        public async Task<IEnumerable<RegisterClientResponseDto>> GetAllClientsAsync()
        {
            var clients = await _unitOfWork.Clients.GetAllWithUserAsync();

            return _mapper.Map<IEnumerable<RegisterClientResponseDto>>(clients);
        }

        /// <summary>
        /// Consulta um cliente específico por id.
        /// </summary>
        /// <param name="id">Id do cliente.</param>
        /// <returns>Dados do cliente, ou null se não encontrado.</returns>
        public async Task<RegisterClientResponseDto?> GetClientByIdAsync(long id)
        {
            var client = await _unitOfWork.Clients.GetByIdWithUserAsync(id);

            if (client == null)
            {
                _logger.LogInformation("Cliente {ClientId} não encontrado", id);
                return null;
            }

            return _mapper.Map<RegisterClientResponseDto>(client);
        }
    }
}
