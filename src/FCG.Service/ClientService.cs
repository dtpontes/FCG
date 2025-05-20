using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Service.DTO;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service
{
    public class ClientService : BaseService, IClientService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientService(IUnitOfWork unitOfWork,
                            INotificationHandler<DomainNotification> notifications,
                            IMediatorHandler mediator): base(notifications, mediator) 
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Client?> CreateClientAsync(RegisterClientDto clientDto, IdentityUser user)
        {
            if (!IsValidTransaction(clientDto))
            {
                var teste = HasValidationError();
                return null;
            }

            var client = new Client
            {
                Name = clientDto.Name,
                User = user,
                UserId = user.Id
            };
            await _unitOfWork.Clients.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();
            return client;
        }
    }
}
