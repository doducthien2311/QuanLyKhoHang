using System.ComponentModel.DataAnnotations;

namespace QuanLyKhoLogistics.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tài khoản")]
        [Display(Name = "Tên tài khoản")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        public string Role { get; set; }
    }
}
