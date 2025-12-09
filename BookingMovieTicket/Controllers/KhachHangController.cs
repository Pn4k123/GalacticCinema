using AutoMapper;
using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Humanizer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
                            new Claim(ClaimTypes.NameIdentifier,khachHang.MaNd),
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
            NguoiDung nd = db.NguoiDungs.Find(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return View(nd);
        }

        [Authorize]
        public IActionResult lichSuGiaoDich()
        {
            NguoiDung nd = db.NguoiDungs.Find(User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.NguoiDung = nd;

            var dsDonHang = db.DonDatVes
               .Include(d => d.ChiTietDonDatVes)
                   .ThenInclude(ct => ct.MaVeNavigation)
                       .ThenInclude(v => v.MaGheNavigation)
               .Include(d => d.ChiTietDonDatVes)
                   .ThenInclude(ct => ct.MaVeNavigation)
                       .ThenInclude(v => v.MaSuatChieuNavigation)
                           .ThenInclude(s => s.MaPhimNavigation)
                .Include(d => d.ChiTietDonDatVes)
                   .ThenInclude(ct => ct.MaVeNavigation)
                       .ThenInclude(v => v.MaSuatChieuNavigation)
                           .ThenInclude(s => s.MaPhongNavigation)
                               .ThenInclude(r => r.MaRapNavigation)
               .Where(d => d.MaNd == User.FindFirstValue(ClaimTypes.NameIdentifier))
               .OrderByDescending(d => d.ThoiGianDat)
               .ToList();

            var model =  dsDonHang
                        .Where(d => d.ChiTietDonDatVes != null && d.ChiTietDonDatVes.Any()&&d.TrangThai=="Đã thanh toán")
                        .Select(don => new LichSuDatVeVM
                        {
                            MaDon = don.MaDon,
                            NgayDat = don.ThoiGianDat,
                            TrangThai = don.TrangThai,
                            NgayChieu = don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.NgayChieu,
                            GioChieu = don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.GioChieu,
                            TongTien = don.ChiTietDonDatVes.Sum(ct => ct.GiaVe),

                            TenPhim = don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.TenPhim ?? "Không xác định",
                            Poster = don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.Poster,
                            TenRapvaPhong = don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.MaPhongNavigation.MaRapNavigation.TenRap + " - " +
                             don.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation.MaSuatChieuNavigation.MaPhongNavigation.TenPhong,

                            Ghe = string.Join(", ", don.ChiTietDonDatVes.Select(ct => ct.MaVeNavigation.MaGheNavigation.HangGhe + ct.MaVeNavigation.MaGheNavigation.SoGhe))
                        }).ToList();

            return View(model);
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
