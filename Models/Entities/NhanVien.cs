using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoLogistics.Models.Entities // Thiện kiểm tra lại Namespace xem có đúng tên Project của mình không nhé
{
    [Table("NhanVien")]
    public class NhanVien
    {
        [Key]
        public int MaNV { get; set; }

        [Required(ErrorMessage = "Họ tên nhân viên không được để trống")]
        [StringLength(100)]
        public string HoTenNV { get; set; }

        [StringLength(10)]
        public string GioiTinh { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(255)]
        public string DiaChi { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string DienThoai { get; set; }

        [StringLength(50)]
        public string ChucVu { get; set; } // Ví dụ: 'Nhân viên giao hàng', 'Nhân viên bán hàng'
    }
}