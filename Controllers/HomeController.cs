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

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connectionString) && !connectionString.Contains("TrustServerCertificate"))
            {
                connectionString += ";TrustServerCertificate=True;";
            }
            return connectionString;
        }

        // ==========================================================
        // 1. CÁC TRANG GIAO DIỆN CHÍNH (Đã bỏ BangTin)
        // ==========================================================

        public IActionResult Index()
{
    // Trang Dashboard tổng quát
    return View();
}

public IActionResult GioiThieu() => View();
public IActionResult KhoHang(int id = 0) 
{
    string connectionString = _configuration.GetConnectionString("DefaultConnection");

    // TRƯỜNG HỢP 1: HIỂN THỊ TỔNG QUAN (4 CARD) - Khi id chưa có hoặc bằng 0
    if (id == 0)
    {
        List<KhoSummary> danhSachKho = new List<KhoSummary>();
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
                string sql = @"
                    SELECT 
                        k.KhoId, k.TenKho, k.DiaChi,
                        (SELECT COUNT(*) FROM OKho WHERE KhoId = k.KhoId) as TongSoO,
                        (SELECT COUNT(*) FROM OKho WHERE KhoId = k.KhoId AND TrangThai = 1) as SoODaCoHang,
                        -- Lấy tổng số lượng từ bảng TonKho đã được cập nhật đồng bộ
                        ISNULL((
                            SELECT SUM(t.SoLuong) 
                            FROM TonKho t 
                            INNER JOIN OKho o ON t.ViTriID = o.ViTriId 
                            WHERE o.KhoId = k.KhoId
                        ), 0) as TongSoLuongSP
                    FROM Kho k";
            SqlCommand command = new SqlCommand(sql, connection);
            connection.Open();
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                   danhSachKho.Add(new KhoSummary {
                        KhoId = Convert.ToInt32(reader["KhoId"]),
                        TenKho = reader["TenKho"].ToString(),
                        DiaChi = reader["DiaChi"].ToString(),
                        TongSoO = Convert.ToInt32(reader["TongSoO"]),
                        SoODaCoHang = Convert.ToInt32(reader["SoODaCoHang"]),
                        
                        // Gán giá trị SUM từ SQL vào biến TongSanPham của Model
                        TongSanPham = Convert.ToInt32(reader["TongSoLuongSP"]) 
                    });
                }
            }
        }
        // Trả về đúng View danh sách 4 Card
        return View("KhoHang", danhSachKho); 
    }

    // TRƯỜNG HỢP 2: HIỂN THỊ CHI TIẾT Ô HÀNG (Khi click Quản lý kho, truyền id > 0)
