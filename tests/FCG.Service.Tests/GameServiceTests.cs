using AutoMapper;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Entities;
using FCG.Domain.Interfaces.Commons;
using FCG.Domain.Repositories;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using MediatR;
using Moq;

namespace FCG.Service.Tests
{
    public class GameServiceTests
    {
        [Fact]
        public async Task CreateGame_ReturnsGame_WhenValid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<GameResponseDto>(It.IsAny<Game>()))
                .Returns(new GameResponseDto
                {
                    Id = 1,
                    Name = "Test Game",
                    Description = "Test Description",
                    DateRelease = DateTime.Now.AddDays(-1),
                    DateUpdate = DateTime.Now
                });

            mapperMock.Setup(m => m.Map<Game>(It.IsAny<GameRequestDto>()))
                .Returns(new Game
                {
                    Name = "Test Game",
                    Description = "Test Description",
                    DateRelease = DateTime.Now.AddDays(-1),
                    DateUpdate = DateTime.Now
                });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            var dto = new GameRequestDto
            {
                Name = "Test Game",
                Description = "Test Description",
                DateRelease = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await gameService.CreateGameAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Game", result.Name);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task GetGameById_ReturnsGame_WhenExists()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var game = new Game
            {
                Id = 1,
                Name = "Test Game",
                Description = "Test Description",
                DateRelease = DateTime.Now.AddDays(-1),
                DateUpdate = DateTime.Now
            };

            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<GameResponseDto>(game))
                .Returns(new GameResponseDto
                {
                    Id = 1,
                    Name = "Test Game",
                    Description = "Test Description",
                    DateRelease = DateTime.Now.AddDays(-1),
                    DateUpdate = DateTime.Now
                });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            // Act
            var result = await gameService.GetGameByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Game", result.Name);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task DeleteGame_ReturnsTrue_WhenGameExists()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var game = new Game
            {
                Id = 1,
                Name = "Test Game",
                Description = "Test Description",
                DateRelease = DateTime.Now.AddDays(-1),
                DateUpdate = DateTime.Now
            };

            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);
            repoMock.Setup(r => r.Delete(game));

            var mapperMock = new Mock<IMapper>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            // Act
            var result = await gameService.DeleteGameAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteGame_ReturnsFalse_WhenGameDoesNotExist()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Game?)null);

            var mapperMock = new Mock<IMapper>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            // Act
            var result = await gameService.DeleteGameAsync(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllGames_ReturnsEmptyList_WhenNoGamesExist()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Game>());

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<GameResponseDto>>(It.IsAny<IEnumerable<Game>>()))
                .Returns(new List<GameResponseDto>());

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            // Act
            var result = await gameService.GetAllGamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllGames_ReturnsListOfGames_WhenGamesExist()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var games = new List<Game>
            {
                new Game { Id = 1, Name = "Game 1", Description = "Description 1", DateRelease = DateTime.Now.AddDays(-1), DateUpdate = DateTime.Now },
                new Game { Id = 2, Name = "Game 2", Description = "Description 2", DateRelease = DateTime.Now.AddDays(-2), DateUpdate = DateTime.Now }
            };

            repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(games);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<IEnumerable<GameResponseDto>>(games))
                .Returns(new List<GameResponseDto>
                {
                    new GameResponseDto { Id = 1, Name = "Game 1", Description = "Description 1", DateRelease = DateTime.Now.AddDays(-1), DateUpdate = DateTime.Now },
                    new GameResponseDto { Id = 2, Name = "Game 2", Description = "Description 2", DateRelease = DateTime.Now.AddDays(-2), DateUpdate = DateTime.Now }
                });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            // Act
            var result = await gameService.GetAllGamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task UpdateGame_ReturnsTrue_WhenGameExistsAndValid()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var game = new Game
            {
                Id = 1,
                Name = "Old Game",
                Description = "Old Description",
                DateRelease = DateTime.Now.AddDays(-10),
                DateUpdate = DateTime.Now.AddDays(-5)
            };

            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(game);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map(It.IsAny<GameRequestDto>(), game))
                .Callback<GameRequestDto, Game>((dto, g) =>
                {
                    g.Name = dto.Name;
                    g.Description = dto.Description;
                    g.DateRelease = dto.DateRelease;
                    g.DateUpdate = DateTime.Now;
                });

            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            var dto = new GameRequestDto
            {
                Name = "Updated Game",
                Description = "Updated Description",
                DateRelease = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await gameService.UpdateGameAsync(1, dto);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated Game", game.Name);
            Assert.Equal("Updated Description", game.Description);
        }

        [Fact]
        public async Task UpdateGame_ReturnsFalse_WhenGameDoesNotExist()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Game?)null);

            var mapperMock = new Mock<IMapper>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            var dto = new GameRequestDto
            {
                Name = "Updated Game",
                Description = "Updated Description",
                DateRelease = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await gameService.UpdateGameAsync(1, dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateGame_ReturnsNull_WhenValidationFails()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            var repoMock = new Mock<IRepository<Game>>();
            unitOfWorkMock.SetupGet(u => u.Games).Returns(repoMock.Object);

            var mapperMock = new Mock<IMapper>();
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            var gameService = new GameService(
                unitOfWorkMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                mapperMock.Object);

            var dto = new GameRequestDto
            {
                Name = "", // Invalid Name
                Description = "Updated Description",
                DateRelease = DateTime.Now.AddDays(-1)
            };

            // Act
            var result = await gameService.UpdateGameAsync(1, dto);

            // Assert
            Assert.Null(result);
        }
    }
}