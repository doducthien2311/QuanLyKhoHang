using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuanLyKhoLogistics.Models.Entities;
using Microsoft.Data.SqlClient; // Dùng thư viện mới nhất để hỗ trợ TrustServerCertificate
using System.Data;
using System;

namespace QuanLyKhoLogistics.Controllers
{
    // Cho phép mọi người xem danh sách, nhưng chỉ Admin mới được Thêm/Xóa (Tùy Thiện cấu hình)
    public class NhaCungCapController : Controller
    {
        
      private readonly string _connectionString;
// Hàm khởi tạo này sẽ tự động lấy chuỗi kết nối từ appsettings.json
        public NhaCungCapController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
        // 1. TRANG HIỂN THỊ DANH SÁCH
        public IActionResult Index()
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    // Lấy đúng các cột trong ảnh SQL của Thiện
                    string sql = "SELECT NccID, TenNCC, DiaChi, SoDienThoai FROM NhaCungCap";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tải dữ liệu: " + ex.Message;
            }

            return View(dt); // Trả về DataTable cho file NhaCungCap.cshtml
        }

        // 2. HÀM LƯU NHÀ CUNG CẤP (Dùng cho AJAX)
        [HttpPost]
        public IActionResult LuuNhaCungCap([FromBody] NhaCungCapModel ncc)
        {
            if (ncc == null) return Json(new { success = false, message = "Dữ liệu gửi lên bị trống!" });

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Bỏ Email và MaSoThue vì SQL của Thiện không có
                    string sql = "INSERT INTO NhaCungCap (TenNCC, SoDienThoai, DiaChi) VALUES (@ten, @sdt, @dc)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ten", (object)ncc.TenNCC ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@sdt", (object)ncc.SoDienThoai ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@dc", (object)ncc.DiaChi ?? DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi SQL: " + ex.Message });
            }
        }

        // 3. HÀM XÓA NHÀ CUNG CẤP
        [HttpPost]
        public IActionResult XoaNhaCungCap(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM NhaCungCap WHERE NccID = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }
}