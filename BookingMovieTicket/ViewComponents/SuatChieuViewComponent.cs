using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookingMovieTicket.ViewComponents
{
    public class SuatChieuViewComponent : ViewComponent
    {
        private readonly QuanLyDatVePhimContext db;

        public SuatChieuViewComponent(QuanLyDatVePhimContext context) => db = context;

        public IViewComponentResult Invoke(string maphim, string maRap, DateOnly ngay)
        {
            if (ngay == default)
                return View(Enumerable.Empty<SuatChieuVM>());

            var date = ngay.ToDateTime(new TimeOnly(0, 0));

            var rapPhongIds = db.Phongs
                .Where(p => p.MaRap == maRap)
                .Select(p => p.MaPhong);

            var list = db.SuatChieus
    .Where(sc =>
        rapPhongIds.Contains(sc.MaPhong) &&
        sc.MaPhim == maphim &&
        sc.NgayChieu == DateOnly.FromDateTime(date))
    .Select(sc => new
    {
        sc.MaSuatChieu,
        sc.MaPhong,
        sc.MaPhim,
        sc.NgayChieu,
        sc.GioChieu,
        sc.MaPhongNavigation
    })
    .ToList(); // load nhẹ

            var data = list
                .GroupBy(x => x.GioChieu)
                .Select(g => g.First())
                .OrderBy(x => x.GioChieu)
                .Select(x => new SuatChieuVM { MaSuatChieu=x.MaSuatChieu,MaPhong = x.MaPhong,MaPhim=x.MaPhim,NgayChieu=x.NgayChieu,GioChieu=x.GioChieu })
                .ToList();

            return View(data);
        }


    }
}
