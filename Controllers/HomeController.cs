using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QuanLyKhoLogistics.Models;
using QuanLyKhoLogistics.Models.Entities;
using QuanLyKhoLogistics.Models.Requests;
using QuanLyKhoLogistics.Models.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;
namespace QuanLyKhoLogistics.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;

    // Constructor để nạp cấu hình từ hệ thống
    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
   private string GetConnectionString()
    {
        // Nó sẽ tự tìm đến dòng "DefaultConnection" trong file appsettings.json của Thiện
        return _configuration.GetConnectionString("DefaultConnection");
    }
        public IActionResult GuiLienHe(string HoTen, string Email, string SoDienThoai, string NoiDung)
    {
        // 1. Chuẩn bị câu lệnh SQL
        string query = "INSERT INTO LienHe (HoTen, Email, SoDienThoai, NoiDung) VALUES (@HoTen, @Email, @SoDienThoai, @NoiDung)";

        // 2. Truyền tham số để chống tấn công SQL Injection
        SqlParameter[] parameters = new SqlParameter[]
        {
            new SqlParameter("@HoTen", HoTen ?? (object)DBNull.Value),
            new SqlParameter("@Email", Email ?? (object)DBNull.Value),
            new SqlParameter("@SoDienThoai", SoDienThoai ?? (object)DBNull.Value),
            new SqlParameter("@NoiDung", NoiDung ?? (object)DBNull.Value)
        };

        // 3. Gọi DatabaseHelper để thực thi
        bool result = DatabaseHelper.ThucThiLenh(query, parameters);

        if (result)
        {
            TempData["Message"] = "Gửi thông tin thành công! Chúng tôi sẽ liên hệ sớm.";
        }
        else
        {
            TempData["Message"] = "Có lỗi xảy ra, vui lòng thử lại sau.";
        }

        return RedirectToAction("Index"); // Quay lại trang chủ
    }
        // 1. TRANG CHỦ: Chỉ hiện tổng quan hoặc 4 ô màu
        public IActionResult Index() 
        {
                    // 1. Gọi hàm phụ để lấy danh sách nhân viên từ SQL
                    List<NhanVien> dsNhanVien = GetDanhSachNhanVien();

                    // 2. Bỏ vào ViewBag để bên file .cshtml có thể gọi ra dùng
                    ViewBag.ListNhanVien = dsNhanVien;

                    // 3. Trả về giao diện kho hàng
            return View(); 
        }

        // 2. GIỚI THIỆU
        public IActionResult GioiThieu() 
        {
            return View();
        }

        // 3. KHO HÀNG
        public IActionResult KhoHang() 
        {
            return View();
        }

        // 4. BẢNG TIN: Chuyển toàn bộ logic lấy bài viết về đây
        public IActionResult BangTin() 
        {
            string query = "SELECT * FROM Posts ORDER BY CreatedAt DESC";
            DataTable dt = DatabaseHelper.LayDuLieu(query);
            ViewBag.PostList = dt;
            return View();
        }

        // Hàm xử lý đăng bài: Sau khi đăng xong sẽ quay lại trang Bảng Tin
        [HttpPost]
        private List<NhanVien> GetDanhSachNhanVien()
{
    List<NhanVien> ds = new List<NhanVien>();
    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
    {
        string sql = "SELECT MaNV, HoTenNV FROM NhanVien";
        SqlCommand cmd = new SqlCommand(sql, conn);
        conn.Open();
        using (SqlDataReader rdr = cmd.ExecuteReader())
        {
            while (rdr.Read())
            {
                ds.Add(new NhanVien { 
                    MaNV = Convert.ToInt32(rdr["MaNV"]), 
                    HoTenNV = rdr["HoTenNV"].ToString() 
                });
            }
        }
    }
    return ds;
}

      [HttpPost]
