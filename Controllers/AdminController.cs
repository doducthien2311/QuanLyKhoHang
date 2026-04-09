using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Data;
using Microsoft.Data.SqlClient;
using QuanLyKhoLogistics.Models.Helpers;

namespace QuanLyKhoLogistics.Controllers
{
    public class AdminController : Controller
    {
        // Trang danh sách nhân viên
        public IActionResult QuanLyNhanVien()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            string query = "SELECT UserId, FullName, Username, Role FROM Users";
            DataTable dt = DatabaseHelper.LayDuLieu(query);

            // THAY ĐỔI Ở ĐÂY: Chỉ định đường dẫn tuyệt đối để không bao giờ lạc đường
            return View("~/Views/Admin/QuanLyNhanVien.cshtml", dt);
        }

        // Hàm xử lý đổi quyền qua AJAX
        [HttpPost]
        public IActionResult CapNhatQuyen(int userId, string newRole)
        {
            var currentRole = HttpContext.Session.GetString("UserRole");
            if (currentRole != "Admin") return Json(new { success = false, message = "Bạn không có quyền này!" });

            string query = "UPDATE Users SET Role = @role WHERE UserId = @id";
            SqlParameter[] parameters = {
                new SqlParameter("@role", newRole),
                new SqlParameter("@id", userId)
            };

            bool result = DatabaseHelper.ThucThiLenh(query, parameters);
            return Json(new { success = result });
        }
    }
}