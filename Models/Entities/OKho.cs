using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoLogistics.Models.Entities
{
    [Table("OKho")] // Đảm bảo khớp với tên bảng Thiện đã đổi trong SQL
    public class OKho
    {
        [Key] // Khai báo khóa chính
        public int ViTriID { get; set; } 

        // THÊM DÒNG NÀY ĐỂ HẾT LỖI TRONG CONTROLLER
        [NotMapped] 
        public int SoThuTu { get; set; }
        
        public string MaViTri { get; set; } 
        public int KhuID { get; set; }
        public string TrangThai { get; set; } 

        [NotMapped] // Cột này không có trong DB, chỉ để tính toán logic
        public bool DaCoHang => TrangThai == "Có hàng";
        
        [NotMapped] // Cột này không có trong DB, dùng để hiển thị tên sản phẩm
        public string TenSP { get; set; }
        
        [NotMapped] // Cột này không có trong DB, dùng để hiển thị số lượng
        public int? SoLuong { get; set; }
    }
}