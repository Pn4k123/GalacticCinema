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
            // 1. Xóa validation không cần thiết
            ModelState.Remove("MaPhimNavigation");
            ModelState.Remove("MaPhongNavigation");
            ModelState.Remove("Ves");

            if (suatChieu == null) return BadRequest();

            // 2. KIỂM TRA MÃ SUẤT CHIẾU
            if (string.IsNullOrWhiteSpace(suatChieu.MaSuatChieu))
            {
                ModelState.AddModelError(nameof(suatChieu.MaSuatChieu), "Vui lòng nhập mã suất chiếu.");
            }
            else
            {
                // Check độ dài: Database chỉ cho phép tối đa 10 ký tự
                if (suatChieu.MaSuatChieu.Length > 10)
                {
                    ModelState.AddModelError(nameof(suatChieu.MaSuatChieu), "Mã suất chiếu không được quá 10 ký tự.");
                }
                else if (db.SuatChieus.Any(s => s.MaSuatChieu == suatChieu.MaSuatChieu))
                {
                    ModelState.AddModelError(nameof(suatChieu.MaSuatChieu), "Mã suất chiếu đã tồn tại.");
                }
            }

            if (string.IsNullOrWhiteSpace(suatChieu.MaPhim))
                ModelState.AddModelError(nameof(suatChieu.MaPhim), "Vui lòng chọn phim.");

            if (string.IsNullOrWhiteSpace(suatChieu.MaPhong))
                ModelState.AddModelError(nameof(suatChieu.MaPhong), "Vui lòng chọn phòng.");

            if (suatChieu.NgayChieu == default)
                ModelState.AddModelError(nameof(suatChieu.NgayChieu), "Vui lòng chọn ngày chiếu.");

            if (suatChieu.GioChieu == default)
                ModelState.AddModelError(nameof(suatChieu.GioChieu), "Vui lòng chọn giờ chiếu.");

            if (ModelState.IsValid)
            {
                var exists = db.SuatChieus.Any(s =>
                    s.MaPhong == suatChieu.MaPhong
                    && s.NgayChieu == suatChieu.NgayChieu
                    && s.GioChieu == suatChieu.GioChieu);

                if (exists)
                    ModelState.AddModelError("", "Đã có suất chiếu tại phòng này vào giờ này.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu.MaPhim);
                ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu.MaPhong);
                return View(suatChieu);
            }

            try
            {
                db.SuatChieus.Add(suatChieu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // HIỆN CHI TIẾT LỖI RA MÀN HÌNH ĐỂ BẠN ĐỌC
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi lưu Database: " + fullError);

                ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu.MaPhim);
                ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu.MaPhong);
                return View(suatChieu);
            }
        }
    }
}
