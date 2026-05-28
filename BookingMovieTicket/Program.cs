using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// SERVICES
// ==========================================
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<BookingMovieTicket.Helper.LayoutDataFilter>();
});


// DbContext chỉ đăng ký một lần qua DI
builder.Services.AddDbContext<QuanLyDatVePhimContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("MovieData")));

builder.Services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(builder.Environment);

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddScoped<xuLyMaKH>();
builder.Services.AddScoped<xuLyMaDon>();
builder.Services.AddScoped<xuLyMaVe>(); 

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30); 
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
    option.Cookie.SecurePolicy = CookieSecurePolicy.Always; //Chỉ gửi cookie qua HTTPS
});

// Tách biệt scheme đăng nhập cho Admin và KhachHang
// Để Admin và KhachHang không dùng chung cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/KhachHang/DangNhap";
        options.AccessDeniedPath = "/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

builder.Services.AddHostedService<VeAutoCleanerService>();
builder.Services.AddSingleton<IVnPayService, VnPayService>();
builder.Services.AddScoped<IZaloPayService, ZaloPayService>();

// ==========================================
// PIPELINE
// ==========================================
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); 
}

app.UseHttpsRedirection(); 
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Admin}/{action=Login}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();