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
    }
}
