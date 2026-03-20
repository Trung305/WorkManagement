using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Enums;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace WorkManagement.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Status != -1);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Status != -1);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.Status != -1)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async System.Threading.Tasks.Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Soft delete: set Status = -1 thay vì xóa khỏi DB.
        /// </summary>
        public async System.Threading.Tasks.Task DeleteAsync(User user)
        {
            user.Status = -1;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(
            int page, int pageSize, string? search, int? role, bool? isActive)
        {
            var query = _context.Users
                .Where(u => u.Status != -1)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(u =>
                    u.FullName.Contains(search) || u.Email.Contains(search));

            if (role.HasValue)
                query = query.Where(u => u.Role == (UserRole)role.Value);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Users
                .Where(u => u.Email == email && u.Status != -1);

            if (excludeId.HasValue)
                query = query.Where(u => u.Id != excludeId.Value);

            return await query.AnyAsync();
        }
        public async Task<IEnumerable<User>> GetByRoleAsync(int role)
    => await _context.Users
        .Where(u => (int)u.Role == role && u.IsActive)
        .OrderBy(u => u.FullName)
        .ToListAsync();
    }
}
