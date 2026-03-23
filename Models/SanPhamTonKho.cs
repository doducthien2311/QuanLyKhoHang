namespace QuanLyKhoLogistics.Models
{
    public class SanPhamTonKho
    {
        public string MaSKU { get; set; }
        public string TenSP { get; set; }
        public string TenDanhMuc { get; set; } // Thêm dòng này
        public int SoLuong { get; set; }
        public string DonViTinh { get; set; }  // Thêm dòng này
        public string MaViTri { get; set; }   // Thêm dòng này
    }
}