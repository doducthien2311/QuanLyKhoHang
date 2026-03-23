var builder = WebApplication.CreateBuilder(args);

// --- 1. KHU VỰC ĐĂNG KÝ DỊCH VỤ (SERVICES) ---
builder.Services.AddControllersWithViews();

// Thêm dòng này để dùng Session và HttpContextAccessor (để hiện tên người dùng)
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút
});
builder.Services.AddHttpContextAccessor(); 

var app = builder.Build();

// --- 2. KHU VỰC CẤU HÌNH PIPELINE (MIDDLEWARE) ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// QUAN TRỌNG: Dòng UseSession phải đặt trước UseAuthorization
app.UseSession(); 

app.UseAuthorization();

// Cấu hình trang mặc định khi mở web là trang Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();