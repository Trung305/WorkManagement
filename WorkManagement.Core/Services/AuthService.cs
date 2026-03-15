using AuthSystem.Application.Common;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.Auth;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private IConfiguration _configuration;
        public AuthService(IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }
        public async Task<Result> RegisterAsync(RegisterDto dto)
        {
            var checkUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (checkUser != null)
            {
                return Result.Fail("Email đã tồn tại");
            }
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {

            };
            await _userRepository.AddAsync(user);
            return Result.Success();
        }
        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var checkUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (checkUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(dto.Password, checkUser.PasswordHash))
                {
                    var accessToken = GenerateAccessToken(checkUser);
                    var refreshToken = GenerateRefreshToken();
                    await _refreshTokenRepository.RevokeAllByUserIdAsync(checkUser.Id);
                    await _refreshTokenRepository.AddAsync(new RefreshToken
                    {
                        UserId = checkUser.Id,
                        Token = refreshToken,
                        ExpiresAt = DateTime.UtcNow.AddDays(7),
                        IsRevoked = false,
                        CreatedAt = DateTime.UtcNow,
                    });
                    return Result<LoginResponseDto>.Success(new LoginResponseDto { AccessToken = accessToken, RefreshToken = refreshToken });
                }
            }
            return Result<LoginResponseDto>.Fail("Sai mật khẩu hoặc email");
        }
        public async Task<Result<TokenResponseDto>> RefreshAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            var TokenNew = GenerateRefreshToken();
            if (token != null)
            {
                if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                    return Result<TokenResponseDto>.Fail("Token không hợp lệ");
                await _refreshTokenRepository.RevokeByIdAsync(token.Id);
                await _refreshTokenRepository.AddAsync(new RefreshToken
                {
                    UserId = token.UserId,
                    Token = TokenNew,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow,
                });
                var user = await _userRepository.GetByIdAsync(token.UserId);
                if (user == null) return Result<TokenResponseDto>.Fail("User không tồn tại");
                var newAccessToken = GenerateAccessToken(user);
                return Result<TokenResponseDto>.Success(new TokenResponseDto { AccessToken = newAccessToken, RefreshToken = TokenNew });
            }
            return Result<TokenResponseDto>.Fail("Lỗi không tạo được token");
        }
        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, user.Role),
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