else // TRƯỜNG HỢP 2: HIỂN THỊ CHI TIẾT
{
    List<OKho> danhSachO = new List<OKho>();
    List<KhuVuc> dsKhu = new List<KhuVuc>();
    string tenKhoHienTai = "";

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // 1. Lấy tên Kho
        string sqlTenKho = "SELECT TenKho FROM Kho WHERE KhoId = @KhoId";
        SqlCommand cmdTen = new SqlCommand(sqlTenKho, connection);
        cmdTen.Parameters.AddWithValue("@KhoId", id);
        tenKhoHienTai = cmdTen.ExecuteScalar()?.ToString();

        // 2. Lấy danh sách KHU VỰC
        string sqlKhu = "SELECT * FROM KhuVuc WHERE KhoId = @KhoId";
        SqlCommand cmdKhu = new SqlCommand(sqlKhu, connection);
        cmdKhu.Parameters.AddWithValue("@KhoId", id);
        using (SqlDataReader rKhu = cmdKhu.ExecuteReader())
        {
            while (rKhu.Read())
            {
                dsKhu.Add(new KhuVuc {
                    KhuId = Convert.ToInt32(rKhu["KhuId"]),
                    TenKhu = rKhu["TenKhu"].ToString(),
                    DienTich = rKhu["DienTich"].ToString()
                });
            }
        }

        // 3. Lấy danh sách 100 ô hàng
        string sqlO = @"SELECT o.*, sp.TenSP, sp.DanhMucId, dm.TenDanhMuc 
                        FROM OKho o 
                        LEFT JOIN SanPham sp ON o.SanPhamID = sp.SanPhamID 
                        LEFT JOIN DanhMuc dm ON sp.DanhMucId = dm.DanhMucId 
                        WHERE o.KhoId = @KhoId";
        SqlCommand command = new SqlCommand(sqlO, connection);
        command.Parameters.AddWithValue("@KhoId", id);
        using (SqlDataReader reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                OKho o = new OKho();
                o.ViTriId = Convert.ToInt32(reader["ViTriId"]);
                o.MaViTri = reader["MaViTri"].ToString(); 
                o.KhuId = Convert.ToInt32(reader["KhuId"]);
                o.TrangThai = Convert.ToInt32(reader["TrangThai"]);
                o.SanPhamID = reader["SanPhamID"] != DBNull.Value ? Convert.ToInt32(reader["SanPhamID"]) : (int?)null;
                o.TenSP = reader["TenSP"] != DBNull.Value ? reader["TenSP"].ToString() : "Trống";
                o.TenDanhMuc = reader["TenDanhMuc"] != DBNull.Value ? reader["TenDanhMuc"].ToString() : "";
                danhSachO.Add(o);
            }
        }

        // --- PHẦN SỬA LỖI Ở ĐÂY: Đưa việc nạp DataTable vào trong cùng một kết nối ---
        
        // 4. Nạp DataTable Sản phẩm
        DataTable dtSanPham = new DataTable();
        using (SqlDataAdapter da = new SqlDataAdapter("SELECT SanPhamID, TenSP, MaSKU, DanhMucId FROM SanPham", connection))
        {
            da.Fill(dtSanPham);
        }
        ViewBag.DanhSachSanPham = dtSanPham;

        // 5. Nạp DataTable Nhà cung cấp
        DataTable dtNCC = new DataTable();
        using (SqlDataAdapter da = new SqlDataAdapter("SELECT NccID, TenNCC FROM NhaCungCap", connection))
        {
            da.Fill(dtNCC);
        }
        ViewBag.DanhSachNCC = dtNCC;
    } 
    // Kết thúc using (connection) ở đây thì biến 'connection' mới hết tác dụng

    ViewBag.ListKhuVuc = dsKhu; 
    ViewBag.TenKho = tenKhoHienTai;
    ViewBag.KhoId = id;
    ViewBag.KhuHienTaiId = dsKhu.FirstOrDefault()?.KhuId ?? 0;

    return View("DatLichKiemKho", danhSachO);
}
}
[HttpPost]
public IActionResult GuiLienHe(string HoTen, string Email, string SoDienThoai, string NoiDung)
{
    // 1. Lấy chuỗi kết nối từ file appsettings.json
    string connectionString = _configuration.GetConnectionString("DefaultConnection");

    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            // 2. Câu lệnh SQL Insert (Khớp với các cột trong ảnh database của bạn)
            string sql = @"INSERT INTO [dbo].[LienHe] (HoTen, Email, SoDienThoai, NoiDung, NgayGui) 
                           VALUES (@HoTen, @Email, @SoDienThoai, @NoiDung, GETDATE())";

            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                // 3. Truyền tham số để chống SQL Injection
                cmd.Parameters.AddWithValue("@HoTen", HoTen ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@SoDienThoai", SoDienThoai ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NoiDung", NoiDung ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery(); // Thực thi lệnh lưu
            }
        }

        return Json(new { success = true, message = "Gửi thông tin và lưu Database thành công!" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi lưu DB: " + ex.Message });
    }
}
// ==========================================================
// 2. QUẢN LÝ Ô KHO & CHI TIẾT KHO (SỬA LẠI LOGIC ID)
// ==========================================================
public IActionResult DatLichKiemKho(int id, int? KhuId)
{
    List<OKho> danhSachOKho = new List<OKho>();
    List<KhuVuc> danhSachKhu = new List<KhuVuc>();
    string tenKho = "";
    string connectionString = _configuration.GetConnectionString("DefaultConnection");

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();

        // 1. Lấy tên Kho
        string sqlGetKho = "SELECT TenKho FROM Kho WHERE KhoId = @KhoId";
        using (SqlCommand cmdKho = new SqlCommand(sqlGetKho, connection))
        {
            cmdKho.Parameters.AddWithValue("@KhoId", id);
            tenKho = cmdKho.ExecuteScalar()?.ToString() ?? "Không xác định";
        } 

        // 2. Lấy danh sách Khu vực
        string sqlGetKhu = "SELECT KhoId, KhuId, TenKhu, DienTich FROM KhuVuc WHERE KhoId = @KhoId ORDER BY KhuId ASC";
        using (SqlCommand cmdKhuList = new SqlCommand(sqlGetKhu, connection))
        {
            cmdKhuList.Parameters.AddWithValue("@KhoId", id);
            using (SqlDataReader readerKhu = cmdKhuList.ExecuteReader())
            {
                while (readerKhu.Read())
                {
                    danhSachKhu.Add(new KhuVuc {
                        KhoId = Convert.ToInt32(readerKhu["KhoId"]),
                        KhuId = Convert.ToInt32(readerKhu["KhuId"]),
                        TenKhu = readerKhu["TenKhu"].ToString(),
                        DienTich = readerKhu["DienTich"]?.ToString()
                    });
                }
            }
        }

        // 3. Xử lý logic KhuId mặc định
        if (KhuId == null || KhuId == 0)
        {
            KhuId = danhSachKhu.FirstOrDefault()?.KhuId ?? 0;
        }

        // 4. Lấy danh sách Ô kho (JOIN để lấy thêm TenSP và SoLuong nếu ô đó có hàng)
        // Lưu ý: Thiện cần bảng ChiTietKho hoặc tương tự để JOIN lấy dữ liệu thực tế
// Sửa lại đoạn SQL trong Bước 4 (Lấy Ô Kho) và Bước 5 (Lấy Sản Phẩm)
string sqlGetO = @"
    SELECT 
        o.ViTriId, 
        o.MaViTri, 
        o.KhuId, 
        o.TrangThai,
        sp.TenSP, 
        dm.TenDanhMuc
    FROM OKho o
    LEFT JOIN SanPham sp ON o.SanPhamId = sp.SanPhamID
    LEFT JOIN DanhMuc dm ON sp.DanhMucId = dm.DanhMucId 
    WHERE o.KhoId = @K_KhoId AND o.KhuId = @K_KhuId 
    ORDER BY o.ViTriId ASC";

                    using (SqlCommand cmdO = new SqlCommand(sqlGetO, connection))
                    {
                        cmdO.Parameters.AddWithValue("@K_KhoId", id);
                        cmdO.Parameters.AddWithValue("@K_KhuId", KhuId);
                        using (SqlDataReader reader = cmdO.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                danhSachOKho.Add(new OKho
                                {
                                    ViTriId = Convert.ToInt32(reader["ViTriId"]),
                                    MaViTri = reader["MaViTri"].ToString(),
                                    KhuId = Convert.ToInt32(reader["KhuId"]),
                                    TrangThai = Convert.ToInt32(reader["TrangThai"]),
                                    // PHẢI THÊM 2 DÒNG NÀY:
                                    TenSP = reader["TenSP"] != DBNull.Value ? reader["TenSP"].ToString() : "Trống",
                                    TenDanhMuc = reader["TenDanhMuc"] != DBNull.Value ? reader["TenDanhMuc"].ToString() : ""
                                });
                            }
                        }
                    }
        // 5. LẤY DỮ LIỆU CHO DROPDOWN (SẢN PHẨM & NHÀ CUNG CẤP)
        // Dùng DataTable để khớp với code @foreach (DataRow row in ...) ở View của Thiện
        
        // --- Danh sách Sản phẩm ---
            // --- 5. LẤY DỮ LIỆU CHO DROPDOWN ---
            DataTable dtSanPham = new DataTable();
            // Dùng JOIN để lấy tên danh mục từ bảng DanhMuc
            string sqlSP = @"
    SELECT 
        s.SanPhamID, 
        s.TenSP, 
        s.MaSKU, 
        s.DVT, 
        s.DanhMucId, -- Sửa thành chữ 'd' thường cho đúng bảng SanPham
        d.TenDanhMuc 
    FROM SanPham s
    LEFT JOIN DanhMuc d ON s.DanhMucId = d.DanhMucId 
    ORDER BY s.TenSP ASC";
            using (SqlDataAdapter daSP = new SqlDataAdapter(sqlSP, connection))
            {
                daSP.Fill(dtSanPham);
            }
            ViewBag.DanhSachSanPham = dtSanPham;

        // --- Danh sách Nhà cung cấp ---
        DataTable dtNCC = new DataTable();
        string sqlNCC = "SELECT NccID, TenNCC FROM NhaCungCap ORDER BY TenNCC ASC";
        using (SqlDataAdapter daNCC = new SqlDataAdapter(sqlNCC, connection))
        {
            daNCC.Fill(dtNCC);
        }
        ViewBag.DanhSachNCC = dtNCC;
    }

    // 6. TRUYỀN DỮ LIỆU RA VIEW
    ViewBag.KhoId = id;
    ViewBag.TenKho = tenKho;
    ViewBag.ListKhuVuc = danhSachKhu;
    ViewBag.KhuHienTaiId = KhuId;

    return View(danhSachOKho);
}
[HttpPost]
public IActionResult ThemNCC([FromBody] NhaCungCapModel req)
{
    if (string.IsNullOrEmpty(req.TenNCC))
        return Json(new { success = false, message = "Tên NCC không được để trống!" });

    try
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            // Câu lệnh chèn và lấy ID vừa tạo ngay lập tức
            string sql = @"INSERT INTO NhaCungCap (TenNCC, DiaChi, SoDienThoai) 
                           VALUES (@ten, @dc, @sdt);
                           SELECT SCOPE_IDENTITY();";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ten", req.TenNCC);
            cmd.Parameters.AddWithValue("@dc", (object?)req.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sdt", (object?)req.SoDienThoai ?? DBNull.Value);

            // Thực thi và lấy về ID mới tạo
            int newId = Convert.ToInt32(cmd.ExecuteScalar());

            return Json(new { success = true, nccId = newId });
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi SQL: " + ex.Message });
    }
}

