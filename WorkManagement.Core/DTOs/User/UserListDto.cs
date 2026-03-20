using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkManagement.Core.DTOs.User
{
    public class UserListDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int Role { get; set; }
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
    public class UserPagedResultDto
    {
        public List<UserListDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        public int ActiveCount => Items.Count(u => u.IsActive);     
        public int LockedCount => Items.Count(u => !u.IsActive);   
        public int ManagerCount => Items.Count(u => u.Role == 2);   
    }
}
