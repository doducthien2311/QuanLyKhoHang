namespace QuanLyKhoLogistics.Models.Requests
{

public class NhapKhoTuDoRequest {
    public int ViTriID { get; set; }
    public string TenNCC { get; set; }
    public string TenSP { get; set; }
    public string MaSKU { get; set; }
    public string DonViTinh { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string GhiChu { get; set; }
    public int DanhMucID { get; set; }
    public string SoHoaDon { get; set; }
    public int? MaNVGiaoHang { get; set; } // ID của nhân viên thực hiện giao hàng
}
}