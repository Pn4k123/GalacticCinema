using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    public class QuanLyPhimController : AdminBaseController
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyPhimController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }

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
                ModelState.AddModelError("MaTheLoais", "Vui lòng chọn ít nhất 1 thể loại phim");

            if (phim.ImageFile == null)
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh");

            ModelState.Remove("SuatChieus");
            ModelState.Remove("MaTheLoais");

            if (MaTheLoais != null && MaTheLoais.Any())
            {
                var theLoais = db.TheLoais.Where(tl => MaTheLoais.Contains(tl.MaTheLoai)).ToList();
                phim.MaTheLoais = theLoais;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
                return View(phim);
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

                TempData["SuccessMessage"] = $"Thêm phim '{phim.TenPhim}' thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
            return View(phim);
        }

        [HttpGet]
        public IActionResult xoa(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var phim = db.Phims
                .Include(p => p.MaTheLoais)
                .FirstOrDefault(p => p.MaPhim == id);

            if (phim == null) return NotFound();

            return View(phim);
        }

        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var phim = db.Phims
                .Include(p => p.MaTheLoais)
                .Include(p => p.SuatChieus)
                .FirstOrDefault(p => p.MaPhim == id);

            if (phim == null) return NotFound();

            try
            {
                if (phim.SuatChieus != null && phim.SuatChieus.Any())
                {
                    ModelState.AddModelError("", "Không thể xóa phim này vì đã có suất chiếu liên kết. Vui lòng xóa các suất chiếu trước.");
                    return View(phim);
                }

                if (phim.MaTheLoais != null && phim.MaTheLoais.Any())
                {
                    phim.MaTheLoais.Clear();
                    db.SaveChanges();
                }

                db.Phims.Remove(phim);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(phim.Poster))
                {
                    var posterRelative = phim.Poster.TrimStart('/');
                    var filePath = Path.Combine(env.WebRootPath, posterRelative.Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }

                TempData["SuccessMessage"] = $"Phim '{phim.TenPhim}' đã được xóa thành công!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể xóa phim này do ràng buộc dữ liệu: " + fullError);
                var phimReload = db.Phims.Include(p => p.MaTheLoais).Include(p => p.SuatChieus).FirstOrDefault(p => p.MaPhim == id);
                return View(phimReload);
            }
        }

        [HttpGet]
        public IActionResult sua(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var phim = db.Phims.Include(p => p.MaTheLoais).FirstOrDefault(p => p.MaPhim == id);
            if (phim == null) return NotFound();

            var selectedTheLoais = phim.MaTheLoais.Select(t => t.MaTheLoai).ToList();
            ViewBag.DSTheLoai = new MultiSelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", selectedTheLoais);

            return View(phim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(Phim phim, List<string> MaTheLoais)
        {
            if (phim.ImageFile == null) ModelState.Remove("ImageFile");
            ModelState.Remove("SuatChieus");
            ModelState.Remove("MaTheLoais");

            if (string.IsNullOrWhiteSpace(phim.TenPhim))
                ModelState.AddModelError(nameof(phim.TenPhim), "Vui lòng nhập tên phim.");

            if (MaTheLoais == null || !MaTheLoais.Any())
                ModelState.AddModelError("MaTheLoais", "Vui lòng chọn ít nhất 1 thể loại phim.");

            if (ModelState.IsValid)
            {
                var exists = db.Phims.Any(p => p.TenPhim == phim.TenPhim && p.MaPhim != phim.MaPhim);
                if (exists)
                    ModelState.AddModelError(nameof(phim.TenPhim), "Tên phim này đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSTheLoai = new MultiSelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
                return View(phim);
            }

            try
            {
                var existingPhim = db.Phims.Include(p => p.MaTheLoais).FirstOrDefault(p => p.MaPhim == phim.MaPhim);
                if (existingPhim == null) return NotFound();

                existingPhim.TenPhim = phim.TenPhim;
                existingPhim.MoTa = phim.MoTa;
                existingPhim.ThoiLuong = phim.ThoiLuong;
                existingPhim.DaoDien = phim.DaoDien;
                existingPhim.DanhGia = phim.DanhGia;
                existingPhim.NgayPhatHanh = phim.NgayPhatHanh;
                existingPhim.Trailer = phim.Trailer;

                existingPhim.MaTheLoais.Clear();
                if (MaTheLoais != null)
                {
                    var newTheLoais = db.TheLoais.Where(tl => MaTheLoais.Contains(tl.MaTheLoai)).ToList();
                    foreach (var tl in newTheLoais)
                        existingPhim.MaTheLoais.Add(tl);
                }

                if (phim.ImageFile != null)
                {
                    if (!string.IsNullOrEmpty(existingPhim.Poster))
                    {
                        string oldPath = Path.Combine(env.WebRootPath, existingPhim.Poster.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                    }

                    string uploadFolder = Path.Combine(env.WebRootPath, "upload");
                    if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                    string extension = Path.GetExtension(phim.ImageFile.FileName);
                    string uniqueFileName = Guid.NewGuid().ToString() + phim.MaPhim + extension;
                    string filePath = Path.Combine(uploadFolder, uniqueFileName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                        phim.ImageFile.CopyTo(fs);

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