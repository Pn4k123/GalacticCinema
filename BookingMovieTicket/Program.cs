using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QuanLyDatVePhimContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("MovieData")));

builder.Services.AddSingleton<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(builder.Environment);

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

builder.Services.AddScoped<xuLyMaKH>();

builder.Services.AddScoped<xuLyMaVe>();

builder.Services.AddScoped<xuLyMaDon>();

builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(10);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/KhachHang/DangNhap";
    options.AccessDeniedPath = "/AccessDenied";
});

builder.Services.AddHostedService<BookingMovieTicket.Services.VeAutoCleanerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}"
    );


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
