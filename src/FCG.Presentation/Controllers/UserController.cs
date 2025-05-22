using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using FCG.Service.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FCG.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IClientService _clientService;

        public UserController(UserManager<IdentityUser> userManager, 
                                SignInManager<IdentityUser> signInManager,                                
                                IUserService userService,
                                IClientService clientService,
                                IMediatorHandler mediator,
                                INotificationHandler<DomainNotification> notifications) : base(notifications, mediator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _clientService = clientService;
        }

        /// <summary>
        /// Registra um novo usuário administrador.
        /// </summary>
        /// <param name="registerUserDto">Dados do usuário</param>
        /// <returns>Usuário criado ou erros de validação</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterUserResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterAdmin(RegisterUserDto registerUserDto)
        {
            var userDto = await _userService.CreateUser(registerUserDto, "Admin");
            if (userDto == null)
                return Response();
            
            return Response(userDto);
        }

        /// <summary>
        /// Realiza autenticação do usuário e retorna um token JWT em caso de sucesso.
        /// </summary>
        /// <param name="loginUserDto">Dados de login do usuário (e-mail e senha).</param>
        /// <returns>Token JWT se autenticado, ou erro de validação/autenticação.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login(LoginUserDto loginUserDto)
        {
            var(roles, user)  = await _userService.LoginAsync(loginUserDto);

            if (roles != null && user != null)
            {                
                var token = GenerateJwtToken(user, roles);
                return Response(new { token });
            }

            return Response();
        }

        /// <summary>
        /// Redefine a senha do usuário usando token.
        /// </summary>
        /// <param name="resetPasswordDto">Dados para redefinição</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("reset-password")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _userService.ResetPassword(resetPasswordDto);
            
            return Response();
        }

        /// <summary>
        /// Envia o token de redefinição de senha para o e-mail do usuário.
        /// </summary>
        /// <param name="email">E-mail do usuário</param>
        /// <returns>Resultado do envio</returns>
        [AllowAnonymous]
        [HttpPost("send-reset-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SendResetToken([FromBody] string email)
        {
            var result = await _userService.SendResetPasswordToken(email);
            if (!result)
                return BadRequest("Ocorreu algum erro, tente novamente mais tarde!");

            return Response("Link de redefinição de senha enviado para o Email");
        }


        /// <summary>
        /// Gera um token JWT para o usuário autenticado, incluindo suas roles.
        /// </summary>
        /// <param name="user">Usuário autenticado do Identity.</param>
        /// <param name="roles">Lista de roles (perfis) do usuário.</param>
        /// <returns>Token JWT como string.</returns>
        private string GenerateJwtToken(IdentityUser user, IList<string> roles)
        {
            var jwtSettings = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? "")
            };

            // Add role claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
