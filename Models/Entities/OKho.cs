using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyKhoLogistics.Models.Entities
{
    [Table("OKho")] 
    public class OKho
    {
        [Key]
        [Column("ViTriId")]
        public int ViTriId { get; set; } 

        [Column("MaViTri")]
        public string MaViTri { get; set; } = string.Empty;

        [Column("KhuId")] 
        public int KhuId { get; set; }

        // Cột này rất quan trọng để biết ô nào đang chứa sản phẩm nào
        [Column("SanPhamId")]
        public int? SanPhamId { get; set; }

        [Column("TrangThai")]
        public int TrangThai { get; set; } = 0; // 0: Trống, 1: Có hàng

        // Cột ghi chú hoặc loại ô nếu bạn có dùng trong SQL
        [Column("LoaiO")]
        public string? LoaiO { get; set; }

        // --- CÁC THUỘC TÍNH PHỤ TRỢ (DÙNG ĐỂ HIỂN THỊ - KHÔNG CÓ TRONG BẢNG OKHO) ---
        // Sử dụng [NotMapped] để EF không báo lỗi khi không tìm thấy cột trong DB

        [NotMapped] 
        public string? TenSP { get; set; }

        [NotMapped] 
        public string? TenDanhMuc { get; set; } // Để lọc theo Khu (Điện tử, Thời trang...)

        [NotMapped] 
        public int? SoLuong { get; set; }
         public int? SanPhamID { get; set; }
        
        [NotMapped] 
        public int SoThuTu { get; set; }
        public int? DanhMucId { get; set; }
    }
}