public IActionResult LuuNhapKhoTuDo([FromBody] NhapKhoTuDoRequest req)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                // 1. Xử lý Nhà cung cấp (Lấy ID hoặc tạo mới nếu chưa có)
                string sqlNCC = @"
                    IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE TenNCC = @tenNCC)
                    BEGIN 
                        INSERT INTO NhaCungCap (TenNCC) VALUES (@tenNCC); 
                        SELECT SCOPE_IDENTITY(); 
                    END
                    ELSE 
                    BEGIN 
                        SELECT NccID FROM NhaCungCap WHERE TenNCC = @tenNCC; 
                    END";
                SqlCommand cmdNCC = new SqlCommand(sqlNCC, conn, trans);
                cmdNCC.Parameters.AddWithValue("@tenNCC", req.TenNCC ?? "Nha Cung Cap Moi");
                int nccId = Convert.ToInt32(cmdNCC.ExecuteScalar());

                // 2. Xử lý Sản phẩm (Lấy ID hoặc tạo mới dựa trên SKU)
                string skuActual = string.IsNullOrEmpty(req.MaSKU) ? "SKU-" + DateTime.Now.Ticks.ToString().Substring(10) : req.MaSKU;
                
                string sqlSP = @"
                    IF NOT EXISTS (SELECT 1 FROM SanPham WHERE MaSKU = @sku)
                    BEGIN 
                        INSERT INTO SanPham (TenSP, MaSKU, DonViTinh, DanhMucID, SoLuongTon) 
                        VALUES (@tenSP, @sku, @dvt, @dmId, 0); 
                        SELECT SCOPE_IDENTITY(); 
                    END
                    ELSE 
                    BEGIN 
                        SELECT SanPhamID FROM SanPham WHERE MaSKU = @sku; 
                    END";

                SqlCommand cmdSP = new SqlCommand(sqlSP, conn, trans);
                cmdSP.Parameters.AddWithValue("@tenSP", req.TenSP);
                cmdSP.Parameters.AddWithValue("@sku", skuActual);
                cmdSP.Parameters.AddWithValue("@dvt", req.DonViTinh ?? "Cái");
                cmdSP.Parameters.AddWithValue("@dmId", req.DanhMucID);
                int spId = Convert.ToInt32(cmdSP.ExecuteScalar());

                // 3. Tạo Phiếu Nhập (Duy nhất 1 lần)
                string sqlPhieu = @"INSERT INTO PhieuNhap (NccID, NgayNhap, TongTien, SoHoaDon, GhiChu) 
                                   VALUES (@ncc, GETDATE(), @tong, @shd, @note); 
                                   SELECT SCOPE_IDENTITY();";

                SqlCommand cmdP = new SqlCommand(sqlPhieu, conn, trans);
                cmdP.Parameters.AddWithValue("@ncc", nccId);
                cmdP.Parameters.AddWithValue("@tong", req.SoLuong * req.DonGia);
                cmdP.Parameters.AddWithValue("@shd", (object)req.SoHoaDon ?? DBNull.Value); 
                cmdP.Parameters.AddWithValue("@note", req.GhiChu ?? "");
                
                int phieuId = Convert.ToInt32(cmdP.ExecuteScalar());

                // 4. Tạo Chi Tiết Phiếu Nhập
                string sqlCT = "INSERT INTO ChiTietPhieuNhap (PhieuNhapID, SanPhamID, SoLuong, DonGia) VALUES (@pId, @spId, @sl, @dg)";
                SqlCommand cmdCT = new SqlCommand(sqlCT, conn, trans);
                cmdCT.Parameters.AddWithValue("@pId", phieuId);
                cmdCT.Parameters.AddWithValue("@spId", spId);
                cmdCT.Parameters.AddWithValue("@sl", req.SoLuong);
                cmdCT.Parameters.AddWithValue("@dg", req.DonGia);
                cmdCT.ExecuteNonQuery();

                // 5. CẬP NHẬT SỐ LƯỢNG TỒN (Quan trọng để dữ liệu chính xác)
                string sqlUpdateTon = "UPDATE SanPham SET SoLuongTon = ISNULL(SoLuongTon, 0) + @sl WHERE SanPhamID = @spId";
                SqlCommand cmdUpdateTon = new SqlCommand(sqlUpdateTon, conn, trans);
                cmdUpdateTon.Parameters.AddWithValue("@sl", req.SoLuong);
                cmdUpdateTon.Parameters.AddWithValue("@spId", spId);
                cmdUpdateTon.ExecuteNonQuery();

                // 6. Cập nhật trạng thái ô kho thành 'Có hàng' (Để ô kho chuyển sang màu Đỏ)
                string sqlUpKho = "UPDATE OKho SET TrangThai = N'Có hàng' WHERE ViTriID = @vtId";
                SqlCommand cmdUpKho = new SqlCommand(sqlUpKho, conn, trans);
                cmdUpKho.Parameters.AddWithValue("@vtId", req.ViTriID);
                cmdUpKho.ExecuteNonQuery();

                // Hoàn tất giao dịch
                trans.Commit();
                return Json(new { success = true, message = "Nhập kho thành công! Ô số " + req.ViTriID + " đã chuyển trạng thái." });
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Json(new { success = false, message = "Lỗi xử lý nghiệp vụ: " + ex.Message });
            }
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi kết nối cơ sở dữ liệu: " + ex.Message });
    }
}


   

        public JsonResult XacNhanNhapKho([FromBody] NhapKhoRequest req)
{
    // Kiểm tra dữ liệu đầu vào cơ bản
    if (req == null || req.ViTriID <= 0) 
        return Json(new { success = false, message = "Dữ liệu không hợp lệ!" });

    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
    {
        conn.Open();
        SqlTransaction trans = conn.BeginTransaction(); // Đảm bảo an toàn dữ liệu
        try
        {
            // BƯỚC 1: Tạo PhieuNhap mới để lấy ID
            string sqlPhieu = @"INSERT INTO PhieuNhap (NccID, NgayNhap, GhiChu) 
                                OUTPUT INSERTED.PhieuNhapID 
                                VALUES (@ncc, GETDATE(), @note)";
            SqlCommand cmd1 = new SqlCommand(sqlPhieu, conn, trans);
            cmd1.Parameters.AddWithValue("@ncc", req.NccID);
            cmd1.Parameters.AddWithValue("@note", req.GhiChu ?? "");
            int newPhieuID = (int)cmd1.ExecuteScalar();

            // BƯỚC 2: Lưu vào ChiTietPhieuNhap (Kết nối Sản phẩm với Ô kho)
            string sqlChiTiet = @"INSERT INTO ChiTietPhieuNhap (PhieuNhapID, SanPhamID, SoLuong, DonGia, ViTriID) 
                                  VALUES (@pId, @sId, @sl, @dg, @vId)";
            SqlCommand cmd2 = new SqlCommand(sqlChiTiet, conn, trans);
            cmd2.Parameters.AddWithValue("@pId", newPhieuID);
            cmd2.Parameters.AddWithValue("@sId", req.SanPhamID);
            cmd2.Parameters.AddWithValue("@sl", req.SoLuong);
            cmd2.Parameters.AddWithValue("@dg", req.DonGia);
            cmd2.Parameters.AddWithValue("@vId", req.ViTriID);
            cmd2.ExecuteNonQuery();

            // BƯỚC 3: Cập nhật trạng thái ô kho từ Trống sang Có hàng
            string sqlUpdateViTri = "UPDATE ViTriO SET TrangThai = 1 WHERE ViTriID = @vId";
            SqlCommand cmd3 = new SqlCommand(sqlUpdateViTri, conn, trans);
            cmd3.Parameters.AddWithValue("@vId", req.ViTriID);
            cmd3.ExecuteNonQuery();

            trans.Commit(); // Chốt lưu dữ liệu
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            trans.Rollback(); // Nếu 1 trong 3 bước lỗi, hủy toàn bộ để tránh sai lệch kho
            return Json(new { success = false, message = "Lỗi SQL: " + ex.Message });
        }
    }
}
        public IActionResult CreatePost(string Content)
        {
            var name = HttpContext.Session.GetString("UserName");
            var role = HttpContext.Session.GetString("UserRole");

            if (!string.IsNullOrEmpty(Content))
            {
                string query = "INSERT INTO Posts (Content, UserName, UserRole) VALUES (@content, @name, @role)";
                Microsoft.Data.SqlClient.SqlParameter[] parameters = {
                    new Microsoft.Data.SqlClient.SqlParameter("@content", Content),
                    new Microsoft.Data.SqlClient.SqlParameter("@name", name),
                    new Microsoft.Data.SqlClient.SqlParameter("@role", role)
                };
                DatabaseHelper.ThucThiLenh(query, parameters);
            }
            return RedirectToAction("BangTin"); // Đăng xong quay về trang Bảng Tin
        }

