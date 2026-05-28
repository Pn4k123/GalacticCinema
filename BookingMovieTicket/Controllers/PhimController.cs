using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Controllers
{
    public class PhimController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public PhimController(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        public IActionResult Index(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            Phim? p = db.Phims
                .Include(p => p.MaTheLoais)
                .FirstOrDefault(p => p.MaPhim == id);

            if (p == null) return NotFound();

            ViewBag.DSTheLoai = p.MaTheLoais;

            return View(p);
        }
    }
}
