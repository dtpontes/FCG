﻿using AutoMapper;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FCG.Service.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public async Task CreateUserAsync_ReturnsUser_WhenValid1()
        {
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            // Mock do IMapper
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<RegisterUserResponseDto>(It.IsAny<IdentityUser>()))
                .Returns(new RegisterUserResponseDto { Email = "test@test.com" });

            var contextAccessor = new Mock<IHttpContextAccessor>().Object;
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>().Object;
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>().Object;
            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;


            var userConfirmationMock = new Mock<IUserConfirmation<IdentityUser>>().Object;

            var signInManager = new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessor,
                claimsFactory,
                options,
                logger,
                schemes,
                userConfirmationMock
            );

            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(),
                new IRoleValidator<IdentityRole>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<ILogger<RoleManager<IdentityRole>>>());

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
                signInManager,
                mapperMock.Object);

            var dto = new RegisterUserDto { Email = "test@test.com", Password = "P@ssword123" };
            // Simule validação
            dto.ValidationResult = null;

            // Act
            var result = await service.CreateUser(dto, "Admin");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@test.com", result.Email);
        }

        [Fact]
        public async Task SendResetPasswordToken_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            // Mock do IMapper
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<RegisterUserResponseDto>(It.IsAny<IdentityUser>()))
                .Returns(new RegisterUserResponseDto { Email = "test@test.com" });

            var contextAccessor = new Mock<IHttpContextAccessor>().Object;
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>().Object;
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>().Object;
            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;


            var userConfirmationMock = new Mock<IUserConfirmation<IdentityUser>>().Object;

            var signInManager = new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessor,
                claimsFactory,
                options,
                logger,
                schemes,
                userConfirmationMock
            );

            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(),
                new IRoleValidator<IdentityRole>[0], 
                Mock.Of<ILookupNormalizer>(),        
                Mock.Of<IdentityErrorDescriber>(),   
                Mock.Of<ILogger<RoleManager<IdentityRole>>>());
            

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
                signInManager,
                mapperMock.Object);

            var dto = new ResetPasswordDto { Email = "found@test.com", Password = "Newpass@1234", Token = "token" };
            // Simulate valid
            dto.ValidationResult = null;

            // Act
            var result = await service.ResetPassword(dto);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ResetPassword_ReturnsFalse_WhenDtoIsInvalid()
        {
            // Arrange
            // Arrange
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                Mock.Of<IPasswordHasher<IdentityUser>>(),
                new IUserValidator<IdentityUser>[0],
                new IPasswordValidator<IdentityUser>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<IServiceProvider>(),
                Mock.Of<ILogger<UserManager<IdentityUser>>>());

            // Mock do IMapper
            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<RegisterUserResponseDto>(It.IsAny<IdentityUser>()))
                .Returns(new RegisterUserResponseDto { Email = "test@test.com" });

            var contextAccessor = new Mock<IHttpContextAccessor>().Object;
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>().Object;
            var logger = new Mock<ILogger<SignInManager<IdentityUser>>>().Object;
            var schemes = new Mock<IAuthenticationSchemeProvider>().Object;


            var userConfirmationMock = new Mock<IUserConfirmation<IdentityUser>>().Object;

            var signInManager = new SignInManager<IdentityUser>(
                userManagerMock.Object,
                contextAccessor,
                claimsFactory,
                options,
                logger,
                schemes,
                userConfirmationMock
            );

            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(),
                new IRoleValidator<IdentityRole>[0],
                Mock.Of<ILookupNormalizer>(),
                Mock.Of<IdentityErrorDescriber>(),
                Mock.Of<ILogger<RoleManager<IdentityRole>>>());

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
                signInManager,
                mapperMock.Object);

            var dto = new ResetPasswordDto { Email = "found@test.com", Password = "Newpass", Token = "token" };
            // Simulate valid
            dto.ValidationResult = null;

            // Act
            var result = await service.ResetPassword(dto);

            // Assert
            Assert.False(result);
        }
    }
}
