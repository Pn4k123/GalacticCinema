using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyRapController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public QuanLyRapController(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        public IActionResult Index()
        {
            ViewBag.qlr = db.Raps.ToList();
            return View();
        }

        [HttpGet]
        public IActionResult them()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(Rap rap)
        {
            if (rap == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(rap.MaRap))
            {
                ModelState.AddModelError(nameof(rap.MaRap), "Vui lòng chọn rạp.");
            }

            if (string.IsNullOrWhiteSpace(rap.TenRap))
            {
                ModelState.AddModelError(nameof(rap.TenRap), "Vui lòng nhập tên phòng.");
            }

            
            if (!string.IsNullOrWhiteSpace(rap.MaRap) && !string.IsNullOrWhiteSpace(rap.TenRap))
            {
                var exists = db.Raps.Any(p =>
                    p.MaRap == rap.MaRap);

                if (exists)
                {
                    ModelState.AddModelError("", "Rạp cùng tên đã tồn tại trong rạp này.");
                }
            }

            db.Raps.Add(rap);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
