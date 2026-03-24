using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Auth;

namespace WorkManagement.Core.Interfaces.Services
{
    public interface IAuthService
    {
        public Task<Result> RegisterAsync(RegisterDto dto);
        public Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto);
        public Task<Result<TokenResponseDto>> RefreshAsync(string refreshToken);
        Task<Result<LoginResponseDto>> LoginWithGoogleAsync(string email, string googleId, string fullName);
    }
}
