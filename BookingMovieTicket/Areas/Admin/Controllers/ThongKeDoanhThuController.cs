using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ThongKeDoanhThuController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public ThongKeDoanhThuController(QuanLyDatVePhimContext context) {
            db = context;
        }
        public IActionResult Index()
        {
            var data = db.ChiTietDonDatVes
                .Include(ct => ct.MaDonNavigation)
                .Include(ct => ct.MaVeNavigation)
                    .ThenInclude(v => v.MaSuatChieuNavigation)
                        .ThenInclude(s => s.MaPhimNavigation)
                .Include(ct => ct.MaVeNavigation)
                    .ThenInclude(v => v.MaSuatChieuNavigation)
                        .ThenInclude(s => s.MaPhongNavigation)
                            .ThenInclude(p => p.MaRapNavigation)
                // QUAN TRỌNG: Lọc đơn đã thanh toán
                .Where(ct => ct.MaDonNavigation.TrangThai == "Đã thanh toán" || ct.MaDonNavigation.TrangThai == "Thành công")
                .GroupBy(ct => new
                {
                    TenRap = ct.MaVeNavigation.MaSuatChieuNavigation.MaPhongNavigation.MaRapNavigation.TenRap,
                    TenPhim = ct.MaVeNavigation.MaSuatChieuNavigation.MaPhimNavigation.TenPhim
                })
                .Select(g => new ThongKeDoanhThuVM
                {
                    TenRap = g.Key.TenRap,
                    TenPhim = g.Key.TenPhim,
                    SoVeDaBan = g.Count(),
                    TongDoanhThu = g.Sum(x => x.GiaVe)
                })
                .OrderBy(x => x.TenRap)
                .ThenByDescending(x => x.TongDoanhThu)
                .ToList();

            return View(data);
        }
    }
}
