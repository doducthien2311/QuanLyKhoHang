using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QuanLyKhoLogistics; // Thay bằng tên project của Thiện
using QuanLyKhoLogistics.Models.Entities;
using QuanLyKhoLogistics.Models.Helpers;
namespace QuanLyKhoLogistics.Controllers // Đổi "Web" thành "Logistics" cho khớp với tên dự án
{   
    public class AccountController : Controller
    {
        public IActionResult Register() => View();

        [HttpPost]

       public IActionResult Register(string FullName, string Username, string Password, string ConfirmPassword)
{// Kiểm tra xem Thiện có nhập thiếu ô nào không
    if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
    {
        ViewBag.Error = "Bạn ơi, bạn nhập thiếu thông tin rồi!";
        return View();
    }

    // Câu lệnh SQL (Đảm bảo tên bảng Users và các cột đúng 100% trong SQL Server)
   string query = "INSERT INTO Users (FullName, Username, Password, Role) VALUES (@name, @user, @pass, 'User')";
    
    SqlParameter[] parameters = {
        new SqlParameter("@name", FullName),
        new SqlParameter("@user", Username),
        new SqlParameter("@pass", Password)
    };

    try {
        bool success = DatabaseHelper.ThucThiLenh(query, parameters);
        if (success) {
            TempData["SuccessMsg"] = "Đăng ký thành công rồi! Đăng nhập thôi.";
            return RedirectToAction("Login");
        }
    } catch (Exception ex) {
        // Nếu lỗi thật sự, nó sẽ hiện ra thông báo lỗi hệ thống ở đây để Thiện sửa
        ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
        return View();
    }

    ViewBag.Error = "Đăng ký không thành công. Thiện kiểm tra lại SQL Server nhé!";
    return View();
}
        [HttpPost]
public IActionResult Login(User model) // Nhận đối tượng User từ View
{
    // Kiểm tra dữ liệu đầu vào
    if (model == null || string.IsNullOrEmpty(model.Username)) {
        ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
        return View();
    }

    string query = "SELECT * FROM Users WHERE Username = @user AND Password = @pass";
    SqlParameter[] parameters = {
        new SqlParameter("@user", model.Username),
        new SqlParameter("@pass", model.Password)
    };

    var dt = DatabaseHelper.LayDuLieu(query, parameters);

    if (dt != null && dt.Rows.Count > 0)
    {
        // Lưu thông tin vào Session để dùng ở các trang khác
        HttpContext.Session.SetString("UserName", dt.Rows[0]["FullName"].ToString());
        HttpContext.Session.SetString("UserRole", dt.Rows[0]["Role"].ToString());

        // Chuyển hướng sang Dashboard
        return RedirectToAction("Index", "Home"); 
    }
    
    ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
    return View();
}


public IActionResult Logout()
{
    HttpContext.Session.Clear(); // Xóa sạch thông tin khi đăng xuất
    return RedirectToAction("Login");
}

        public IActionResult Login() => View();
    }
}