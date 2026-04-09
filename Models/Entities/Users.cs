using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyKhoLogistics.Models.Entities
{
    [Table("Users")]
    public class Users
    {
        [Key]
        public int UserId { get; set; } // Khớp INT IDENTITY PRIMARY KEY

        [Required]
        public string Username { get; set; } // Khớp NVARCHAR(50)

        [Required]
        public string Password { get; set; } // Khớp NVARCHAR(255)

        public string FullName { get; set; } // Tên hiển thị

        public string Role { get; set; } // Quyền: Admin/Staff

        // CỘT LIÊN KẾT QUAN TRỌNG
        public string MaNV { get; set; } 

        // Khai báo mối quan hệ khóa ngoại (Dành cho Entity Framework)
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }
    }
}