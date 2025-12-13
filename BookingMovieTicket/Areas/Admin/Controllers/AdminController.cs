using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BookingMovieTicket.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public AdminController(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(DangNhapVM model)
        {
            if (ModelState.IsValid)
            {
                var admin = db.NguoiDungs.SingleOrDefault(kh => kh.Email == model.Email);
                if (admin == null)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại");
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
                            new Claim(ClaimTypes.NameIdentifier,admin.MaNd),
                            new Claim(ClaimTypes.Role, "Admin")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "login");

                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);

                        HttpContext.Session.SetString("NguoiDung", JsonConvert.SerializeObject(admin));

                        return RedirectToAction("Index");
                    }
                }
            }
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Remove("NguoiDung");

            return Redirect("/Admin");
        }
    }
}
