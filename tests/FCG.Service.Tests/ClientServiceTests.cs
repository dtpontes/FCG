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
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq;

namespace FCG.Service.Tests
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task CreateClient_ReturnsClient_WhenValid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IClientRepository>();
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

            var loggerMock = new Mock<ILogger<ClientService>>();
            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object,
                loggerMock.Object);

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

        }

        [Fact]
        public async Task GetAllClientsAsync_SemClientesCadastrados_RetornaListaVazia()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IClientRepository>();
            repoMock.Setup(r => r.GetAllWithUserAsync()).ReturnsAsync(Enumerable.Empty<Client>());
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<RegisterClientResponseDto>>(It.IsAny<IEnumerable<Client>>()))
                .Returns(Enumerable.Empty<RegisterClientResponseDto>());

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();
            var userServiceMock = new Mock<IUserService>();

            var loggerMock = new Mock<ILogger<ClientService>>();
            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object,
                loggerMock.Object);

            // Act
            var result = await clientService.GetAllClientsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllClientsAsync_ComClientesCadastrados_RetornaListaDeClientes()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IClientRepository>();

            var user1 = new IdentityUser { Id = "1", Email = "client1@test.com" };
            var user2 = new IdentityUser { Id = "2", Email = "client2@test.com" };
            var clients = new List<Client>
            {
                new Client { Id = 1, Name = "Cliente Um", UserId = "1", User = user1 },
                new Client { Id = 2, Name = "Cliente Dois", UserId = "2", User = user2 }
            };
            repoMock.Setup(r => r.GetAllWithUserAsync()).ReturnsAsync(clients);
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            var responseDtos = new List<RegisterClientResponseDto>
            {
                new RegisterClientResponseDto { Id = 1, Name = "Cliente Um", Email = "client1@test.com" },
                new RegisterClientResponseDto { Id = 2, Name = "Cliente Dois", Email = "client2@test.com" }
            };

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<RegisterClientResponseDto>>(clients))
                .Returns(responseDtos);

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();
            var userServiceMock = new Mock<IUserService>();

            var loggerMock = new Mock<ILogger<ClientService>>();
            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object,
                loggerMock.Object);

            // Act
            var result = await clientService.GetAllClientsAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal("Cliente Um", resultList[0].Name);
            Assert.Equal("client1@test.com", resultList[0].Email);
            Assert.Equal(2, resultList[1].Id);
            Assert.Equal("Cliente Dois", resultList[1].Name);
            Assert.Equal("client2@test.com", resultList[1].Email);
        }

        [Fact]
        public async Task GetClientByIdAsync_QuandoClienteExiste_RetornaDadosDoCliente()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IClientRepository>();

            var user = new IdentityUser { Id = "1", Email = "client@test.com" };
            var client = new Client { Id = 1, Name = "Nome do Cliente", UserId = "1", User = user };
            repoMock.Setup(r => r.GetByIdWithUserAsync(1)).ReturnsAsync(client);
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<RegisterClientResponseDto>(client))
                .Returns(new RegisterClientResponseDto { Id = 1, Name = "Nome do Cliente", Email = "client@test.com" });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();
            var userServiceMock = new Mock<IUserService>();

            var loggerMock = new Mock<ILogger<ClientService>>();
            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object,
                loggerMock.Object);

            // Act
            var result = await clientService.GetClientByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Nome do Cliente", result.Name);
            Assert.Equal("client@test.com", result.Email);
        }

        [Fact]
        public async Task GetClientByIdAsync_QuandoClienteNaoExiste_RetornaNulo()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IClientRepository>();
            repoMock.Setup(r => r.GetByIdWithUserAsync(999)).ReturnsAsync((Client?)null);
            unitOfWorkMock.SetupGet(u => u.Clients).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();
            var userServiceMock = new Mock<IUserService>();

            var loggerMock = new Mock<ILogger<ClientService>>();
            var clientService = new ClientService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                userServiceMock.Object,
                mapperMock.Object,
                loggerMock.Object);

            // Act
            var result = await clientService.GetClientByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

    }
}