[HttpGet]
public JsonResult GetChiTietO(int viTriId)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            // JOIN với ChiTietKho để lấy số lượng THỰC TẾ hiện tại
            // JOIN với PhieuNhap/ChiTietPhieuNhap để lấy thêm thông tin NCC và ngày nhập gần nhất
            string sql = @"
                SELECT TOP 1 
                    sp.SanPhamID,
                    sp.TenSP, 
                    sp.MaSKU,
                    sp.DVT,
                    ctk.SoLuong AS SoLuongThucTe, 
                    ncc.TenNCC, 
                    pn.NgayNhap, 
                    ISNULL(pn.SoHoaDon, N'Trống') as SoHoaDon
                FROM ChiTietKho ctk
                JOIN SanPham sp ON ctk.SanPhamID = sp.SanPhamID
                LEFT JOIN ChiTietPhieuNhap ctpn ON sp.SanPhamID = ctpn.SanPhamID AND ctk.ViTriId = ctpn.ViTriId
                LEFT JOIN PhieuNhap pn ON ctpn.PhieuNhapID = pn.PhieuNhapID
                LEFT JOIN NhaCungCap ncc ON pn.NccID = ncc.NccID
                WHERE ctk.ViTriId = @viTriId 
                ORDER BY pn.NgayNhap DESC"; // Lấy thông tin đợt nhập mới nhất

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@viTriId", viTriId);
            
            conn.Open();
            using (SqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    return Json(new {
                        success = true,
                        spId = rdr["SanPhamID"], // Quan trọng để làm lệnh xuất
                        TenSP = rdr["TenSP"]?.ToString(),
                        MaSKU = rdr["MaSKU"]?.ToString(),
                        DVT = rdr["DVT"]?.ToString(),
                        SoLuong = rdr["SoLuongThucTe"], // Đây là số lượng thực tế trong ô
                        TenNCC = rdr["TenNCC"]?.ToString() ?? "N/A",
                        NgayNhap = rdr["NgayNhap"] != DBNull.Value ? Convert.ToDateTime(rdr["NgayNhap"]).ToString("dd/MM/yyyy HH:mm") : "N/A",
                        SoHoaDon = rdr["SoHoaDon"]?.ToString()
                    });
                }
            }
        }
    }
    catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    return Json(new { success = false, message = "Ô này hiện đang trống!" });
}
[HttpPost]
public IActionResult LuuNhapKhoTuDo([FromBody] NhapKhoTuDoRequest req)
{
    // 1. Lấy MaNV từ Session (Dạng chuỗi: NV...)
    var maNVSession = HttpContext.Session.GetString("MaNV");
    if (string.IsNullOrEmpty(maNVSession)) 
        return Json(new { success = false, message = "Session trống! Thử dùng mã tạm NV999" });

    try
    {       
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();
            try
            {
                // 1. Xử lý Nhà cung cấp
                string sqlNCC = @"
                    IF NOT EXISTS (SELECT 1 FROM NhaCungCap WHERE TenNCC = @tenNCC)
                    BEGIN 
                        INSERT INTO NhaCungCap (TenNCC, DiaChi, SoDienThoai) 
                        VALUES (@tenNCC, @diaChi, @sdt); 
                        SELECT SCOPE_IDENTITY(); 
                    END
                    ELSE SELECT NccID FROM NhaCungCap WHERE TenNCC = @tenNCC;";
                
                SqlCommand cmdNCC = new SqlCommand(sqlNCC, conn, trans);
                cmdNCC.Parameters.AddWithValue("@tenNCC", req.TenNCC ?? "NCC Vãng Lai");
                cmdNCC.Parameters.AddWithValue("@diaChi", (object)req.DiaChiNCC ?? DBNull.Value);
                cmdNCC.Parameters.AddWithValue("@sdt", (object)req.SoDienThoaiNCC ?? DBNull.Value);
                
                int nccId = Convert.ToInt32(cmdNCC.ExecuteScalar());    

                // 2. Xử lý Sản phẩm (Đồng bộ MaSKU và DanhMucID)
                string sqlSP = @"
                        IF NOT EXISTS (SELECT 1 FROM SanPham WHERE TenSP = @tenSP)
                        BEGIN 
                            -- Lưu vào cột DanhMucId vừa thêm
                            INSERT INTO SanPham (TenSP, MaSKU, DVT, SoLuongTon, DanhMucId) 
                            VALUES (@tenSP, @sku, @dvt, @sl, @dmId); 
                            SELECT SCOPE_IDENTITY(); 
                        END
                        ELSE  
                        BEGIN 
                            UPDATE SanPham SET SoLuongTon = ISNULL(SoLuongTon, 0) + @sl, DanhMucId = @dmId WHERE TenSP = @tenSP;
                            SELECT SanPhamID FROM SanPham WHERE TenSP = @tenSP; 
                        END";

                    SqlCommand cmdSP = new SqlCommand(sqlSP, conn, trans);
                    cmdSP.Parameters.AddWithValue("@tenSP", req.TenSP);
                    cmdSP.Parameters.AddWithValue("@sku", req.MaSKU ?? ("SKU-" + DateTime.Now.Ticks.ToString().Substring(10)));
                    cmdSP.Parameters.AddWithValue("@dvt", req.DVT ?? "Cái");
                    cmdSP.Parameters.AddWithValue("@sl", req.SoLuong);

                    // Gán ID danh mục từ giao diện gửi về
                    cmdSP.Parameters.AddWithValue("@dmId", req.KhuId);
                int spId = Convert.ToInt32(cmdSP.ExecuteScalar());
                // 1. Tìm KhoId từ vị trí (Thêm đoạn này vào trước khi tạo phiếu nhập)
                string sqlGetKho = "SELECT KhoId FROM OKho WHERE ViTriId = @vtId";
                SqlCommand cmdGetKho = new SqlCommand(sqlGetKho, conn, trans);
                cmdGetKho.Parameters.AddWithValue("@vtId", req.ViTriId);
                int khoIdCuaViTri = Convert.ToInt32(cmdGetKho.ExecuteScalar() ?? 1);

                // 2. Tạo Phiếu Nhập
                string sqlPhieu = @"INSERT INTO PhieuNhap 
                    (NgayNhap, SoHoaDon, NccID, MaNV, TongTien, GhiChu, KhoId) 
                    VALUES 
                    (GETDATE(), @SoHoaDon, @nccId, @maNV, @tongTien, @ghiChu, @khoId);
                    SELECT SCOPE_IDENTITY();";

            SqlCommand cmdP = new SqlCommand(sqlPhieu, conn, trans);

            // --- PHẦN NÀY PHẢI KHỚP TÊN VỚI BÊN TRÊN ---
            cmdP.Parameters.AddWithValue("@SoHoaDon", (object)req.SoHoaDon ?? ("HDN-" + DateTime.Now.Ticks));
            cmdP.Parameters.AddWithValue("@nccId", nccId); // Đã sửa từ @ncc thành @nccId cho khớp SQL
            cmdP.Parameters.AddWithValue("@maNV", maNVSession);
            cmdP.Parameters.AddWithValue("@tongTien", req.SoLuong * req.DonGiaNhap); // Đã sửa từ @tong thành @tongTien
            cmdP.Parameters.AddWithValue("@ghiChu", req.GhiChu ?? ("Nhập vị trí: " + req.ViTriId)); // Đã sửa từ @note thành @ghiChu
            cmdP.Parameters.AddWithValue("@khoId", khoIdCuaViTri);

            int phieuId = Convert.ToInt32(cmdP.ExecuteScalar());

                    // 4. Tạo Chi Tiết Phiếu & Cập nhật ChiTietKho & Cập nhật trạng thái OKho
                    string sqlFinal = @"
                        -- 1. Lưu chi tiết phiếu nhập
                        INSERT INTO ChiTietPhieuNhap (PhieuNhapID, SanPhamID, SoLuong, DonGiaNhap, ViTriId) 
                        VALUES (@pId, @spId, @sl, @dg, @vId);

                        -- 2. Cập nhật bảng ChiTietKho (Quản lý theo Ô)
                        IF EXISTS (SELECT 1 FROM ChiTietKho WHERE ViTriId = @vId)
                            UPDATE ChiTietKho SET SoLuong = SoLuong + @sl, SanPhamID = @spId, NgayCapNhat = GETDATE() WHERE ViTriId = @vId;
                        ELSE
                            INSERT INTO ChiTietKho (ViTriId, SanPhamID, SoLuong, NgayCapNhat) VALUES (@vId, @spId, @sl, GETDATE());

                        -- 3. Cập nhật bảng TonKho (Quản lý Tổng tồn theo Sản phẩm tại Vị trí)
                        IF EXISTS (SELECT 1 FROM TonKho WHERE ViTriID = @vId AND SanPhamID = @spId)
                            UPDATE TonKho SET SoLuong = SoLuong + @sl, NgayCapNhat = GETDATE() WHERE ViTriID = @vId AND SanPhamID = @spId;
                        ELSE
                            INSERT INTO TonKho (ViTriID, SanPhamID, SoLuong, NgayCapNhat) VALUES (@vId, @spId, @sl, GETDATE());

                        -- 4. Cập nhật trạng thái OKho
                        UPDATE OKho SET TrangThai = 1, SanPhamID = @spId WHERE ViTriId = @vId;";

                    SqlCommand cmdFinal = new SqlCommand(sqlFinal, conn, trans);
                    cmdFinal.Parameters.AddWithValue("@pId", phieuId);
                    cmdFinal.Parameters.AddWithValue("@spId", spId); // spId lấy từ kết quả SCOPE_IDENTITY() ở trên
                    cmdFinal.Parameters.AddWithValue("@sl", req.SoLuong);
                    cmdFinal.Parameters.AddWithValue("@dg", req.DonGiaNhap);
                    cmdFinal.Parameters.AddWithValue("@vId", req.ViTriId);
                    cmdFinal.ExecuteNonQuery();

                trans.Commit();
                return Json(new { success = true, message = "✅ Hệ thống: Đã nhập kho thành công cho vị trí " + req.ViTriId });
            }
            catch (Exception ex)
            {
                trans.Rollback();
                return Json(new { success = false, message = "Lỗi xử lý SQL: " + ex.Message });
            }
        }
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
    }
}

