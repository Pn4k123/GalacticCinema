using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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

        // Search-enabled Index
        public IActionResult Index(string tuKhoa = "")
        {
            try
            {
                List<Rap> danhSach;

                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    danhSach = db.Raps.ToList();
                }
                else
                {
                    var k = tuKhoa.Trim().ToLower();
                    // Tìm kiếm theo Mã, Tên, Địa chỉ hoặc Trạng thái
                    danhSach = db.Raps
                        .Where(r =>
                            ((r.MaRap ?? "").ToLower().Contains(k)) ||
                            ((r.TenRap ?? "").ToLower().Contains(k)) ||
                            ((r.DiaChi ?? "").ToLower().Contains(k)) ||
                            ((r.TrangThai ?? "").ToLower().Contains(k))
                        ).ToList();
                }

                ViewBag.qlr = danhSach;
                ViewBag.TuKhoa = tuKhoa;
                return View();
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.LoiTimKiem = "Lỗi khi tìm kiếm: " + fullError;
                ViewBag.qlr = new List<Rap>();
                return View();
            }
        }

        // ============= Thêm ==================
        [HttpGet]
        public IActionResult them()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(Rap rap)
        {
            if (rap == null) return BadRequest();

            // 1. Validate dữ liệu
            if (string.IsNullOrWhiteSpace(rap.MaRap))
                ModelState.AddModelError(nameof(rap.MaRap), "Vui lòng nhập mã rạp.");

            if (string.IsNullOrWhiteSpace(rap.TenRap))
                ModelState.AddModelError(nameof(rap.TenRap), "Vui lòng nhập tên rạp.");

            if (string.IsNullOrWhiteSpace(rap.TrangThai))
                ModelState.AddModelError(nameof(rap.TrangThai), "Vui lòng chọn trạng thái.");

            // 2. Kiểm tra trùng mã
            if (!string.IsNullOrWhiteSpace(rap.MaRap) && db.Raps.Any(p => p.MaRap == rap.MaRap))
            {
                ModelState.AddModelError(nameof(rap.MaRap), "Mã rạp này đã tồn tại.");
            }

            if (!ModelState.IsValid) return View(rap);

            try
            {
                db.Raps.Add(rap);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Thêm rạp mới thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // Bắt lỗi Check Constraint cụ thể để báo người dùng dễ hiểu hơn
                if (fullError.Contains("CHECK constraint"))
                {
                    ModelState.AddModelError("TrangThai", "Trạng thái không hợp lệ. Vui lòng chọn 'Còn hoạt động' hoặc 'Bảo trì'.");
                }
                else
                {
                    ModelState.AddModelError("", "Lỗi lưu Database: " + fullError);
                }
                return View(rap);
            }
        }

        // ============= Xoá ==================

        // 1. GET: Hiển thị trang xác nhận xóa
        [HttpGet]
        public IActionResult xoa(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            // Load Rạp kèm danh sách Phòng để hiển thị cảnh báo
            var rap = db.Raps
                .Include(r => r.Phongs)
                .FirstOrDefault(r => r.MaRap == id);

            if (rap == null) return NotFound();

            return View(rap);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            var rap = db.Raps
                .Include(r => r.Phongs) // Load Phongs để check ràng buộc
                .FirstOrDefault(r => r.MaRap == id);

            if (rap == null) return NotFound();

            try
            {
                // Kiểm tra ràng buộc: Nếu Rạp còn Phòng -> Không cho xóa
                if (rap.Phongs != null && rap.Phongs.Any())
                {
                    ModelState.AddModelError("", $"Không thể xóa rạp này vì đang có {rap.Phongs.Count} phòng hoạt động. Vui lòng xóa các phòng trước.");
                    return View(rap);
                }

                db.Raps.Remove(rap);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Rạp '{rap.TenRap}' đã được xóa thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể xóa rạp này do ràng buộc dữ liệu: " + fullError);
                return View(rap);
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi khi xóa rạp: " + fullError);
                return View(rap);
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

            var rap = db.Raps.Find(id);
            if (rap == null)
            {
                return NotFound();
            }

            return View(rap);
        }

        // 2. POST: Thực hiện cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(Rap rap)
        {
            // 1. Xóa validation không cần thiết của bảng liên kết
            ModelState.Remove("Phongs");

            if (rap == null) return BadRequest();

            // 2. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(rap.TenRap))
                ModelState.AddModelError(nameof(rap.TenRap), "Vui lòng nhập tên rạp.");

            if (string.IsNullOrWhiteSpace(rap.TrangThai))
                ModelState.AddModelError(nameof(rap.TrangThai), "Vui lòng chọn trạng thái.");

            // 3. Logic nghiệp vụ: Kiểm tra trùng tên rạp (trừ chính nó ra)
            if (ModelState.IsValid)
            {
                var exists = db.Raps.Any(r => r.TenRap == rap.TenRap && r.MaRap != rap.MaRap);
                if (exists)
                {
                    ModelState.AddModelError(nameof(rap.TenRap), "Tên rạp này đã tồn tại. Vui lòng chọn tên khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(rap);
            }

            try
            {
                db.Raps.Update(rap);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"Cập nhật rạp '{rap.TenRap}' thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // Bắt lỗi Check Constraint cụ thể
                if (fullError.Contains("CHECK constraint"))
                {
                    ModelState.AddModelError("TrangThai", "Trạng thái không hợp lệ. Vui lòng chọn 'Còn hoạt động' hoặc 'Bảo trì'.");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể cập nhật. Lỗi Database: " + fullError);
                }

                return View(rap);
            }
        }
    }
}
