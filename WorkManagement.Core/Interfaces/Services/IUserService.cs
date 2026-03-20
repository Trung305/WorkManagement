using AuthSystem.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkManagement.Core.DTOs.User;

namespace WorkManagement.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<Result<UserPagedResultDto>> GetPagedAsync(int page, int pageSize, string? search, int? role, bool? isActive);
        Task<Result<UserDto>> GetByIdAsync(int id);
        Task<Result> CreateAsync(CreateUserDto dto);
        Task<Result> UpdateAsync(UpdateUserDto dto);
        Task<Result> ToggleActiveAsync(int id);
        Task<Result> DeleteAsync(int id);
    }
}