// 1. HÀM XUẤT KHO

[HttpPost]
public IActionResult LuuXuatKho([FromBody] XuatKhoRequest req)
{
    if (req == null || req.SoLuong <= 0 || req.SanPhamID <= 0 || req.ViTriId <= 0) 
        return Json(new { success = false, message = "⚠️ Dữ liệu đầu vào không hợp lệ!" });

    // Lấy MaNV từ Session (Tạm để "1" nếu null)
    var maNVSession = HttpContext.Session.GetString("MaNV") ?? "1"; 

    try
    {
        using (SqlConnection conn = new SqlConnection(GetConnectionString()))
        {
            conn.Open();
            using (SqlTransaction trans = conn.BeginTransaction())
            {
                try
                {
                    // --- BƯỚC 0: KIỂM TRA TỒN KHO TẠI VỊ TRÍ (Tránh trường hợp 2 người cùng xuất 1 lúc) ---
                    string sqlCheck = "SELECT SoLuong FROM ChiTietKho WHERE ViTriId = @vtId AND SanPhamID = @spId";
                    SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn, trans);
                    cmdCheck.Parameters.AddWithValue("@vtId", req.ViTriId);
                    cmdCheck.Parameters.AddWithValue("@spId", req.SanPhamID);
                    var tonTaiViTri = Convert.ToInt32(cmdCheck.ExecuteScalar() ?? 0);

                    if (tonTaiViTri < req.SoLuong) {
                        return Json(new { success = false, message = $"❌ Số lượng tại vị trí này không đủ! (Hiện có: {tonTaiViTri})" });
                    }
                    // --- BƯỚC 0.5: TỰ ĐỘNG TÌM KHOID TỪ VỊ TRÍ ĐANG CHỌN (MÌNH THÊM VÀO ĐÂY) ---
                    string sqlGetKho = "SELECT KhoId FROM OKho WHERE ViTriId = @vtId";
                    SqlCommand cmdGetKho = new SqlCommand(sqlGetKho, conn, trans);
                    cmdGetKho.Parameters.AddWithValue("@vtId", req.ViTriId);
                    // Lấy ra số KhoId (ví dụ 1 hoặc 2), nếu lỗi thì mặc định lấy kho 1
                    int khoIdCuaViTri = Convert.ToInt32(cmdGetKho.ExecuteScalar() ?? 1);
                        // 3. Tính toán tiền (Dùng cột TongTien duy nhất)
                    // --- TÍNH TOÁN TRƯỚC KHI LƯU ---
                    decimal phanTramCK = req.ChietKhau ?? 0;
                    decimal donGiaXuat = req.DonGiaXuat;
                    decimal thanhTien = req.SoLuong * donGiaXuat;

                    // Tính số tiền được giảm (Ví dụ: 10% của 1.000.000 là 100.000)
                    decimal soTienGiam = thanhTien * (phanTramCK / 100);
                    decimal tongTienSauCK = thanhTien - soTienGiam;

                    // --- BƯỚC 1: TẠO PHIẾU XUẤT ---
// --- BƯỚC 1: TẠO PHIẾU XUẤT ---
                    string sqlPhieu = @"INSERT INTO PhieuXuat 
                    (NgayXuat, TenKhachHang, GhiChu, MaKH, MaNV, ChietKhauBan, TongTien, SoHoaDon, KhoId) 
                    VALUES 
                    (GETDATE(), @tenKH, @ghiChu, @maKH, @maNV, @chietKhauBan, @tongTien, @soHD, @khoId);
                    SELECT SCOPE_IDENTITY();";

SqlCommand cmdP = new SqlCommand(sqlPhieu, conn, trans);
cmdP.Parameters.AddWithValue("@tenKH", (object?)req.TenKhachHang ?? "Khách lẻ");
cmdP.Parameters.AddWithValue("@ghiChu", (object?)req.GhiChu ?? "Xuất kho từ sơ đồ");
cmdP.Parameters.AddWithValue("@maKH", (object?)req.MaKH ?? DBNull.Value);
cmdP.Parameters.AddWithValue("@maNV", maNVSession);

// LƯU Ý: ChietKhauBan lưu SỐ TIỀN giảm giá
cmdP.Parameters.AddWithValue("@chietKhauBan", soTienGiam); 
cmdP.Parameters.AddWithValue("@tongTien", tongTienSauCK);

cmdP.Parameters.AddWithValue("@soHD", (object?)req.SoHoaDon ?? "PX-" + DateTime.Now.Ticks);
cmdP.Parameters.AddWithValue("@khoId", khoIdCuaViTri);

int phieuXuatId = Convert.ToInt32(cmdP.ExecuteScalar());

                    // --- BƯỚC 2: LƯU CHI TIẾT PHIẾU XUẤT ---
                    string sqlCT = "INSERT INTO ChiTietPhieuXuat (PhieuXuatID, SanPhamID, SoLuong, DonGiaXuat, ViTriId) VALUES (@pId, @spId, @sl, @dg, @vId)";
                    SqlCommand cmdCT = new SqlCommand(sqlCT, conn, trans);
                    cmdCT.Parameters.AddWithValue("@pId", phieuXuatId);
                    cmdCT.Parameters.AddWithValue("@spId", req.SanPhamID);
                    cmdCT.Parameters.AddWithValue("@sl", req.SoLuong);
                    cmdCT.Parameters.AddWithValue("@dg", req.DonGiaXuat);
                    cmdCT.Parameters.AddWithValue("@vId", req.ViTriId);
                    cmdCT.ExecuteNonQuery();

                    // --- BƯỚC 3: CẬP NHẬT TỒN TỔNG (Bảng SanPham) ---
                    string sqlUpTon = "UPDATE SanPham SET SoLuongTon = SoLuongTon - @sl WHERE SanPhamID = @spId";
                    SqlCommand cmdUpTon = new SqlCommand(sqlUpTon, conn, trans);
                    cmdUpTon.Parameters.AddWithValue("@sl", req.SoLuong);
                    cmdUpTon.Parameters.AddWithValue("@spId", req.SanPhamID);
                    cmdUpTon.ExecuteNonQuery();

                    // --- BƯỚC 4: CẬP NHẬT CHI TIẾT Ô KHO & TRẠNG THÁI Ô ---
                    // Sử dụng logic: Trừ xong nếu <= 0 thì Xóa bản ghi và đổi trạng thái ô thành 0 (Trắng)
                string sqlUpKho = @"
                    -- 1. Trừ số lượng ở ChiTietKho
                    UPDATE ChiTietKho SET SoLuong = SoLuong - @sl 
                    WHERE ViTriId = @vtId AND SanPhamID = @spId;
                    
                    -- 2. Trừ số lượng ở TonKho
                    UPDATE TonKho SET SoLuong = SoLuong - @sl 
                    WHERE ViTriID = @vtId AND SanPhamID = @spId;

                    -- 3. KIỂM TRA: Nếu ô đó thực sự hết sạch hàng (tổng số lượng = 0) mới reset ô
                    IF NOT EXISTS (SELECT 1 FROM ChiTietKho WHERE ViTriId = @vtId AND SoLuong > 0)
                    BEGIN
                        DELETE FROM ChiTietKho WHERE ViTriId = @vtId;
                        DELETE FROM TonKho WHERE ViTriID = @vtId;
                        UPDATE OKho SET TrangThai = 0, SanPhamID = NULL WHERE ViTriId = @vtId;
                    END
                    ELSE
                    BEGIN
                        -- Nếu vẫn còn hàng khác trong ô đó, chỉ xóa bản ghi của sản phẩm vừa xuất nếu nó hết
                        DELETE FROM ChiTietKho WHERE ViTriId = @vtId AND SanPhamID = @spId AND SoLuong <= 0;
                        DELETE FROM TonKho WHERE ViTriID = @vtId AND SanPhamID = @spId AND SoLuong <= 0;
                    END";

                    SqlCommand cmdUpKho = new SqlCommand(sqlUpKho, conn, trans);
                    cmdUpKho.Parameters.AddWithValue("@sl", req.SoLuong);
                    cmdUpKho.Parameters.AddWithValue("@vtId", req.ViTriId);
                    cmdUpKho.Parameters.AddWithValue("@spId", req.SanPhamID);
                    cmdUpKho.ExecuteNonQuery();

                    trans.Commit();
                    return Json(new { success = true, message = "✅ Xuất kho thành công!" });
                }
                catch (Exception ex) 
                { 
                    trans.Rollback(); 
                    return Json(new { success = false, message = "❌ Lỗi hệ thống: " + ex.Message }); 
                }
            }
        }
    }
    catch (Exception ex) 
    { 
        return Json(new { success = false, message = "❌ Lỗi kết nối: " + ex.Message }); 
    }
}

