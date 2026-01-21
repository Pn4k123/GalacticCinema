using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.EntityFrameworkCore;

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
        // Search-enabled Index
        public IActionResult Index(string tuKhoa = "")
        {
            try
            {
                // Load data including navigation so view (or search) can show meaningful values
                var all = db.SuatChieus
                    .Include(s => s.MaPhimNavigation)
                    .Include(s => s.MaPhongNavigation)
                    .ToList();

                List<SuatChieu> danhSach;
                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    danhSach = all;
                }
                else
                {
                    var k = tuKhoa.Trim().ToLower();

                    // Filter in-memory to avoid provider-specific string conversions issues
                    danhSach = all.Where(s =>
                        (s.MaSuatChieu ?? "").ToLower().Contains(k) ||
                        (s.MaPhong ?? "").ToLower().Contains(k) ||
                        (s.MaPhim ?? "").ToLower().Contains(k) ||
                        (s.TrangThai ?? "").ToLower().Contains(k) ||
                        (s.MaPhimNavigation?.TenPhim ?? "").ToLower().Contains(k) ||
                        (s.MaPhongNavigation?.TenPhong ?? "").ToLower().Contains(k) ||
                        // date/time as string
                        s.NgayChieu.ToString("yyyy-MM-dd").ToLower().Contains(k) ||
                        s.GioChieu.ToString().ToLower().Contains(k)
                    ).ToList();
                }

                ViewBag.qlsuatchieu = danhSach;
                ViewBag.TuKhoa = tuKhoa;
                return View();
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.LoiTimKiem = "Lỗi khi tìm kiếm: " + fullError;
                ViewBag.qlsuatchieu = new List<SuatChieu>();
                return View();
            }
        }

        //=============Thêm==================
        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim");
            ViewBag.DSPhong = new SelectList( db.Phongs
                                                .Include(p => p.MaRapNavigation)
                                                .Select(p => new
                                                {
                                                    p.MaPhong,
                                                    TenHienThi = p.TenPhong + " - " + p.MaRapNavigation.TenRap
                                                })
                                                .ToList(),
                                            "MaPhong",
                                            "TenHienThi"
                                        );

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
                if (suatChieu.MaSuatChieu.Length > 10)
                {
                    ModelState.AddModelError(nameof(suatChieu.MaSuatChieu), "Mã suất chiếu không được quá 10 ký tự.");
                }
                else if (suatChieu.NgayChieu < DateOnly.FromDateTime(DateTime.Now))
                {
                    ModelState.AddModelError(nameof(suatChieu.NgayChieu),
                        "Ngày chiếu không được nhỏ hơn ngày hiện tại.");
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


        // ============= Xoá ==================

        // 1. GET: Hiển thị trang xác nhận xóa
        [HttpGet]
        public IActionResult xoa(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Tìm suất chiếu và kèm theo thông tin Phim, Phòng để hiển thị tên cho rõ
            var suatChieu = db.SuatChieus
                .Include(s => s.MaPhimNavigation)
                .Include(s => s.MaPhongNavigation)
                .FirstOrDefault(s => s.MaSuatChieu == id);

            if (suatChieu == null)
            {
                return NotFound();
            }

            return View(suatChieu);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            var suatChieu = db.SuatChieus.Find(id);
            if (suatChieu == null)
            {
                return NotFound();
            }

            try
            {
                db.SuatChieus.Remove(suatChieu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Bắt lỗi ràng buộc (Foreign Key)
                // Ví dụ: Suất chiếu này đã có Vé được bán -> Không xóa được
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                ModelState.AddModelError("", "Không thể xóa suất chiếu này (có thể do đã có vé được đặt). Lỗi: " + fullError);

                // Load lại thông tin phụ để hiển thị lại View đẹp
                var scReload = db.SuatChieus
                    .Include(s => s.MaPhimNavigation)
                    .Include(s => s.MaPhongNavigation)
                    .FirstOrDefault(s => s.MaSuatChieu == id);

                return View(scReload);
            }
        }

        // ============= Sửa ==================

        // 1. GET: Hiển thị giao diện sửa
        [HttpGet]
        public IActionResult sua(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Tìm suất chiếu theo mã
            var suatChieu = db.SuatChieus.Find(id);
            if (suatChieu == null)
            {
                return NotFound();
            }

            // Load danh sách Phim và Phòng để hiển thị Dropdown
            ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu.MaPhim);
            ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu.MaPhong);

            return View(suatChieu);
        }

        // 2. POST: Thực hiện cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(SuatChieu suatChieu)
        {
            // 1. Xóa validation không cần thiết của bảng liên kết
            ModelState.Remove("MaPhimNavigation");
            ModelState.Remove("MaPhongNavigation");
            ModelState.Remove("Ves");

            if (suatChieu == null) return BadRequest();

            // 2. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(suatChieu.MaPhim))
                ModelState.AddModelError(nameof(suatChieu.MaPhim), "Vui lòng chọn phim.");

            if (string.IsNullOrWhiteSpace(suatChieu.MaPhong))
                ModelState.AddModelError(nameof(suatChieu.MaPhong), "Vui lòng chọn phòng.");

            if (suatChieu.NgayChieu == default)
                ModelState.AddModelError(nameof(suatChieu.NgayChieu), "Vui lòng chọn ngày chiếu.");

            if (suatChieu.GioChieu == default)
                ModelState.AddModelError(nameof(suatChieu.GioChieu), "Vui lòng chọn giờ chiếu.");

            if (suatChieu.NgayChieu < DateOnly.FromDateTime(DateTime.Now))
            {
                ModelState.AddModelError(nameof(suatChieu.NgayChieu),
                    "Ngày chiếu không được nhỏ hơn ngày hiện tại.");
            }
            // 3. Logic nghiệp vụ: Kiểm tra trùng lịch chiếu
            // (Quan trọng: Phải loại trừ chính suất chiếu đang sửa ra, dùng && s.MaSuatChieu != suatChieu.MaSuatChieu)
            if (ModelState.IsValid)
            {
                var exists = db.SuatChieus.Any(s =>
                    s.MaPhong == suatChieu.MaPhong
                    && s.NgayChieu == suatChieu.NgayChieu
                    && s.GioChieu == suatChieu.GioChieu
                    && s.MaSuatChieu != suatChieu.MaSuatChieu);

                if (exists)
                {
                    ModelState.AddModelError("", "Đã có suất chiếu khác tại phòng này vào khung giờ này.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu.MaPhim);
                ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu.MaPhong);
                return View(suatChieu);
            }

            try
            {
                // Thực hiện Update
                db.SuatChieus.Update(suatChieu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hiện lỗi chi tiết nếu có trục trặc Database (Ví dụ lỗi check constraint Trạng Thái)
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể cập nhật. Lỗi Database: " + fullError);

                ViewBag.DSPhim = new SelectList(db.Phims.ToList(), "MaPhim", "TenPhim", suatChieu.MaPhim);
                ViewBag.DSPhong = new SelectList(db.Phongs.ToList(), "MaPhong", "TenPhong", suatChieu.MaPhong);
                return View(suatChieu);
            }
        }
    }
}
