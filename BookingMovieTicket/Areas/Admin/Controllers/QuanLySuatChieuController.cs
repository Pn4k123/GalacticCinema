using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLySuatChieuController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLySuatChieuController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }
        public IActionResult Index()
        {
            ViewBag.qlsuatchieu = db.SuatChieus.ToList();
            return View();
        }

        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim");
            ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(SuatChieu suatChieu)
        {
            if (suatChieu == null)
                return BadRequest();

            if (string.IsNullOrWhiteSpace(suatChieu.MaPhim))
                ModelState.AddModelError(nameof(suatChieu.MaPhim), "Vui lòng chọn phim.");

            if (string.IsNullOrWhiteSpace(suatChieu.MaPhong))
                ModelState.AddModelError(nameof(suatChieu.MaPhong), "Vui lòng chọn phòng.");

            if (suatChieu.NgayChieu == default)
                ModelState.AddModelError(nameof(suatChieu.NgayChieu), "Vui lòng chọn ngày chiếu.");

            if (suatChieu.GioChieu == default)
                ModelState.AddModelError(nameof(suatChieu.GioChieu), "Vui lòng chọn giờ chiếu.");

            // check duplicate exact time in same room
            if (ModelState.IsValid)
            {
                var exists = db.SuatChieus.Any(s =>
                    s.MaPhong == suatChieu.MaPhong
                    && s.NgayChieu == suatChieu.NgayChieu
                    && s.GioChieu == suatChieu.GioChieu);

                if (exists)
                    ModelState.AddModelError(string.Empty, "Đã tồn tại suất chiếu cùng ngày/giờ trong phòng này.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu?.MaPhim);
                ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu?.MaPhong);
                return View(suatChieu);
            }

            // generate MaSuatChieu (keeps length reasonable)
            suatChieu.MaSuatChieu = "SC" + Guid.NewGuid().ToString("N").Substring(0, 8);

            db.SuatChieus.Add(suatChieu);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
