using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;

namespace WorkManagement.Core.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        System.Threading.Tasks.Task AddAsync(RefreshToken token);
        System.Threading.Tasks.Task RevokeAllByUserIdAsync(int userId);
        System.Threading.Tasks.Task RevokeByIdAsync(int tokenId);
    }
}
