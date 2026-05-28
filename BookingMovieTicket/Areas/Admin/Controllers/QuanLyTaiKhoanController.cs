using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyTaiKhoanController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public QuanLyTaiKhoanController(QuanLyDatVePhimContext context)
        {
            db = context;
        }

        public IActionResult Index()
        {
            ViewBag.qltk = db.NguoiDungs.ToList();
            return View();
        }
    }
}