// 6. CHI TIẾT KHO: Hàm xử lý khi nhấn vào nút Quản lý kho
public IActionResult DatLichKiemKho(int id, int khuId = 1) // 1. Chuyển 'string khu' thành 'int khuId'
{
    // Giữ nguyên phần thông tin ViewBag cũ của Thiện
    ViewBag.TenKho = id switch { 1 => "Kho Hoàng Mai", 2 => "Kho Long Biên", 3 => "Kho Ba Đình", 4 => "Kho Đống Đa", _ => "Kho Tổng" };
    ViewBag.KhoId = id;
    ViewBag.KhuHienTaiId = khuId; // Dùng ID này để biết đang chọn khu nào

    // 2. Chuyển đổi KhuID sang Chữ cái đại diện (y hệt logic UPDATE SQL)
    string kyTuKhu = khuId switch
    {
        1 => "A", // Thời trang
        2 => "B", // Điện tử
        3 => "C", // Gia dụng
        4 => "D", // Thể thao
        5 => "E", // Sức khỏe
        _ => "Z"  
    };

    var danhSachO = new List<OKho>();
    for (int i = 1; i <= 100; i++)
    {
        // Giả lập logic sinh ra ID y hệt như SQL (Ví dụ ID từ 1-100, 101-200...)
        int currentViTriId = ((khuId - 1) * 100) + i; 

        danhSachO.Add(new OKho {
            SoThuTu = i,
            ViTriID = currentViTriId,
            // 3. Sử dụng kyTuKhu vừa dịch được để tạo mã A-xxx y hệt như SQL của Thiện
            // Nó sẽ ra A-001, B-001 hoặc bất kỳ mã nào Thiện muốn
            MaViTri = $"{kyTuKhu}-{i:D3}", 
            TrangThai = (i % 7 == 0 || i % 10 == 0) ? "Có hàng" : "Trống",
            KhuID = khuId 
        });
    }
  DataTable dtSP = new DataTable();
    DataTable dtNCC = new DataTable();
    try 
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            
            // 1. Lấy danh sách Sản phẩm
            string sqlSP = "SELECT SanPhamID, TenSP FROM SanPham";
            SqlDataAdapter daSP = new SqlDataAdapter(sqlSP, conn);
            daSP.Fill(dtSP); 

            // 2. Lấy danh sách Nhà cung cấp (Dựa theo ảnh Diagram của Thiện)
            string sqlNCC = "SELECT NccID, TenNCC FROM NhaCungCap"; 
            SqlDataAdapter daNCC = new SqlDataAdapter(sqlNCC, conn);
            daNCC.Fill(dtNCC);
        }
    }
    catch (Exception ex)
    {
        ViewBag.ErrorMessage = "Lỗi kết nối SQL: " + ex.Message;
    }

    // Đổ cả 2 vào ViewBag để View sử dụng
    ViewBag.DanhSachSanPham = dtSP;
    ViewBag.DanhSachNCC = dtNCC;

    return View(danhSachO);
}
public IActionResult LichSuKiemKe()
{
    var listLichSu = new List<LichSuKiemKeVM>();
    try 
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            // Truy vấn lấy dữ liệu từ bảng PhieuNhap và PhieuXuat
            string sql = @"
                SELECT PhieuNhapID as ID, 'NHẬP' as Loai, SoHoaDon, NgayNhap as Ngay, GhiChu, TongTien 
                FROM PhieuNhap
                UNION ALL
                SELECT PhieuXuatID as ID, 'XUẤT' as Loai, SoHoaDon, NgayXuat as Ngay, GhiChu, TongTien 
                FROM PhieuXuat
                ORDER BY Ngay DESC";

            SqlCommand cmd = new SqlCommand(sql, conn);
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    listLichSu.Add(new LichSuKiemKeVM {
                        ID = Convert.ToInt32(rdr["ID"]),
                        LoaiPhieu = rdr["Loai"].ToString(),
                        SoHoaDon = rdr["SoHoaDon"]?.ToString() ?? "---",
                        NgayThucHien = Convert.ToDateTime(rdr["Ngay"]),
                        GhiChu = rdr["GhiChu"]?.ToString() ?? "HOU-Admin",
                        TongTien = rdr["TongTien"] != DBNull.Value ? Convert.ToDecimal(rdr["TongTien"]) : 0
                    });
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Nếu lỗi SQL, sẽ hiện thông báo thay vì sập trang
        ViewBag.Error = "Lỗi kết nối dữ liệu: " + ex.Message;
    }
    return View(listLichSu);
}
        // 5. LIÊN HỆ
        public IActionResult LienHe() 
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

