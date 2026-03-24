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
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

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
                return Result.Fail("Email đã tồn tại");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.Now,
            };

            await _userRepository.AddAsync(user);
            return Result.Success();
        }

        public async Task<Result<LoginResponseDto>> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Result<LoginResponseDto>.Fail("Sai mật khẩu hoặc email");

            if (!user.IsActive)
                return Result<LoginResponseDto>.Fail("Tài khoản đã bị khóa");

            // Cập nhật LastLoginAt
            user.LastLoginAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Hủy tất cả refresh token cũ, tạo mới
            await _refreshTokenRepository.RevokeAllByUserIdAsync(user.Id);
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.Now,
            });

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        public async Task<Result<TokenResponseDto>> RefreshAsync(string refreshToken)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null)
                return Result<TokenResponseDto>.Fail("Token không hợp lệ");

            if (token.IsRevoked || token.ExpiresAt < DateTime.Now)
                return Result<TokenResponseDto>.Fail("Token đã hết hạn hoặc bị thu hồi");

            var user = await _userRepository.GetByIdAsync(token.UserId);
            if (user == null)
                return Result<TokenResponseDto>.Fail("User không tồn tại");

            // Rotate refresh token
            await _refreshTokenRepository.RevokeByIdAsync(token.Id);
            var newRefreshToken = GenerateRefreshToken();
            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.Now,
            });

            var newAccessToken = GenerateAccessToken(user);
            return Result<TokenResponseDto>.Success(new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        public async Task<Result<LoginResponseDto>> LoginWithGoogleAsync(string email, string googleId, string fullName)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // Tự động tạo tài khoản nếu chưa có
                user = new User
                {
                    Email = email,
                    FullName = fullName,
                    GoogleId = googleId,
                    Role = UserRole.User,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()) // random password
                };
                await _userRepository.AddAsync(user);
            }
            else
            {
                // Cập nhật GoogleId nếu chưa có
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                    await _userRepository.UpdateAsync(user);
                }

                if (!user.IsActive)
                    return Result<LoginResponseDto>.Fail("Tài khoản đã bị khóa.");
            }

            user.LastLoginAt = DateTime.Now;
            await _userRepository.UpdateAsync(user);

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.Now.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.Now
            });

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, ((int)user.Role).ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
