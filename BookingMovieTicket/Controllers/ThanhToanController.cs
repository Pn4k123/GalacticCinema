using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly QuanLyDatVePhimContext db;

        public ThanhToanController(QuanLyDatVePhimContext context) {
            db = context;
        }
        public IActionResult Index(string maDon)
        {
            // 1. Lấy thông tin đơn hàng kèm các bảng liên quan
            var donHang = db.DonDatVes
                .Include(d => d.ChiTietDonDatVes)
                    .ThenInclude(ct => ct.MaVeNavigation)
                        .ThenInclude(v => v.MaGheNavigation)
                .Include(d => d.ChiTietDonDatVes)
                    .ThenInclude(ct => ct.MaVeNavigation)
                        .ThenInclude(v => v.MaSuatChieuNavigation)
                            .ThenInclude(s => s.MaPhimNavigation)
                 .Include(d => d.ChiTietDonDatVes)
                    .ThenInclude(ct => ct.MaVeNavigation)
                        .ThenInclude(v => v.MaSuatChieuNavigation)
                            .ThenInclude(s => s.MaPhongNavigation)
                                .ThenInclude(r => r.MaRapNavigation)
                .FirstOrDefault(d => d.MaDon == maDon);

            if (donHang == null) return NotFound();

            // 2. Lấy thông tin phim từ vé đầu tiên (vì các vé cùng 1 phim)
            var veDauTien = donHang.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation;
            if (veDauTien == null) return RedirectToAction("Index", "Home");

            // 3. Tổng hợp danh sách ghế (Ví dụ: "A1, B2")
            var listGhe = donHang.ChiTietDonDatVes
                .Select(ct => ct.MaVeNavigation.MaGheNavigation.HangGhe + ct.MaVeNavigation.MaGheNavigation.SoGhe)
                .ToList();

            // 4. Map sang ViewModel
            var model = new ThanhToanVM
            {
                MaDon = donHang.MaDon,
                TenPhim = veDauTien.MaSuatChieuNavigation.MaPhimNavigation.TenPhim,
                TenPhong = veDauTien.MaSuatChieuNavigation.MaPhongNavigation.TenPhong,
                TenRap = veDauTien.MaSuatChieuNavigation.MaPhongNavigation.MaRapNavigation.TenRap,
                SuatChieu = $"{veDauTien.MaSuatChieuNavigation.GioChieu} - {veDauTien.MaSuatChieuNavigation.NgayChieu:dd/MM/yyyy}",
                SoLuongVe = listGhe.Count,
                GheDaChon = string.Join(", ", listGhe),
                TongTien = donHang.ChiTietDonDatVes.Sum(ct => ct.GiaVe)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ProcessPayment(string maDon, string phuongThuc)
        {
            // 1. Tìm đơn hàng
            var donHang = db.DonDatVes
                .Include(d => d.ChiTietDonDatVes) // Load chi tiết để kiểm tra vé
                .FirstOrDefault(d => d.MaDon == maDon);

            if (donHang == null)
            {
                return NotFound();
            }

            if (donHang.TrangThai != "Đang chờ")
            {
                // Trả về view báo lỗi hoặc thông báo cho người dùng
                ViewBag.Message = "Giao dịch thất bại! Đơn hàng đã bị hủy do quá thời gian thanh toán hoặc đã được xử lý.";
                return View("PaymentError"); 
            }

            //Kiểm tra xem Vé trong bảng Ve còn tồn tại không?
            // Vì Background Service đã xóa vé để nhả ghế, nên ta phải check lại.
            var danhSachMaVe = donHang.ChiTietDonDatVes.Select(ct => ct.MaVe).ToList();
            var soLuongVeTrongDb = db.Ves.Count(v => danhSachMaVe.Contains(v.MaVe));

            if (soLuongVeTrongDb != donHang.ChiTietDonDatVes.Count)
            {
                // Trường hợp hiếm: Đơn chưa đổi trạng thái nhưng vé đã bị xóa
                donHang.TrangThai = "Đã hủy";
                db.SaveChanges();

                ViewBag.Message = "Giao dịch thất bại! Ghế ngồi đã bị giải phóng do quá hạn.";
                return View("PaymentError");
            }

            // --- NẾU VƯỢT QUA TẤT CẢ CÁC CHECK TRÊN THÌ MỚI THU TIỀN ---

            // 4. Cập nhật trạng thái thành công
            donHang.TrangThai = "Đã thanh toán";

            // 5. Lưu lịch sử thanh toán
            var thanhToan = new ThanhToan
            {
                MaDon = maDon,
                PhuongThuc = phuongThuc,
                ThoiGian = DateTime.Now,
                TrangThai = "Thành công"
            };
            db.ThanhToans.Add(thanhToan);

            db.SaveChanges();

            return RedirectToAction("Index","Home");
        }
    }
}
