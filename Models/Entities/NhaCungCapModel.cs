namespace QuanLyKhoLogistics.Models.Entities
{
    public class NhaCungCapModel
    {
        // Khớp với cột [NccID] trong SQL của Thiện
        public int NccID { get; set; } 

        // Khớp với cột [TenNCC]
        public string TenNCC { get; set; }

        // Khớp với cột [SoDienThoai]
        public string SoDienThoai { get; set; }

        // Khớp với cột [DiaChi]
        public string DiaChi { get; set; }

        // Lưu ý: Đã bỏ MaSoThue và Email vì trong ảnh SQL 
        // của Thiện không có 2 cột này.
    }
}