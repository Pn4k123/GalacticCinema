using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace BookingMovieTicket.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public HomeController(QuanLyDatVePhimContext context)
        {
           db = context;
        }

        public IActionResult Index()
        {
            ViewBag.tenPhim = new Phim();
            ViewBag.tenRap = new Rap();
            ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim");
            ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap");
            return View();
        }

   

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
