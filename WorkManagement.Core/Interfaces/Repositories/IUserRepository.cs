using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;

namespace WorkManagement.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email);
        public Task<User?> GetByIdAsync(int id);
        public System.Threading.Tasks.Task AddAsync(User user);
        public System.Threading.Tasks.Task UpdateAsync(User user);
        Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(
        int page, int pageSize, string? search, int? role, bool? isActive);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        System.Threading.Tasks.Task DeleteAsync(User user);
        Task<IEnumerable<User>> GetByRoleAsync(int role);
    }
}
