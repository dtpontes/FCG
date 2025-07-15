using AutoMapper;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FCG.Service.Tests
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateClient_ReturnsClient_WhenValid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Client>>();
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            // Mock do IMapper
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<RegisterClientResponseDto>(It.IsAny<Client>()))
                .Returns(new RegisterClientResponseDto { Email = "client@test.com", Id = 1, Name = "Nome do Usuário" });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var userServiceMock = new Mock<IUserService>();
            var identityUser = new IdentityUser { Id = "1", Email = "client@test.com" };
            userServiceMock
                .Setup(s => s.CreateUserAsync(It.IsAny<RegisterUserDto>(), "user"))
                .ReturnsAsync(identityUser);

            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object);

            var dto = new RegisterClientDto
            {
                Email = "client@test.com",
                Password = "P@ssword123",
                Name = "Nome do Usuário"
            };

            // Act
            var result = await clientService.CreateClient(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Nome do Usuário", result.Name);
            Assert.Equal(1, result.Id);
            Assert.Equal("client@test.com", "client1@test.com");
        }

    }
}