public IActionResult BaoCaoTong()
{
    // Lấy giá trị UserRole từ Session
    string userRole = HttpContext.Session.GetString("UserRole");

    // Nếu không phải Admin thì đá về trang chủ
    if (userRole != "Admin")
    {
        return RedirectToAction("KhoHang", "Home");
    }
    List<BaoCaoTongModel> danhSach = new List<BaoCaoTongModel>();

    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
    {
        // Query lấy toàn bộ dữ liệu từ tất cả các kho, kèm theo tên kho
        string sql = @"
            SELECT 
                p.NgayNhap as Ngay, 
                p.SoHoaDon, 
                p.TongTien, 
                nv.HoTenNV, 
                k.TenKho,
                N'Nhập kho' as Loai
            FROM PhieuNhap p
            JOIN Kho k ON p.KhoId = k.KhoId
            LEFT JOIN NhanVien nv ON p.MaNV = nv.MaNV
            
            UNION ALL
            
            SELECT 
                x.NgayXuat as Ngay, 
                x.SoHoaDon, 
                x.TongTien, 
                nv.HoTenNV, 
                k.TenKho,
                N'Xuất kho' as Loai
            FROM PhieuXuat x
            JOIN Kho k ON x.KhoId = k.KhoId
            LEFT JOIN NhanVien nv ON x.MaNV = nv.MaNV
            
            ORDER BY Ngay DESC";

        SqlCommand cmd = new SqlCommand(sql, conn);
        conn.Open();
        SqlDataReader rdr = cmd.ExecuteReader();
        
        while (rdr.Read())
        {
            danhSach.Add(new BaoCaoTongModel
            {
                ThoiGian = Convert.ToDateTime(rdr["Ngay"]),
                SoHoaDon = rdr["SoHoaDon"].ToString(),
                LoaiPhieu = rdr["Loai"].ToString(),
                NguoiThucHien = rdr["HoTenNV"].ToString(),
                TenKho = rdr["TenKho"].ToString(),
                GiaTri = rdr["TongTien"] != DBNull.Value ? Convert.ToDecimal(rdr["TongTien"]) : 0
            });
        }
    }
    // Trả về trang báo cáo tổng mới
    return View(danhSach);
}

