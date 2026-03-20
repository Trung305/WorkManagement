using System.ComponentModel.DataAnnotations;
using WorkManagement.Core.DTOs.User;

namespace WorkManagement.Web.Models.Admin
{
    public class UserIndexViewModel
    {
        public UserPagedResultDto PagedResult { get; set; } = new();
        public string? SearchQuery { get; set; }
        public int? RoleFilter { get; set; }
        public bool? StatusFilter { get; set; }
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string Password { get; set; } = "";

        [Range(0, 2, ErrorMessage = "Vai trò không hợp lệ")]
        public int Role { get; set; } = 1; // default: User
    }

    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = "";

        [Range(0, 2, ErrorMessage = "Vai trò không hợp lệ")]
        public int Role { get; set; }

        public bool IsActive { get; set; }

        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
        public string? NewPassword { get; set; }
    }
}
