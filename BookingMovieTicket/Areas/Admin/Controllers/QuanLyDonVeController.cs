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
        public IActionResult Index()
        {
            ViewBag.qldonve = db.DonDatVes.ToList();
            return View();
        }

        

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
    }
}
