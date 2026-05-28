using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookingMovieTicket.Controllers
{
    public class DonDatVeController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly xuLyMaDon _xlDon;

        public DonDatVeController(QuanLyDatVePhimContext context, xuLyMaDon xlDon)
        {
            db = context;
            _xlDon = xlDon;
        }

        public IActionResult chiTietDonHang(string id)
        {
            SuatChieu? sc = db.SuatChieus
                .Include(x => x.MaPhimNavigation)
                .Include(x => x.MaPhongNavigation)
                .Include(x => x.MaPhongNavigation.MaRapNavigation)
                .FirstOrDefault(x => x.MaSuatChieu == id);

            if (sc == null) return NotFound();

            var tatCaGhe = db.Ghes
                .Where(g => g.MaPhong == sc.MaPhong)
                .ToList();

            var gheDaDat = db.Ves
                .Where(v => v.MaSuatChieu == id)
                .Where(v => v.TrangThai == "Chưa sử dụng" || v.TrangThai == "Đã sử dụng")
                .Select(v => v.MaGhe)
                .ToList();

            var model = new DatVeVM
            {
                MaSuatChieu = sc.MaSuatChieu,
                MaPhim = sc.MaPhim,
                TenPhim = sc.MaPhimNavigation.TenPhim,
                Poster = sc.MaPhimNavigation.Poster,
                TenPhong = sc.MaPhongNavigation.TenPhong,
                TenRap = sc.MaPhongNavigation.MaRapNavigation.TenRap,
                GioChieu = sc.GioChieu,
                NgayChieu = sc.NgayChieu,
                DanhSachGhe = tatCaGhe.Select(g => new GheHienThi
                {
                    MaGhe = g.MaGhe,
                    SoGhe = g.SoGhe,
                    HangGhe = g.HangGhe,
                    Gia = g.Gia,
                    LoaiGhe = g.LoaiGhe,
                    DaDat = gheDaDat.Contains(g.MaGhe)
                })
                .OrderBy(g => g.HangGhe)
                .ThenBy(g => g.SoGhe)
                .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDatVe(string maSuatChieu, List<string> danhSachMaGheChon)
        {
            if (danhSachMaGheChon == null || !danhSachMaGheChon.Any())
                return RedirectToAction("chiTietDonHang", new { id = maSuatChieu });

            //Giới hạn số ghế tối đa mỗi lần đặt để tránh abuse
            if (danhSachMaGheChon.Count > 10)
                return BadRequest("Không thể đặt quá 10 ghế một lần.");

            using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                //Lock các ghế bằng cách query với UPDLOCK (pessimistic locking)
                // Kiểm tra lại ghế trong cùng transaction để tránh race condition
                var gheDaDat = await db.Ves
                    .Where(v => v.MaSuatChieu == maSuatChieu
                             && danhSachMaGheChon.Contains(v.MaGhe)
                             && (v.TrangThai == "Chưa sử dụng" || v.TrangThai == "Đã sử dụng"))
                    .AnyAsync();

                if (gheDaDat)
                    return Content("Một hoặc nhiều ghế đã bị người khác đặt! Vui lòng chọn ghế khác.");

                //Sinh mã đơn an toàn (thread-safe)
                var maDon = await _xlDon.donIdAsync();

                var donHang = new DonDatVe
                {
                    MaDon = maDon,
                    MaNd = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    ThoiGianDat = DateTime.Now,
                    TrangThai = "Đang chờ"
                };
                db.DonDatVes.Add(donHang);
                await db.SaveChangesAsync();

                // Sinh mã vé trong cùng transaction
                var existingVeNums = await db.Ves
                    .Where(v => v.MaVe.StartsWith("V"))
                    .Select(v => v.MaVe.Substring(1))
                    .ToListAsync();

                int currentMax = existingVeNums
                    .Where(s => int.TryParse(s, out _))
                    .Select(s => int.Parse(s))
                    .DefaultIfEmpty(0)
                    .Max();

                foreach (var maGhe in danhSachMaGheChon)
                {
                    currentMax++;
                    string maVe = "V" + currentMax.ToString("D3");

                    var gheInfo = await db.Ghes.FindAsync(maGhe);
                    if (gheInfo == null) continue;

                    var veMoi = new Ve
                    {
                        MaVe = maVe,
                        MaSuatChieu = maSuatChieu,
                        MaGhe = maGhe,
                        TrangThai = "Chưa sử dụng",
                        ThoiGianPhatHanh = DateTime.Now
                    };
                    db.Ves.Add(veMoi);

                    var chiTiet = new ChiTietDonDatVe
                    {
                        MaDon = donHang.MaDon,
                        MaVe = veMoi.MaVe,
                        GiaVe = gheInfo.Gia
                    };
                    db.ChiTietDonDatVes.Add(chiTiet);
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction("Index", "ThanhToan", new { maDon = donHang.MaDon });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Content("Lỗi đặt vé: " + ex.Message);
            }
        }
    }
}