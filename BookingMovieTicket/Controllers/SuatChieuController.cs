using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

namespace BookingMovieTicket.Controllers
{
    public class SuatChieuController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public SuatChieuController(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public IActionResult loadSuatChieu(DateOnly ngay ,string maPhim,string maRap)
        {
            var now = DateTime.Now;
            var scUpdate = db.SuatChieus
                             .Where(s => s.NgayChieu <= ngay && s.TrangThai != "Đã chiếu")
                             .ToList();
            foreach (var sc in scUpdate)
            {
                if (sc.NgayChieu.ToDateTime(sc.GioChieu) < now)
                    sc.TrangThai = "Đã chiếu";
            }
            db.SaveChanges();

            var suatChieu = db.SuatChieus
                            .Include(s => s.MaPhongNavigation)
                            .Where(s => s.MaPhim == maPhim && s.NgayChieu == ngay && s.MaPhongNavigation.MaRap == maRap && s.TrangThai == "Sắp chiếu")
                            .OrderBy(s => s.GioChieu)
                            .Distinct()
                            .ToList();

            var session = HttpContext.Session.GetString("NguoiDung");
            ViewBag.DaDangNhap = (session != null);

            return PartialView("_DanhSachSuatChieu", suatChieu);
        }
    }
}
