using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BookingMovieTicket.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public AdminController(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            var model = new AdminDashboardVM
            {
                TongPhim = db.Phims.Count(),
                TongDoanhThu = db.ChiTietDonDatVes.Any() ? db.ChiTietDonDatVes.Sum(ct => ct.GiaVe) : 0,
                TongKH = db.NguoiDungs.Count(k => k.MaNd.StartsWith("KH")),
                TongVeBan = db.DonDatVes.Count(d => d.TrangThai == "Đã thanh toán"),
                TopPhims = db.ChiTietDonDatVes
                    .GroupBy(ct => new { 
                        ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.MaPhim,
                        ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.TenPhim,
                        ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.Poster
                    })
                    .Select(g => new TopPhimVM
                    {
                        TenPhim = g.Key.TenPhim,
                        DoanhThu = g.Sum(x => x.GiaVe),
                        Hinh = g.Key.Poster
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .Take(5)
                    .ToList(),
                DonHangMoi = db.DonDatVes
                    .Include(d => d.ChiTietDonDatVes)
                    .Include(d => d.MaNdNavigation)
                    .OrderByDescending(d => d.ThoiGianDat)
                    .Take(5)
                    .Select(d => new DonHangMoiVM
                    {
                        MaDon = d.MaDon,
                        TenND = d.MaNdNavigation.HoTen,
                        ThoiGianDat = d.ThoiGianDat,
                        TongTien = d.ChiTietDonDatVes != null ? d.ChiTietDonDatVes.Sum(ct => ct.GiaVe) : 0,
                        TrangThai = d.TrangThai
                    })
                    .ToList()
            };

            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(DangNhapVM model)
        {
            if (ModelState.IsValid)
            {
                var admin = db.NguoiDungs.SingleOrDefault(kh => kh.Email == model.Email);

                if (admin == null || admin.VaiTro != "Admin")
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại hoặc không có quyền truy cập");
                }
                else
                {
                    if (admin.MatKhau != model.MatKhau.ToMd5Hash(admin.RandomKey))
                    {
                        ModelState.AddModelError("MatKhau", "Sai thông tin đăng nhập");
                    }
                    else
                    {
                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Email, admin.Email),
                            new Claim(ClaimTypes.Name, admin.HoTen),
                            new Claim(ClaimTypes.NameIdentifier, admin.MaNd),
                            new Claim(ClaimTypes.Role, "Admin")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "login");
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);
                        HttpContext.Session.SetString("AdminName", admin.HoTen);
                        HttpContext.Session.SetString("AdminId", admin.MaNd);

                        return RedirectToAction("Index");
                    }
                }
            }
            return View();
        }

        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Remove("AdminName");
            HttpContext.Session.Remove("AdminId");

            return Redirect("/Admin");
        }
    }
}