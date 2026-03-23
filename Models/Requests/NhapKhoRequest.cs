namespace QuanLyKhoLogistics.Models.Requests
{

public class NhapKhoRequest {
    public int ViTriID { get; set; }
    public int NccID { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string GhiChu { get; set; }
    public bool IsNhapTay { get; set; }
    public string TenSPMoi { get; set; }
    public string MaSKUMoi { get; set; }
    public string DonViTinhMoi { get; set; }
    public int DanhMucID { get; set; }
    public int SanPhamID { get; set; }
}
}