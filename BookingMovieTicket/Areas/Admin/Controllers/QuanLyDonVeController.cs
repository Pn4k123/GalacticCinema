using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyDonVeController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyDonVeController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }
        public IActionResult Index(string tuKhoa = "")
        {
            try
            {
                List<DonDatVe> danhSach;

                if (string.IsNullOrWhiteSpace(tuKhoa))
                {
                    // Nếu không có từ khóa, hiển thị tất cả
                    danhSach = db.DonDatVes.ToList();
                }
                else
                {
                    // Tìm kiếm theo từ khóa (tìm trong MaDon, MaNd, TrangThai)
                    tuKhoa = tuKhoa.Trim().ToLower();
                    danhSach = db.DonDatVes
                        .Where(d => d.MaDon.ToLower().Contains(tuKhoa) ||
                                    d.MaNd.ToLower().Contains(tuKhoa) ||
                                    d.TrangThai.ToLower().Contains(tuKhoa))
                        .ToList();
                }

                ViewBag.qldonve = danhSach;
                ViewBag.TuKhoa = tuKhoa;

                return View();
            }
            catch (Exception ex)
            {
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ViewBag.LuoiTimKiem = "Lỗi khi tìm kiếm: " + fullError;
                ViewBag.qldonve = new List<DonDatVe>();
                return View();
            }
        }


        //=============Thêm==================
        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen");
            return View(new DonDatVe());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(DonDatVe donDatVe)
        {
            // 1. Xóa các lỗi validation của bảng liên kết (Rất quan trọng)
            ModelState.Remove("MaNdNavigation");
            ModelState.Remove("ChiTietDonDatVes");
            ModelState.Remove("ThanhToan"); // Quan hệ 1-1 hoặc 1-n, cần remove để ko báo lỗi null

            if (donDatVe == null) return BadRequest();

            // 2. KIỂM TRA MÃ ĐƠN (Nhập tay)
            if (string.IsNullOrWhiteSpace(donDatVe.MaDon))
            {
                ModelState.AddModelError(nameof(donDatVe.MaDon), "Vui lòng nhập mã đơn.");
            }
            else
            {
                // Check độ dài: Database quy định tối đa 10 ký tự
                if (donDatVe.MaDon.Length > 10)
                {
                    ModelState.AddModelError(nameof(donDatVe.MaDon), "Mã đơn không được quá 10 ký tự.");
                }
                // Check trùng mã
                else if (db.DonDatVes.Any(d => d.MaDon == donDatVe.MaDon))
                {
                    ModelState.AddModelError(nameof(donDatVe.MaDon), "Mã đơn này đã tồn tại.");
                }
            }

            // Validate các trường khác
            if (string.IsNullOrWhiteSpace(donDatVe.MaNd))
                ModelState.AddModelError(nameof(donDatVe.MaNd), "Vui lòng chọn Người dùng.");

            if (donDatVe.ThoiGianDat == default)
                ModelState.AddModelError(nameof(donDatVe.ThoiGianDat), "Vui lòng chọn Thời gian đặt.");

            if (string.IsNullOrWhiteSpace(donDatVe.TrangThai))
                ModelState.AddModelError(nameof(donDatVe.TrangThai), "Vui lòng chọn Trạng thái.");

            // 3. Logic nghiệp vụ (Optional): Kiểm tra trùng đơn hàng của cùng 1 người tại cùng 1 thời điểm
            if (ModelState.IsValid)
            {
                var exists = db.DonDatVes.Any(s =>
                    s.MaNd == donDatVe.MaNd
                    && s.ThoiGianDat == donDatVe.ThoiGianDat);

                if (exists)
                    ModelState.AddModelError("", "Người dùng này đã có đơn hàng vào thời gian này.");
            }

            // 4. Trả về View nếu có lỗi
            if (!ModelState.IsValid)
            {
                ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen", donDatVe.MaNd);
                return View(donDatVe);
            }

            // 5. Thử lưu vào Database
            try
            {
                db.DonDatVes.Add(donDatVe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // HIỆN CHI TIẾT LỖI RA MÀN HÌNH
                // Lỗi này sẽ giúp bạn biết ngay nếu sai quy định về "Trạng thái"
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Lỗi lưu Database: " + fullError);

                ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen", donDatVe.MaNd);
                return View(donDatVe);
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

            // Tìm đơn vé theo mã
            var donDatVe = db.DonDatVes.FirstOrDefault(m => m.MaDon == id);

            if (donDatVe == null)
            {
                return NotFound();
            }

            // Trả về View để người dùng xem thông tin trước khi quyết định xóa
            return View(donDatVe);
        }

        // 2. POST: Thực hiện xóa khi người dùng nhấn nút "Xóa"
        [HttpPost, ActionName("xoa")]
        [ValidateAntiForgeryToken]
        public IActionResult xoaDaXacNhan(string id)
        {
            var donDatVe = db.DonDatVes.Find(id);
            if (donDatVe == null)
            {
                return NotFound();
            }

            try
            {
                db.DonDatVes.Remove(donDatVe);
                db.SaveChanges();
                // Xóa thành công thì quay về trang chủ
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Bắt lỗi database (ràng buộc khóa ngoại)
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // Thêm lỗi vào ModelState để hiển thị ra View
                ModelState.AddModelError("", "Không thể xóa đơn vé này vì đã thanh toán!");

                // Trả về lại View xác nhận xóa kèm thông báo lỗi
                return View(donDatVe);
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

            // Tìm đơn vé theo mã
            var donDatVe = db.DonDatVes.Find(id);
            if (donDatVe == null)
            {
                return NotFound();
            }

            // Load danh sách người dùng vào ViewBag để hiển thị Dropdown
            ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen", donDatVe.MaNd);

            return View(donDatVe);
        }

        // 2. POST: Thực hiện lưu thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult sua(DonDatVe donDatVe)
        {
            // 1. Xóa validation không cần thiết của bảng liên kết
            ModelState.Remove("MaNdNavigation");
            ModelState.Remove("ChiTietDonDatVes");
            ModelState.Remove("ThanhToan");

            if (donDatVe == null) return BadRequest();

            // 2. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(donDatVe.MaNd))
                ModelState.AddModelError(nameof(donDatVe.MaNd), "Vui lòng chọn Người dùng.");

            if (donDatVe.ThoiGianDat == default)
                ModelState.AddModelError(nameof(donDatVe.ThoiGianDat), "Vui lòng chọn Thời gian đặt.");

            if (string.IsNullOrWhiteSpace(donDatVe.TrangThai))
                ModelState.AddModelError(nameof(donDatVe.TrangThai), "Vui lòng chọn Trạng thái.");

            // 3. Logic nghiệp vụ: Kiểm tra trùng lặp
            // (Kiểm tra xem người này có đơn hàng nào trùng giờ này không, NHƯNG trừ chính đơn hàng đang sửa ra)
            if (ModelState.IsValid)
            {
                var exists = db.DonDatVes.Any(d =>
                    d.MaNd == donDatVe.MaNd
                    && d.ThoiGianDat == donDatVe.ThoiGianDat
                    && d.MaDon != donDatVe.MaDon); // Quan trọng: Trừ chính nó ra

                if (exists)
                    ModelState.AddModelError("", "Người dùng này đã có đơn hàng khác vào thời gian này.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen", donDatVe.MaNd);
                return View(donDatVe);
            }

            try
            {
                // Thực hiện Update
                db.DonDatVes.Update(donDatVe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Hiện lỗi chi tiết (ví dụ lỗi sai Trạng thái quy định trong DB)
                var fullError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Không thể cập nhật. Lỗi Database: " + fullError);

                ViewBag.DSND = new SelectList(db.NguoiDungs.ToList(), "MaNd", "HoTen", donDatVe.MaNd);
                return View(donDatVe);
            }
        }

    }
}
