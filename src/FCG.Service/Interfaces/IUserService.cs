using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCG.Service.DTO.Request;
using FCG.Service.DTO.Response;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// Cria um novo usuário e atribui um papel.
        /// </summary>
        /// <param name="registerUserDto">DTO com os dados do usuário.</param>
        /// <param name="role">Nome do papel a ser atribuído.</param>
        /// <returns>DTO de resposta do usuário criado ou null em caso de erro.</returns>
        Task<RegisterUserResponseDto?> CreateUser(RegisterUserDto registerUserDto, string role);

        /// <summary>
        /// Redefine a senha do usuário usando um token.
        /// </summary>
        /// <param name="resetPasswordDto">DTO com os dados para redefinição.</param>
        /// <returns>True se a senha foi redefinida com sucesso, senão false.</returns>
        Task<bool> ResetPassword(ResetPasswordDto resetPasswordDto);

        /// <summary>
        /// Envia um token de redefinição de senha para o e-mail do usuário.
        /// </summary>
        /// <param name="email">E-mail do usuário.</param>
        /// <returns>True se o token foi enviado, senão false.</returns>
        Task<bool> SendResetPasswordToken(string email);

        /// <summary>
        /// Realiza o login do usuário e retorna suas roles e dados.
        /// </summary>
        /// <param name="loginUserDto">DTO com os dados de login.</param>
        /// <returns>Tupla com as roles e o usuário, ou null em caso de falha.</returns>
        Task<(IList<string>? Success, IdentityUser? User)> LoginAsync(LoginUserDto loginUserDto);

        /// <summary>
        /// Cria um novo usuário do Identity e atribui um papel.
        /// </summary>
        /// <param name="registerUserDto">DTO com os dados do usuário.</param>
        /// <param name="role">Nome do papel a ser atribuído.</param>
        /// <returns>Usuário Identity criado ou null em caso de erro.</returns>
        Task<IdentityUser?> CreateUserAsync(RegisterUserDto registerUserDto, string role);


    }


}
