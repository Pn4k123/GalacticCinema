using AutoMapper;
using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookingMovieTicket.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IMapper _mapper;
        private readonly xuLyMaKH _xl;

        public KhachHangController(QuanLyDatVePhimContext context,IMapper mapper ,xuLyMaKH xl) {
            db = context;
            _mapper = mapper;
            _xl = xl;
        }
        [HttpGet]
        public IActionResult DangKy()
        {
            return PartialView("_DangKy", new DangKyVM());
        }

        [HttpPost]
        public IActionResult DangKy(DangKyVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = _mapper.Map<NguoiDung>(model);
                    khachHang.RandomKey = MyUtil.GenerateRandomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(khachHang.RandomKey);
                    khachHang.MaNd = _xl.khachHangId();
                    khachHang.VaiTro = "KhachHang";

                    db.NguoiDungs.Add(khachHang);
                    db.SaveChanges();
                    
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {

                }
            }
            return PartialView("_DangKy",model);
        }

        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return PartialView("_DangNhap", new DangNhapVM());
        }

        [HttpPost]
        public async Task<IActionResult> DangNhap(DangNhapVM model,string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.NguoiDungs.SingleOrDefault(kh => kh.Email == model.Email);
                if (khachHang == null) {
                    ModelState.AddModelError("","Tài khoản không tồn tại");
                }
                else
                {
                    if(khachHang.MatKhau != model.MatKhau.ToMd5Hash(khachHang.RandomKey)){
                        ModelState.AddModelError("MatKhau", "Sai thông tin đăng nhập");
                    }
                    else
                    {
                        var claims = new List<Claim> {
                            new Claim(ClaimTypes.Email, khachHang.Email),
                            new Claim(ClaimTypes.Name, khachHang.HoTen),

                            new Claim(ClaimTypes.Role, "KhachHang")
                        };

                        var claimsIdentity = new ClaimsIdentity(claims,"login");

                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(claimsPrincipal);

                        HttpContext.Session.SetString("NguoiDung", JsonConvert.SerializeObject(khachHang));


                        return Json(new { success = true, redirectUrl = ReturnUrl ?? "/" });


                    }
                }
            }
            return PartialView("_DangNhap", model);
        }

        [Authorize]
        public IActionResult khachHangProfile()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync();
            HttpContext.Session.Remove("NguoiDung");

            return Redirect("/Home/Index");
        }
    }
}
