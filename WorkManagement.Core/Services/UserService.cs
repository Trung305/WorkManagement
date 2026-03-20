using AuthSystem.Application.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.User;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Core.Interfaces.Services;

namespace WorkManagement.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IRefreshTokenRepository _tokenRepo;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepo,
            IRefreshTokenRepository tokenRepo,
            ILogger<UserService> logger)   // thêm vào constructor
        {
            _userRepo = userRepo;
            _tokenRepo = tokenRepo;
            _logger = logger;
        }

        public async Task<Result<UserPagedResultDto>> GetPagedAsync(
    int page, int pageSize, string? search, int? role, bool? isActive)
        {
            var (items, total) = await _userRepo.GetPagedAsync(page, pageSize, search, role, isActive);

            var dto = new UserPagedResultDto
            {
                Items = items.Select(u => new UserListDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = (int)u.Role,
                    IsActive = u.IsActive,
                    AvatarUrl = u.AvatarUrl,
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLoginAt
                }).ToList(),
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return Result<UserPagedResultDto>.Success(dto);
        }

        public async Task<Result<UserDto>> GetByIdAsync(int id)
        {
            var u = await _userRepo.GetByIdAsync(id);
            if (u == null)
                return Result<UserDto>.Fail("Không tìm thấy tài khoản.");

            return Result<UserDto>.Success(new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = (int)u.Role,
                IsActive = u.IsActive,
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            });
        }

        public async Task<Result> CreateAsync(CreateUserDto dto)
        {
            if (await _userRepo.EmailExistsAsync(dto.Email))
                return Result.Fail("Email đã được sử dụng.");

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = (UserRole)dto.Role,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _userRepo.AddAsync(user);
            _logger.LogInformation("Admin created user {Email}", dto.Email);
            return Result.Success();
        }

        public async Task<Result> UpdateAsync(UpdateUserDto dto)
        {
            var user = await _userRepo.GetByIdAsync(dto.Id);
            if (user == null)
                return Result.Fail("Không tìm thấy tài khoản.");

            if (await _userRepo.EmailExistsAsync(dto.Email, excludeId: dto.Id))
                return Result.Fail("Email đã được sử dụng bởi tài khoản khác.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = (UserRole)dto.Role;
            user.IsActive = dto.IsActive;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("Admin updated user {Id}", dto.Id);
            return Result.Success();
        }

        public async Task<Result> ToggleActiveAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return Result.Fail("Không tìm thấy tài khoản.");

            user.IsActive = !user.IsActive;
            await _userRepo.UpdateAsync(user);
            return Result.Success();
        }

        public async Task<Result> DeleteAsync(int id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user == null)
                return Result.Fail("Không tìm thấy tài khoản.");

            await _tokenRepo.RevokeAllByUserAsync(id);
            await _userRepo.DeleteAsync(user);
            _logger.LogWarning("Admin deleted user {Id} ({Email})", id, user.Email);
            return Result.Success();
        }
    }
}
