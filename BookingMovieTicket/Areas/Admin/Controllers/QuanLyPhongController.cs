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
        // Search-enabled Index
        public IActionResult Index(string tuKhoa = "")
        {
            try
            {
                List<Phong> danhSach;

                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    danhSach = db.Phongs.ToList();
                }
                else
                {
                    var k = tuKhoa.Trim().ToLower();
                    danhSach = db.Phongs
                        .Where(p =>
                            ((p.MaPhong ?? "").ToLower().Contains(k)) ||
                            ((p.MaRap ?? "").ToLower().Contains(k)) ||
                            ((p.TenPhong ?? "").ToLower().Contains(k)) ||
                            ((p.TrangThai ?? "").ToLower().Contains(k))
                        ).ToList();
                }

                ViewBag.qlphong = danhSach;
                ViewBag.TuKhoa = tuKhoa;
                return View();
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.LoiTimKiem = "Lỗi khi tìm kiếm: " + fullError;
                ViewBag.qlphong = new List<Phong>();
                return View();
            }
        }

        //=============Thêm==================
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
            // 1. Xóa validation của các bảng liên kết để tránh lỗi ảo
            ModelState.Remove("MaRapNavigation");
            ModelState.Remove("Ghes");
            ModelState.Remove("SuatChieus");

            if (phong == null) return BadRequest();

            // 2. KIỂM TRA MÃ PHÒNG (Nhập tay)
            if (string.IsNullOrWhiteSpace(phong.MaPhong))
            {
                ModelState.AddModelError(nameof(phong.MaPhong), "Vui lòng nhập mã phòng.");
            }
            else
            {
                // Check độ dài: Database quy định tối đa 10 ký tự
                if (phong.MaPhong.Length > 10)
                {
                    ModelState.AddModelError(nameof(phong.MaPhong), "Mã phòng không được quá 10 ký tự.");
                }
                // Check trùng mã
                else if (db.Phongs.Any(p => p.MaPhong == phong.MaPhong))
                {
                    ModelState.AddModelError(nameof(phong.MaPhong), "Mã phòng này đã tồn tại.");
                }
            }

            // Validate các trường khác
            if (string.IsNullOrWhiteSpace(phong.MaRap))
                ModelState.AddModelError(nameof(phong.MaRap), "Vui lòng chọn rạp.");

            if (string.IsNullOrWhiteSpace(phong.TenPhong))
                ModelState.AddModelError(nameof(phong.TenPhong), "Vui lòng nhập tên phòng.");

            if (string.IsNullOrWhiteSpace(phong.TrangThai))
                ModelState.AddModelError(nameof(phong.TrangThai), "Vui lòng chọn trạng thái.");

            // 3. Logic nghiệp vụ: Check trùng Tên Phòng trong cùng 1 Rạp
            if (ModelState.IsValid)
            {
                var exists = db.Phongs.Any(p =>
                    p.MaRap == phong.MaRap &&
                    p.TenPhong.Trim().ToLower() == phong.TenPhong.Trim().ToLower()
                    && p.MaPhong != phong.MaPhong); // (Optional) trừ chính nó ra nếu update, nhưng đây là thêm mới nên ko cần cũng được

                if (exists)
                {
                    ModelState.AddModelError("", "Tên phòng này đã tồn tại trong rạp đã chọn.");
                }
            }

            // 4. Nếu có lỗi thì trả về View
            if (!ModelState.IsValid)
            {
                ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);
                return View(phong);
            }

            // 5. Thử lưu vào Database
            try
            {
                db.Phongs.Add(phong);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // HIỆN CHI TIẾT LỖI (Quan trọng để biết tại sao sai)
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi lưu Database: " + fullError);

                ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);
                return View(phong);
            }
        }

        // ============= Xoá ==================

        // 1. GET: Hiển thị trang xác nhận xóa phòng
        [HttpGet]
        public IActionResult xoa(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // Tìm phòng theo mã
            var phong = db.Phongs.FirstOrDefault(p => p.MaPhong == id);

            if (phong == null)
            {
                return NotFound();
            }

            return View(phong);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            var phong = db.Phongs.Find(id);
            if (phong == null)
            {
                return NotFound();
            }

            try
            {
                db.Phongs.Remove(phong);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Bắt lỗi ràng buộc (Foreign Key)
                // Ví dụ: Phòng này đang có ghế ngồi hoặc đang có suất chiếu -> Không xóa được
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                ModelState.AddModelError("", "Không thể xóa phòng này. Lỗi Database: " + fullError);

                // Trả về view cũ để hiện lỗi
                return View(phong);
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

            // Tìm phòng theo mã
            var phong = db.Phongs.Find(id);
            if (phong == null)
            {
                return NotFound();
            }

            // Load danh sách Rạp để hiển thị Dropdown
            ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);

            return View(phong);
        }

        // 2. POST: Thực hiện cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(Phong phong)
        {
            // 1. Xóa validation không cần thiết của bảng liên kết
            ModelState.Remove("MaRapNavigation");
            ModelState.Remove("Ghes");
            ModelState.Remove("SuatChieus");

            if (phong == null) return BadRequest();

            // 2. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(phong.TenPhong))
                ModelState.AddModelError(nameof(phong.TenPhong), "Vui lòng nhập tên phòng.");

            if (string.IsNullOrWhiteSpace(phong.MaRap))
                ModelState.AddModelError(nameof(phong.MaRap), "Vui lòng chọn rạp.");

            if (string.IsNullOrWhiteSpace(phong.TrangThai))
                ModelState.AddModelError(nameof(phong.TrangThai), "Vui lòng chọn trạng thái.");

            // 3. Logic nghiệp vụ: Kiểm tra trùng Tên Phòng trong cùng 1 Rạp
            // (Quan trọng: Phải loại trừ chính phòng đang sửa ra, dùng && p.MaPhong != phong.MaPhong)
            if (ModelState.IsValid)
            {
                var exists = db.Phongs.Any(p =>
                    p.MaRap == phong.MaRap &&
                    p.TenPhong.Trim().ToLower() == phong.TenPhong.Trim().ToLower() &&
                    p.MaPhong != phong.MaPhong);

                if (exists)
                {
                    ModelState.AddModelError("", "Tên phòng này đã tồn tại trong rạp đã chọn. Vui lòng đặt tên khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);
                return View(phong);
            }

            try
            {
                // Thực hiện Update
                db.Phongs.Update(phong);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hiện lỗi chi tiết nếu có trục trặc Database
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể cập nhật. Lỗi Database: " + fullError);

                ViewBag.DSRap = new SelectList(db.Raps.ToList(), "MaRap", "TenRap", phong.MaRap);
                return View(phong);
            }
        }
    }
}
