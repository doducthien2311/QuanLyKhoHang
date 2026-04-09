namespace QuanLyKhoLogistics.Models.Requests
{

public class NhapKhoTuDoRequest {
    public int ViTriId { get; set; }
    public int NccID { get; set; } // Phải là NccID (khớp SQL)
    public string TenNCC { get; set; }
    public string TenSP { get; set; }
    public string MaSKU { get; set; }
    public string DVT { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }
    public string GhiChu { get; set; }
    public int KhuId { get; set; }
    public string SoHoaDon { get; set; }
    public int? MaNVGiaoHang { get; set; } // ID của nhân viên thực hiện giao hàng
    // THÊM 3 DÒNG NÀY ĐỂ HẾT BÁO ĐỎ:
  
    public string DiaChiNCC { get; set; }     // Khớp với @diachi
    public string SoDienThoaiNCC { get; set; } // Khớp với @sdt
    public string NhanVienThucHien { get; set; }
}
}