public IActionResult LienHe()
{
    return View();
}
// 2. LỊCH SỬ KIỂM KÊ (Dùng cho View báo cáo)
[HttpGet]
public IActionResult LichSuKiemKe(int id)
{
    // Mặc định lấy kho 1 nếu id không hợp lệ
    if (id <= 0) id = 1;

    ViewBag.CurrentKhoId = id;
    List<LichSuKiemKeVM> danhSach = new List<LichSuKiemKeVM>();

    using (SqlConnection conn = new SqlConnection(GetConnectionString()))
    {
        // SQL dùng LEFT JOIN cho phần Xuất kho để đảm bảo luôn hiện phiếu
 string sql = @"
    -- PHẦN NHẬP KHO (Sửa logic lấy tên khu vực để chống trùng)
    SELECT 
        pn.NgayNhap as Ngay, N'Nhập kho' as Loai, pn.SoHoaDon, 
        sp.TenSP, 
        -- Lấy 1 tên khu vực đại diện để không bị nhân bản dòng
        (SELECT TOP 1 kv.TenKhu 
         FROM OKho o JOIN KhuVuc kv ON o.KhuId = kv.KhuId 
         WHERE o.ViTriId = ctn.ViTriId) as TenKhu, 
        ctn.SoLuong, nv.HoTenNV, pn.TongTien, pn.GhiChu
    FROM PhieuNhap pn
    INNER JOIN ChiTietPhieuNhap ctn ON pn.PhieuNhapID = ctn.PhieuNhapID
    INNER JOIN SanPham sp ON ctn.SanPhamID = sp.SanPhamID
    LEFT JOIN NhanVien nv ON pn.MaNV = nv.MaNV
    WHERE pn.KhoId = @khoId

    UNION ALL

    -- PHẦN XUẤT KHO
    SELECT 
        px.NgayXuat as Ngay, N'Xuất kho' as Loai, px.SoHoaDon, 
        sp.TenSP, 
        (SELECT TOP 1 kv.TenKhu 
         FROM OKho o JOIN KhuVuc kv ON o.KhuId = kv.KhuId 
         WHERE o.ViTriId = ctx.ViTriId) as TenKhu, 
        ctx.SoLuong, nv.HoTenNV, px.TongTien, px.GhiChu
    FROM PhieuXuat px
    INNER JOIN ChiTietPhieuXuat ctx ON px.PhieuXuatID = ctx.PhieuXuatID
    INNER JOIN SanPham sp ON ctx.SanPhamID = sp.SanPhamID
    LEFT JOIN NhanVien nv ON px.MaNV = nv.MaNV
    WHERE px.KhoId = @khoId

    ORDER BY Ngay DESC";

        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@khoId", id);

        conn.Open();
        SqlDataReader rdr = cmd.ExecuteReader();
                        while (rdr.Read())
                {
                    var item = new LichSuKiemKeVM();
                    item.NgayThucHien = rdr["Ngay"] != DBNull.Value ? Convert.ToDateTime(rdr["Ngay"]) : DateTime.Now;
                    item.LoaiPhieu = rdr["Loai"]?.ToString() ?? "";
                    item.SoHoaDon = rdr["SoHoaDon"]?.ToString() ?? "";
                    item.TenSP = rdr["TenSP"]?.ToString() ?? "N/A";
                    
                    // Nếu TenKhu bị NULL thì hiển thị "Chưa gán vị trí"
                    item.TenKhu = rdr["TenKhu"]?.ToString() == "Chưa xác định" ? "Khu vực trống" : rdr["TenKhu"]?.ToString();
                    
                    item.SoLuong = rdr["SoLuong"] != DBNull.Value ? Convert.ToInt32(rdr["SoLuong"]) : 0;
                    item.HoTenNV = rdr["HoTenNV"]?.ToString() ?? "N/A";
                    item.TongTien = rdr["TongTien"] != DBNull.Value ? Convert.ToDecimal(rdr["TongTien"]) : 0;
                    item.GhiChu = rdr["GhiChu"]?.ToString() ?? "";

                    danhSach.Add(item);
                }
    }
    return View(danhSach);
}
[HttpGet]
public JsonResult GetChiTietPhieu(int id, string loai)

