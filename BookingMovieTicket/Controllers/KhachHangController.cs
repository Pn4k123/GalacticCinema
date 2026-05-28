using AutoMapper;
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
    public class KhachHangController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IMapper _mapper;
        private readonly xuLyMaKH _xl;

        public KhachHangController(QuanLyDatVePhimContext context, IMapper mapper, xuLyMaKH xl)
        {
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
                // Kiểm tra email đã tồn tại trước khi tạo
                if (db.NguoiDungs.Any(nd => nd.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                    return PartialView("_DangKy", model);
                }

                if (db.NguoiDungs.Any(nd => nd.Sdt == model.Sdt))
                {
                    ModelState.AddModelError("Sdt", "Số điện thoại này đã được đăng ký.");
                    return PartialView("_DangKy", model);
                }

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
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
                }
            }
            return PartialView("_DangKy", model);
        }

        [HttpGet]
        public IActionResult DangNhap(string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            return PartialView("_DangNhap", new DangNhapVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapVM model, string? ReturnUrl)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            if (ModelState.IsValid)
            {
                var khachHang = db.NguoiDungs.SingleOrDefault(kh => kh.Email == model.Email);
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại");
                }
                else if (khachHang.MatKhau != model.MatKhau.ToMd5Hash(khachHang.RandomKey))
                {
                    ModelState.AddModelError("MatKhau", "Sai thông tin đăng nhập");
                }
                else
                {
                    var claims = new List<Claim> {
                        new Claim(ClaimTypes.Email, khachHang.Email),
                        new Claim(ClaimTypes.Name, khachHang.HoTen),
                        new Claim(ClaimTypes.NameIdentifier, khachHang.MaNd),
                        new Claim(ClaimTypes.Role, "KhachHang")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    await HttpContext.SignInAsync(claimsPrincipal);

                    // Chỉ lưu thông tin không nhạy cảm vào Session
                    // Không lưu MatKhau, RandomKey
                    var sessionData = new
                    {
                        MaNd = khachHang.MaNd,
                        HoTen = khachHang.HoTen,
                        Email = khachHang.Email
                    };
                    HttpContext.Session.SetString("NguoiDung", JsonConvert.SerializeObject(sessionData));

                    // Validate ReturnUrl để tránh open redirect attack
                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                        return Json(new { success = true, redirectUrl = ReturnUrl });

                    return Json(new { success = true, redirectUrl = "/" });
                }
            }
            return PartialView("_DangNhap", model);
        }

        [Authorize]
        public IActionResult khachHangProfile()
        {
            var maNd = User.FindFirstValue(ClaimTypes.NameIdentifier);
            NguoiDung? nd = db.NguoiDungs.Find(maNd);
            if (nd == null) return NotFound();
            return View(nd);
        }

        [Authorize]
        public IActionResult lichSuGiaoDich(int trang = 1)
        {
            var maNd = User.FindFirstValue(ClaimTypes.NameIdentifier);
            NguoiDung? nd = db.NguoiDungs.Find(maNd);
            if (nd == null) return NotFound();

            ViewBag.NguoiDung = nd;

            int soTrangHienThi = 5; 
            int skip = (trang - 1) * soTrangHienThi;

            var query = db.DonDatVes
                .Where(d => d.MaNd == maNd
                         && d.ChiTietDonDatVes.Any()
                         && d.TrangThai == "Đã thanh toán")
                .OrderByDescending(d => d.ThoiGianDat);

            int tongSoBanGhi = query.Count();
            int tongSoTrang = (int)Math.Ceiling((double)tongSoBanGhi / soTrangHienThi);

            var model = query
                .Skip(skip)
                .Take(soTrangHienThi)
                .Select(don => new LichSuDatVeVM
                {
                    MaDon = don.MaDon,
                    NgayDat = don.ThoiGianDat,
                    TrangThai = don.TrangThai,
                    NgayChieu = don.ChiTietDonDatVes
                        .Select(ct => ct.MaVeNavigation.MaSuatChieuNavigation.NgayChieu)
                        .FirstOrDefault(),
                    GioChieu = don.ChiTietDonDatVes
                        .Select(ct => ct.MaVeNavigation.MaSuatChieuNavigation.GioChieu)
                        .FirstOrDefault(),
                    TongTien = don.ChiTietDonDatVes.Sum(ct => ct.GiaVe),
                    TenPhim = don.ChiTietDonDatVes
                        .Select(ct => ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.TenPhim)
                        .FirstOrDefault(),
                    Poster = don.ChiTietDonDatVes
                        .Select(ct => ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.Poster)
                        .FirstOrDefault(),
                    Ghe = string.Join(", ", don.ChiTietDonDatVes
                        .Select(ct => ct.MaVeNavigation.MaGheNavigation.HangGhe
                                    + ct.MaVeNavigation.MaGheNavigation.SoGhe))
                })
                .ToList();

            ViewBag.TrangHienTai = trang;
            ViewBag.TongSoTrang = tongSoTrang;

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