[HttpGet]
public JsonResult GetChiTietO(int viTriId)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            // 1. Câu lệnh SQL (Đã đảm bảo ct.ViTriID tồn tại sau khi bạn chạy ALTER TABLE)
            string sql = @"SELECT TOP 1 
                                ct.SanPhamID, 
                                ct.SoLuong, 
                                pn.NgayNhap, 
                                ISNULL(pn.GhiChu, N'Không có ghi chú') as GhiChu 
                           FROM ChiTietPhieuNhap ct 
                           JOIN PhieuNhap pn ON ct.PhieuNhapID = pn.PhieuNhapID 
                           WHERE ct.ViTriID = @viTriId 
                           ORDER BY pn.NgayNhap DESC"; 

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@viTriId", viTriId);
            conn.Open();

            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    return Json(new
                    {
                        success = true,
                        sanPhamId = rdr["SanPhamID"]?.ToString(),
                        soLuong = rdr["SoLuong"]?.ToString(),
                        // Xử lý ngày tháng an toàn
                        ngayNhap = rdr["NgayNhap"] != DBNull.Value 
                                   ? Convert.ToDateTime(rdr["NgayNhap"]).ToString("dd/MM/yyyy") 
                                   : "N/A",
                        ghiChu = rdr["GhiChu"]?.ToString()
                    });
                }
            }
        }
    }
    catch (Exception ex)
    {
        // Trả về lỗi chi tiết để Thiện dễ debug nếu SQL lại báo lỗi
        return Json(new { success = false, message = "Lỗi SQL: " + ex.Message });
    }
    
    return Json(new { success = false });
}
    }
}