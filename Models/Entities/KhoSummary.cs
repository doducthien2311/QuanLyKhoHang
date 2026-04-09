namespace QuanLyKhoLogistics.Models.Entities
{
    public class KhoSummary
    {
        // ID chính của kho để điều hướng vào trang quản lý chi tiết
        public int KhoId { get; set; } 
        
        public string TenKho { get; set; }
        
        public string? DiaChi { get; set; } 
        
        public string? HinhAnh { get; set; } 
        
        public string? Color { get; set; } 
        
        public int NhanVien { get; set; } 
        
        public int TongSanPham { get; set; } 
        
        // SỐ LIỆU QUYẾT ĐỊNH THANH SỨC CHỨA
        public int TongSoO { get; set; } 
        
        public int SoODaCoHang { get; set; } 
        public int TongSoLuongSP { get; set; } // Thêm thuộc tính này
        
        // --- TÍNH TOÁN LOGIC (GET-ONLY PROPERTIES) ---

        // 1. Tính % dựa trên số ô thực tế
        public double PhanTramSucChua => TongSoO > 0 
            ? Math.Round((double)SoODaCoHang / TongSoO * 100, 1) 
            : 0;
        
        // 2. Tự động trả về class CSS dựa trên % (Để dùng trong class="badge @item.ColorClass")
        public string ColorClass => PhanTramSucChua >= 90 ? "bg-danger" 
                                  : PhanTramSucChua >= 70 ? "bg-warning" 
                                  : "bg-success";

        // 3. Văn bản trạng thái hiển thị trên Card (Đã sửa lại logic mốc 90% cho khớp với giao diện)
        public string TrangThaiText => PhanTramSucChua >= 90 ? "Đầy đủ" 
                                     : (PhanTramSucChua > 0 ? "Còn trống" 
                                     : "Trống");

        // 4. (Mới) Trả về màu text tương ứng để dùng cho tên Kho hoặc icon nếu Thiện muốn bộ nhận diện đồng nhất
        public string TextColorClass => PhanTramSucChua >= 90 ? "text-danger" 
                                      : PhanTramSucChua >= 70 ? "text-warning" 
                                      : "text-success";
    }
}