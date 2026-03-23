using System;
using System.Collections.Generic;

namespace QuanLyKhoLogistics.Models.Entities
{
    // Class này dùng để chứa thông tin hiển thị ở danh sách chính
    public class LichSuKiemKeVM
    {
        public int ID { get; set; }
        public string LoaiPhieu { get; set; } // "NHẬP" hoặc "XUẤT"
        public string SoHoaDon { get; set; }
        public DateTime NgayThucHien { get; set; }
        public string GhiChu { get; set; } // Đây là nơi hiện mã HOU-random của Thiện
        public decimal TongTien { get; set; }
        
        // Danh sách các món hàng nằm trong phiếu đó
        public List<ChiTietHangHoaVM> DanhSachHang { get; set; } = new List<ChiTietHangHoaVM>();
    }

    // Class phụ để hiện chi tiết từng sản phẩm khi bấm vào nút "Chi tiết"
    public class ChiTietHangHoaVM
    {
        public string TenSP { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public string MaViTri { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}