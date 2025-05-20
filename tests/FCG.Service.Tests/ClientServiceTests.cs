using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Service.DTO;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FCG.Service.Tests
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateClientAsync_ReturnsClient_WhenValid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();
            var repoMock = new Mock<IRepository<Client>>();
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            var service = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object);

            var dto = new RegisterClientDto { Email = "client@test.com", Password = "P@ssword123", Name = "Client" };
            var user = new IdentityUser { Id = "1", Email = "client@test.com" };

            // Act
            var result = await service.CreateClientAsync(dto, user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Client", result.Name);
            Assert.Equal("1", result.UserId);
        }        
    }
}
