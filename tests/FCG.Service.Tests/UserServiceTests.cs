using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FCG.Service.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_ReturnsUser_WhenValid()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            var signInManagerMock = new Mock<SignInManager<IdentityUser>>(
                userManagerMock.Object,
                null!,
                null!,
                null!,
                null!,
                null!);

            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(),
                null!,
                null!,
                null!,
                null!);
            var notificationHandlerMock = new Mock<INotificationHandler<DomainNotification>>();
            var mediatorMock = new Mock<IMediatorHandler>();

            userManagerMock.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var service = new UserService(
                userManagerMock.Object,
                roleManagerMock.Object,
                notificationHandlerMock.Object,
                mediatorMock.Object,
                signInManagerMock.Object);

            var dto = new RegisterUserDto { Email = "test@test.com", Password = "P@ssword123" };
            // Simule validação
            dto.ValidationResult = null;

            // Act
            var result = await service.CreateUserAsync(dto, "Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }
    }
}
