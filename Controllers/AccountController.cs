using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyKhoLogistics.Models.Entities;
using QuanLyKhoLogistics.Models.Helpers;
using System.Data;

namespace QuanLyKhoLogistics.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(string FullName, string Username, string Password)
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewBag.Error = "Thiện ơi, nhập thiếu thông tin rồi!";
                return View();
            }

            // 1. TẠO MÃ NHÂN VIÊN TỰ ĐỘNG (Ví dụ: NV + ticks thời gian để không trùng)
            string tuSinhMaNV = "NV" + DateTime.Now.Ticks.ToString().Substring(10);

            // 2. DÙNG TRANSACTION ĐỂ ĐẢM BẢO LƯU CẢ 2 BẢNG HOẶC KHÔNG LƯU GÌ CẢ
            // Bước 2.1: Chèn vào bảng NhanVien trước
            string queryNV = "INSERT INTO NhanVien (MaNV, HoTenNV) VALUES (@ma, @name)";
            SqlParameter[] paramNV = {
                new SqlParameter("@ma", tuSinhMaNV),
                new SqlParameter("@name", FullName)
            };

            // Bước 2.2: Chèn vào bảng Users sau (có kèm MaNV vừa tạo)
            string queryUser = "INSERT INTO Users (FullName, Username, Password, Role, MaNV) VALUES (@name, @user, @pass, 'User', @ma)";
            SqlParameter[] paramUser = {
                new SqlParameter("@name", FullName),
                new SqlParameter("@user", Username),
                new SqlParameter("@pass", Password),
                new SqlParameter("@ma", tuSinhMaNV)
            };

            try
            {
                // Thực thi chèn vào bảng Nhân viên
                DatabaseHelper.ThucThiLenh(queryNV, paramNV);
                // Thực thi chèn vào bảng Users
                bool success = DatabaseHelper.ThucThiLenh(queryUser, paramUser);

                if (success)
                {
                    TempData["SuccessMsg"] = "Đăng ký thành công! Mã nhân viên của bạn là: " + tuSinhMaNV;
                    return RedirectToAction("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                return View();
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(Users model) // Đổi thành Users (số nhiều) cho khớp file bạn vừa tạo
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            // Truy vấn lấy cả MaNV
            string query = "SELECT UserId, FullName, Role, MaNV FROM Users WHERE Username = @user AND Password = @pass";
            SqlParameter[] parameters = {
                new SqlParameter("@user", model.Username),
                new SqlParameter("@pass", model.Password)
            };

            DataTable dt = DatabaseHelper.LayDuLieu(query, parameters);

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];

                // Lưu thông tin vào Session
                HttpContext.Session.SetString("UserId", row["UserId"].ToString());
                HttpContext.Session.SetString("UserName", row["FullName"].ToString());
                HttpContext.Session.SetString("UserRole", row["Role"].ToString());
                
                // QUAN TRỌNG: Lấy MaNV để sau này dùng cho Phiếu Nhập
                string maNV = row["MaNV"] != DBNull.Value ? row["MaNV"].ToString() : "";
                HttpContext.Session.SetString("MaNV", maNV);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();
    }
}