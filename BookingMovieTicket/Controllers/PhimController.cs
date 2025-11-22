using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Controllers
{
    public class PhimController : Controller
    {
        QuanLyDatVePhimContext db = new QuanLyDatVePhimContext();
        public IActionResult Index(string id)
        {
            Phim p = db.Phims.Include(p=>p.MaTheLoais).FirstOrDefault(p => p.MaPhim == id);

            ViewBag.DSTheLoai = p.MaTheLoais;
            return View(p);
        }
    }
}
