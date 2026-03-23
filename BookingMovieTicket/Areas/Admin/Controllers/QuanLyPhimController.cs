using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyPhimController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyPhimController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }

        // Search-enabled Index
        public IActionResult Index(string tuKhoa = "")
        {
            try
            {
                List<Phim> danhSach;

                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    danhSach = db.Phims.ToList();
                }
                else
                {
                    var k = tuKhoa.Trim().ToLower();
                    // Tìm kiếm theo Mã, Tên, Đạo diễn hoặc Đánh giá
                    danhSach = db.Phims
                        .Where(p =>
                            ((p.MaPhim ?? "").ToLower().Contains(k)) ||
                            ((p.TenPhim ?? "").ToLower().Contains(k)) ||
                            ((p.DaoDien ?? "").ToLower().Contains(k)) ||
                            ((p.DanhGia ?? "").ToLower().Contains(k))
                        ).ToList();
                }

                ViewBag.qlp = danhSach;
                ViewBag.TuKhoa = tuKhoa;
                return View();
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.LoiTimKiem = "Lỗi khi tìm kiếm: " + fullError;
                ViewBag.qlp = new List<Phim>();
                return View();
            }
        }

        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(Phim phim, List<string> MaTheLoais)
        {
            if (MaTheLoais == null || !MaTheLoais.Any())
            {
                ModelState.AddModelError("MaTheLoais", "Vui lòng chọn ít nhất 1 thể loại phim");
            }

            if (phim.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh");
            }

            if (MaTheLoais != null && MaTheLoais.Any())
            {
                var theLoais = db.TheLoais.Where(tl => MaTheLoais.Contains(tl.MaTheLoai)).ToList();
                phim.MaTheLoais = theLoais;
            }

            if (phim.ImageFile != null)
            {
                string uploadFolder = Path.Combine(env.WebRootPath, "upload");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                string extension = Path.GetExtension(phim.ImageFile.FileName);

                string uniqueFileName = Guid.NewGuid().ToString() + phim.MaPhim + extension;

                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    phim.ImageFile.CopyTo(fs);
                }

                phim.Poster = "/upload/" + uniqueFileName;

                db.Phims.Add(phim);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
            return View(phim);
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

            // Tìm phim theo mã kèm theo thể loại để hiển thị
            var phim = db.Phims
                .Include(p => p.MaTheLoais)
                .FirstOrDefault(p => p.MaPhim == id);

            if (phim == null)
            {
                return NotFound();
            }

            return View(phim);
        }

        // 2. POST: Thực hiện xóa
        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            // Load phim kèm theo các collection liên quan để kiểm tra ràng buộc và xóa relationship trước
            var phim = db.Phims
                .Include(p => p.MaTheLoais)
                .Include(p => p.SuatChieus)
                .FirstOrDefault(p => p.MaPhim == id);

            if (phim == null)
                return NotFound();

            try
            {
                // Nếu đã có suất chiếu liên kết -> không cho xóa
                if (phim.SuatChieus != null && phim.SuatChieus.Any())
                {
                    ModelState.AddModelError("", "Không thể xóa phim này vì đã có suất chiếu liên kết. Vui lòng xóa các suất chiếu trước.");
                    return View(phim);
                }

                // Nếu có mối quan hệ nhiều-nhiều với Thể loại, xóa liên kết trong bảng join trước
                if (phim.MaTheLoais != null && phim.MaTheLoais.Any())
                {
                    phim.MaTheLoais.Clear();
                    db.SaveChanges(); // remove join rows
                }

                // Thực hiện xóa bản ghi phim
                db.Phims.Remove(phim);
                db.SaveChanges();

                // Xóa file poster SAU khi xóa DB thành công (tránh trạng thái nửa chừng)
                if (!string.IsNullOrEmpty(phim.Poster))
                {
                    var posterRelative = phim.Poster.TrimStart('/');
                    var filePath = Path.Combine(env.WebRootPath, posterRelative.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                TempData["SuccessMessage"] = $"Phim '{phim.TenPhim}' đã được xóa thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể xóa phim này do ràng buộc dữ liệu: " + fullError);

                // Reload để đảm bảo View hiển thị đủ thông tin
                var phimReload = db.Phims
                    .Include(p => p.MaTheLoais)
                    .Include(p => p.SuatChieus)
                    .FirstOrDefault(p => p.MaPhim == id);

                return View(phimReload);
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi khi xóa phim: " + fullError);

                var phimReload = db.Phims
                    .Include(p => p.MaTheLoais)
                    .Include(p => p.SuatChieus)
                    .FirstOrDefault(p => p.MaPhim == id);

                return View(phimReload);
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

            // Tìm phim và nạp danh sách thể loại hiện có
            var phim = db.Phims
                .Include(p => p.MaTheLoais)
                .FirstOrDefault(p => p.MaPhim == id);

            if (phim == null)
            {
                return NotFound();
            }

            // Tạo MultiSelectList để hiển thị dropdown chọn nhiều thể loại, và pre-select các thể loại của phim
            var selectedTheLoais = phim.MaTheLoais.Select(t => t.MaTheLoai).ToList();
            ViewBag.DSTheLoai = new MultiSelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", selectedTheLoais);

            return View(phim);
        }

        // 2. POST: Thực hiện cập nhật
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(Phim phim, List<string> MaTheLoais)
        {
            // 1. Xóa validation không cần thiết
            // ImageFile không bắt buộc khi sửa (nếu null thì giữ ảnh cũ)
            if (phim.ImageFile == null)
            {
                ModelState.Remove("ImageFile");
            }
            ModelState.Remove("SuatChieus");
            ModelState.Remove("MaTheLoais"); // Sẽ xử lý thủ công

            if (phim == null) return BadRequest();

            // 2. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(phim.TenPhim))
                ModelState.AddModelError(nameof(phim.TenPhim), "Vui lòng nhập tên phim.");

            if (MaTheLoais == null || !MaTheLoais.Any())
                ModelState.AddModelError("MaTheLoais", "Vui lòng chọn ít nhất 1 thể loại phim.");

            // 3. Logic nghiệp vụ: Kiểm tra trùng tên phim (trừ chính nó ra)
            if (ModelState.IsValid)
            {
                var exists = db.Phims.Any(p => p.TenPhim == phim.TenPhim && p.MaPhim != phim.MaPhim);
                if (exists)
                {
                    ModelState.AddModelError(nameof(phim.TenPhim), "Tên phim này đã tồn tại. Vui lòng chọn tên khác.");
                }
            }

            if (!ModelState.IsValid)
            {
                // Load lại danh sách thể loại nếu lỗi
                ViewBag.DSTheLoai = new MultiSelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
                return View(phim);
            }

            try
            {
                // Lấy phim gốc từ DB để cập nhật (bao gồm cả quan hệ TheLoai)
                var existingPhim = db.Phims
                    .Include(p => p.MaTheLoais)
                    .FirstOrDefault(p => p.MaPhim == phim.MaPhim);

                if (existingPhim == null) return NotFound();

                // Cập nhật thông tin cơ bản
                existingPhim.TenPhim = phim.TenPhim;
                existingPhim.MoTa = phim.MoTa;
                existingPhim.ThoiLuong = phim.ThoiLuong;
                existingPhim.DaoDien = phim.DaoDien;
                existingPhim.DanhGia = phim.DanhGia;
                existingPhim.NgayPhatHanh = phim.NgayPhatHanh;
                existingPhim.Trailer = phim.Trailer;

                // Cập nhật Thể loại (Xóa cũ, thêm mới)
                existingPhim.MaTheLoais.Clear();
                if (MaTheLoais != null)
                {
                    var newTheLoais = db.TheLoais.Where(tl => MaTheLoais.Contains(tl.MaTheLoai)).ToList();
                    foreach (var tl in newTheLoais)
                    {
                        existingPhim.MaTheLoais.Add(tl);
                    }
                }

                // Xử lý Upload ảnh mới (nếu có)
                if (phim.ImageFile != null)
                {
                    // 1. Xóa ảnh cũ
                    if (!string.IsNullOrEmpty(existingPhim.Poster))
                    {
                        string oldPath = Path.Combine(env.WebRootPath, existingPhim.Poster.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    // 2. Lưu ảnh mới
                    string uploadFolder = Path.Combine(env.WebRootPath, "upload");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string extension = Path.GetExtension(phim.ImageFile.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + phim.MaPhim + extension;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        phim.ImageFile.CopyTo(fs);
                    }

                    existingPhim.Poster = "/upload/" + uniqueFileName;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = $"Cập nhật phim '{phim.TenPhim}' thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể cập nhật. Lỗi Database: " + fullError);

                ViewBag.DSTheLoai = new MultiSelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
                return View(phim);
            }
        }
    }
}

