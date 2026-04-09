using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoLogistics.Models.Entities // Thiện kiểm tra lại Namespace xem có đúng tên Project của mình không nhé
{
    [Table("NhanVien")]
      public class NhanVien
    {
        [Key]
        public string MaNV { get; set; } // Khóa chính - khớp với SQL NVARCHAR(50)
        
        [Required]
        public string HoTenNV { get; set; }
        
        public string GioiTinh { get; set; }
        
        public DateTime? NgaySinh { get; set; } // Dấu ? để cho phép null nếu chưa nhập ngày sinh
        
        public string DiaChi { get; set; }
        
        public string DienThoai { get; set; }
        
        public string ChucVu { get; set; }
    }
}