using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyPhongController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyPhongController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }
        public IActionResult Index()
        {
            ViewBag.qlphong = db.Phongs.ToList();
            return View();
        }

        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(Phong phong)
        {
            if (phong == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(phong.MaRap))
            {
                ModelState.AddModelError(nameof(phong.MaRap), "Vui lòng chọn rạp.");
            }

            if (string.IsNullOrWhiteSpace(phong.TenPhong))
            {
                ModelState.AddModelError(nameof(phong.TenPhong), "Vui lòng nhập tên phòng.");
            }

            // Optional: validate TrangThai if required in your domain
            // if (string.IsNullOrWhiteSpace(phong.TrangThai))
            // {
            //     ModelState.AddModelError(nameof(phong.TrangThai), "Vui lòng nhập trạng thái phòng.");
            // }

            // Prevent duplicate room name within same Rap
            if (!string.IsNullOrWhiteSpace(phong.MaRap) && !string.IsNullOrWhiteSpace(phong.TenPhong))
            {
                var exists = db.Phongs.Any(p =>
                    p.MaRap == phong.MaRap &&
                    p.TenPhong.Trim().ToLower() == phong.TenPhong.Trim().ToLower());

                if (exists)
                {
                    ModelState.AddModelError("", "Phòng cùng tên đã tồn tại trong rạp này.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);
                return View(phong);
            }

            // Generate unique MaPhong (max length 10 in DB)
            phong.MaPhong = "P" + Guid.NewGuid().ToString("N").Substring(0, 9);

            db.Phongs.Add(phong);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
