using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookingTicket.ViewComponents
{
    public class PhimViewComponent : ViewComponent
    {
        private readonly QuanLyDatVePhimContext db;

        public PhimViewComponent(QuanLyDatVePhimContext context) => db = context;

        public IViewComponentResult Invoke(string trangthai)
        {
            var query = db.Phims.AsQueryable();

            if (trangthai == "dangchieu")
            {
                query = query.Where(p => p.NgayPhatHanh <= DateTime.Now);
            }
            else if (trangthai == "sapchieu")
            {
                query = query.Where(p => p.NgayPhatHanh > DateTime.Now);
            }

            var data = query.Select(phim => new PhimVM
            {
                MaPhim = phim.MaPhim,
                TenPhim = phim.TenPhim,
                Poster = phim.Poster,
                Trailer = phim.Trailer,
                NgayPhatHanh = phim.NgayPhatHanh
            }).OrderByDescending(p=>p.NgayPhatHanh);

            return View(data);
        }
    }
}
