namespace QuanLyKhoLogistics.Models.Entities
{
    public class LichSuKiemKeVM
    {
        public int ID { get; set; }
        public string LoaiPhieu { get; set; } // "NHẬP" hoặc "XUẤT"
        public string SoHoaDon { get; set; }
        public DateTime NgayThucHien { get; set; }
        public string GhiChu { get; set; } 
        public decimal TongTien { get; set; }
        public string MaNV { get; set; } // Thêm để biết ai làm
        public string HoTenNV { get; set; } // Tên cột đúng trong DB của Thiện
        public string TenSP { get; set; }    // Để hiện tên sản phẩm
            public string TenKhu { get; set; }   // Để hiện tên khu (Khu A, Khu B...)
            public int SoLuong { get; set; }
        
        // Danh sách các món hàng nằm trong phiếu đó
        public List<ChiTietHangHoaVM> DanhSachHang { get; set; } = new List<ChiTietHangHoaVM>();
    }

    public class ChiTietHangHoaVM
    {
        public string TenSP { get; set; }
        public string MaSKU { get; set; } // Thêm MaSKU cho đồng bộ với phần trước
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string MaViTri { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}