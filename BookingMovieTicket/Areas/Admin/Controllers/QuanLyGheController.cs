using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyGheController : Controller
    {

        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyGheController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }
        public IActionResult Index()
        {
            ViewBag.qlghe = db.Ghes.ToList();
            return View();
        }


    }
}
