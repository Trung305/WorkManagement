using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.Entities;
using WorkManagement.Core.Interfaces.Repositories;
using WorkManagement.Infrastructure.Data;

namespace WorkManagement.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async System.Threading.Tasks.Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        }
        public async System.Threading.Tasks.Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
        }
        public async System.Threading.Tasks.Task RevokeAllByUserIdAsync(int userId)
        {
            await _context.RefreshTokens.Where(t => t.UserId == userId).ExecuteUpdateAsync(t => t.SetProperty(x => x.IsRevoked, true));
        }
        public async System.Threading.Tasks.Task RevokeByIdAsync(int tokenId)
        {
            await _context.RefreshTokens.Where(t => t.Id == tokenId).ExecuteUpdateAsync(t => t.SetProperty(x => x.IsRevoked, true));
        }
    }
}
