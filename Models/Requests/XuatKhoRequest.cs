namespace QuanLyKhoLogistics.Models.Requests
{


public class XuatKhoRequest
{
    public int ViTriId { get; set; }
    public int SanPhamID { get; set; }
    public int SoLuong { get; set; }
    public string? TenKhachHang { get; set; }
    public string? GhiChu { get; set; }
    public int? MaKH { get; set; }
    public decimal DonGiaXuat { get; set; }
    public decimal? ChietKhau { get; set; }
    public string? SoHoaDon { get; set; }
}
}