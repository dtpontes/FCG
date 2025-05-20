using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FCG.Domain.Core.Notifications;
using FCG.Domain.Interfaces.Commons;
using FCG.Service.DTO;
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
        [ProducesResponseType(typeof(IdentityUser), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RegisterAdmin(RegisterUserDto registerUserDto)
        {
            var user = await _userService.CreateUserAsync(registerUserDto, "Admin");
            if (user == null)
                return Response();
            
            return Response(user);
        }

        /// <summary>
        /// Realiza login e retorna um token JWT.
        /// </summary>
        /// <param name="email">E-mail do usuário</param>
        /// <param name="password">Senha</param>
        /// <returns>Token JWT</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return Response();

            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
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
            var result = await _userService.ResetPasswordAsync(resetPasswordDto);
            if (!result)
                return Response();

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
        public async Task<IActionResult> SendResetToken([FromForm] string email)
        {
            var result = await _userService.SendResetPasswordTokenAsync(email);
            if (!result)
                return BadRequest("User not found or error sending token.");

            return Ok("Reset token sent to email.");
        }


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
