using Microsoft.AspNetCore.Authentication.Cookies;
// Thay dòng dưới đây bằng namespace chính xác của file ApplicationContext.cs bạn vừa tạo
using SV22T1020247.Shop;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình MVC
builder.Services.AddControllersWithViews();

// 2. Cấu hình HttpContextAccessor (Bắt buộc để lớp ApplicationContext truy cập được Session)
builder.Services.AddHttpContextAccessor();

// 3. Cấu hình Authentication (Xác thực người dùng) - CHỈ CẤU HÌNH 1 LẦN DUY NHẤT
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "WebBanHang_Customer_Cookie"; // Tên cookie lưu trên trình duyệt
        options.LoginPath = "/Account/Login";               // Đường dẫn đá về khi chưa đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn đá về khi không đủ quyền
        options.ExpireTimeSpan = TimeSpan.FromDays(30);     // Giữ đăng nhập 30 ngày
    });

// 4. Cấu hình Session (Lưu trữ giỏ hàng)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// 5. KHỞI TẠO CẤU HÌNH HỆ THỐNG (Kết nối Database)
// Lấy chuỗi kết nối từ appsettings.json
string connectionString = builder.Configuration.GetConnectionString("SV22T1020247")
    ?? throw new InvalidOperationException("ConnectionString 'SV22T1020247' not found.");

// Khởi tạo các lớp Context và DataService
ApplicationContext.Initialize(connectionString);
SV22T1020247.BusinessLayers.Configuration.Initialize(connectionString);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// THỨ TỰ MIDDLEWARE LÀ CỰC KỲ QUAN TRỌNG
app.UseStaticFiles();
app.UseRouting();

app.UseSession();        // Gọi Session trước hoặc sau Routing đều được, nhưng phải TRƯỚC Auth

app.UseAuthentication(); // 1. Mày là ai? (Đăng nhập)
app.UseAuthorization();  // 2. Mày được làm gì? (Phân quyền)

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();