{
    var listHang = new List<ChiTietHangHoaVM>();
    string sql = "";

    if (loai == "NHẬP") {
        sql = @"SELECT sp.TenSP, sp.MaSKU, ct.SoLuong, ct.DonGiaNhap as DonGia, v.MaViTri 
                FROM ChiTietPhieuNhap ct 
                JOIN SanPham sp ON ct.SanPhamID = sp.SanPhamID 
                LEFT JOIN OKho v ON ct.ViTriId = v.ViTriId
                WHERE ct.PhieuNhapID = @id";
    } else {
        sql = @"SELECT sp.TenSP, sp.MaSKU, ct.SoLuong, ct.DonGiaXuat as DonGia, v.MaViTri 
                FROM ChiTietPhieuXuat ct 
                JOIN SanPham sp ON ct.SanPhamID = sp.SanPhamID 
                LEFT JOIN OKho v ON ct.ViTriId = v.ViTriId
                WHERE ct.PhieuXuatID = @id";
    }

    using (SqlConnection conn = new SqlConnection(GetConnectionString())) {
        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        conn.Open();
        using (SqlDataReader rdr = cmd.ExecuteReader()) {
            while (rdr.Read()) {
                listHang.Add(new ChiTietHangHoaVM {
                    TenSP = rdr["TenSP"].ToString(),
                    MaSKU = rdr["MaSKU"].ToString(),
                    SoLuong = Convert.ToInt32(rdr["SoLuong"]),
                    DonGia = Convert.ToDecimal(rdr["DonGia"]),
                    MaViTri = rdr["MaViTri"]?.ToString() ?? "N/A"
                });
            }
        }
    }
    return Json(listHang);
}
    }
}