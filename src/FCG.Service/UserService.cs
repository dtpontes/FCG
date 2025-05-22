using AutoMapper;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service
{
    /// <summary>
    /// Serviço responsável pelas operações de usuários, autenticação e gerenciamento de senhas.
    /// </summary>
    public class UserService : BaseService, IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="UserService"/>.
        /// </summary>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        /// <param name="roleManager">Gerenciador de papéis do Identity.</param>
        /// <param name="notifications">Handler de notificações de domínio.</param>
        /// <param name="mediator">Handler de eventos do domínio.</param>
        /// <param name="signInManager">Gerenciador de autenticação do Identity.</param>
        /// <param name="mapper">Instância do AutoMapper.</param>
        public UserService(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            INotificationHandler<DomainNotification> notifications,
            IMediatorHandler mediator,
            SignInManager<IdentityUser> signInManager,
            IMapper mapper) : base(notifications, mediator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Cria um novo usuário e atribui um papel.
        /// </summary>
        /// <param name="registerUserDto">DTO com os dados do usuário.</param>
        /// <param name="role">Nome do papel a ser atribuído.</param>
        /// <returns>DTO de resposta do usuário criado ou null em caso de erro.</returns>
        public async Task<RegisterUserResponseDto?> CreateUser(RegisterUserDto registerUserDto, string role)
        {
            if (!IsValidTransaction(registerUserDto))
            {
                return null;    
            }

            var user = await CreateUserAsync(registerUserDto, role);

            if (user == null)
            {
                NotifyError("UserCreationFailed", "Falha ao criar usuário.");
                return null;
            }

            var userDto = _mapper.Map<RegisterUserResponseDto>(user);

            return userDto; 
        }


        /// <summary>
        /// Redefine a senha do usuário usando um token.
        /// </summary>
        /// <param name="resetPasswordDto">DTO com os dados para redefinição.</param>
        /// <returns>True se a senha foi redefinida com sucesso, senão false.</returns>
        public async Task<bool> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!resetPasswordDto.IsValid())
                return false;

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
                return false;

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.Password);
            return result.Succeeded;
        }

        /// <summary>
        /// Envia um token de redefinição de senha para o e-mail do usuário.
        /// </summary>
        /// <param name="email">E-mail do usuário.</param>
        /// <returns>True se o token foi enviado, senão false.</returns>
        public async Task<bool> SendResetPasswordToken(string email)
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


        /// <summary>
        /// Realiza o login do usuário e retorna suas roles e dados.
        /// </summary>
        /// <param name="loginUserDto">DTO com os dados de login.</param>
        /// <returns>Tupla com as roles e o usuário, ou null em caso de falha.</returns>
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

        /// <summary>
        /// Cria um novo usuário do Identity e atribui um papel.
        /// </summary>
        /// <param name="registerUserDto">DTO com os dados do usuário.</param>
        /// <param name="role">Nome do papel a ser atribuído.</param>
        /// <returns>Usuário Identity criado ou null em caso de erro.</returns>
        public async Task<IdentityUser?> CreateUserAsync(RegisterUserDto registerUserDto, string role)
        {

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
    }
}
