using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyTaiKhoanController : Controller
    {
        QuanLyDatVePhimContext db = new QuanLyDatVePhimContext();
        public IActionResult Index()
        {
            ViewBag.qltk = db.NguoiDungs.ToList();
            return View();
        }
    }
}
