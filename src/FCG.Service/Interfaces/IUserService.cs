using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCG.Service.DTO;
using Microsoft.AspNetCore.Identity;

namespace FCG.Service.Interfaces
{
    public interface IUserService
    {
        Task<IdentityUser?> CreateUserAsync(RegisterUserDto registerUserDto, string role);

        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);

        Task<bool> SendResetPasswordTokenAsync(string email);
    }
}
