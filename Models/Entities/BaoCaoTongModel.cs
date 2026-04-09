using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyKhoLogistics.Models.Entities
{
    public class BaoCaoTongModel
    {
        // Thời gian thực hiện giao dịch (Lấy từ NgayNhap hoặc NgayXuat)
        public DateTime ThoiGian { get; set; }

        // Phân biệt đây là "Nhập kho" hay "Xuất kho"
        public string LoaiPhieu { get; set; }

        // Số hóa đơn ví dụ: HDN-177... hoặc HDX-177...
        public string SoHoaDon { get; set; }

        // Tên kho tương ứng (Hoàng Mai, Long Biên, Ba Đình, Đống Đa)
        public string TenKho { get; set; }

        // Họ tên nhân viên thực hiện (Lấy từ bảng NhanVien)
        public string NguoiThucHien { get; set; }

        // Tổng giá trị của hóa đơn đó
        public decimal GiaTri { get; set; }
    }
}