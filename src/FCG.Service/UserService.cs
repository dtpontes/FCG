using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service
{
    public class UserService : BaseService, IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public UserService(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            SignInManager<IdentityUser> signInManager) : base(notifications, mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityUser?> CreateUserAsync(RegisterUserDto registerUserDto, string role)
        {
            if (!IsValidTransaction(registerUserDto))
            {
                return null;    
            }
            var user = new IdentityUser { UserName = registerUserDto.Email, Email = registerUserDto.Email };
            var result = await _userManager.CreateAsync(user, registerUserDto.Password);

            if (!result.Succeeded)
                return null;

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return null;
            }

            return user;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (!resetPasswordDto.IsValid())
                return false;

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return false;

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            return result.Succeeded;
        }

        public async Task<bool> SendResetPasswordTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // TODO: Implemente o envio de e-mail real aqui
            // Exemplo: await _emailService.SendAsync(email, "Password Reset", $"Seu token: {token}");

            // Para teste, apenas logue ou retorne true
            Console.WriteLine($"Reset token for {email}: {token}");
            return true;
        }

        public async Task<(IList<string>? Success, IdentityUser? User)> LoginAsync(LoginUserDto loginUserDto)
        {
            if (!loginUserDto.IsValid())
            {
                return (null,null);
            }                

            var user = await _userManager.FindByEmailAsync(loginUserDto.Email);
            if (user == null)
            {
                NotifyError("UserNotFound", "Usuário não localizado.");
                return (null, null);
            }
                

            var result = await _signInManager.PasswordSignInAsync(loginUserDto.Email, loginUserDto.Password, false, false);

            if (!result.Succeeded)
            {
                NotifyError("UserNotFound", "Usuário ou senha incorretos.");
                return (null, null);
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles == null || !roles.Any())
            {
                NotifyError("UserNotFound", "Usuário não possui acessos.");
                return (null, null);
            }


            return (roles, user);
        }
    }
}
