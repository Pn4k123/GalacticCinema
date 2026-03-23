using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.Services;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BookingMovieTicket.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IVnPayService _vnPayService;
        private readonly IZaloPayService _zaloPayService;
        private readonly IConfiguration _config;
        private readonly IServiceScopeFactory _scopeFactory;

        public ThanhToanController(QuanLyDatVePhimContext context, IVnPayService vnPayService,IZaloPayService zaloPayService,IConfiguration configuration,IServiceScopeFactory serviceScopeFactory) {
            db = context;
            _vnPayService = vnPayService;
            _zaloPayService = zaloPayService;
            _config = configuration;
            _scopeFactory = serviceScopeFactory;
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
        public async Task<IActionResult> ProcessPaymentAsync(string maDon, string phuongThuc)
        {
            var donHang = db.DonDatVes.Include(d => d.ChiTietDonDatVes).FirstOrDefault(d => d.MaDon == maDon);

            if (donHang == null) return NotFound();
            if (donHang.TrangThai != "Đang chờ")
            {
                ViewBag.Message = "Giao dịch thất bại! Đơn hàng đã hết hạn hoặc đã được xử lý.";
                return View("PaymentError");
            }

            // Kiểm tra vé tồn tại (Quan trọng)
            var danhSachMaVe = donHang.ChiTietDonDatVes.Select(ct => ct.MaVe).ToList();
            var soLuongVeTrongDb = db.Ves.Count(v => danhSachMaVe.Contains(v.MaVe));
            if (soLuongVeTrongDb != donHang.ChiTietDonDatVes.Count)
            {
                donHang.TrangThai = "Đã hủy";
                db.SaveChanges();
                ViewBag.Message = "Giao dịch thất bại! Ghế ngồi đã bị giải phóng.";
                return View("PaymentError");
            }

            // 2. Phân chia luồng xử lý theo phương thức
            if (phuongThuc == "VNPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    donHang = donHang
                    // Lưu ý: VnPayService của bạn cần set ReturnUrl trỏ về action "PaymentCallback"
                };
                string paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnPayModel);
                return Redirect(paymentUrl);
            }
            else if (phuongThuc == "ZaloPay")
            {

                // 2. Gọi Service tạo đơn (Async)
                var result = await _zaloPayService.CreateOrderAsync(donHang);

                if (result != null && result.ReturnCode == 1)
                {
                    return Redirect(result.OrderUrl); // Chuyển sang trang thanh toán Zalo
                }
                else
                {
                    var errorMsg = result == null ? "Không nhận được phản hồi từ ZaloPay" :
                       $"{result.ReturnMessage} (SubCode: {result.SubReturnCode} - {result.SubReturnMessage})";

                    ViewBag.Message = $"Lỗi ZaloPay: {errorMsg}";
                    return View("PaymentError");
                }
            }
            else
            {
                // Thanh toán trực tiếp
                XuLyThanhToanThanhCong(maDon, phuongThuc);
                return RedirectToAction("PaymentSuccess");
            }
        }

        public IActionResult PaymentCallback()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || !response.Success)
            {
                ViewBag.Message = $"Lỗi thanh toán VNPay: {response?.VnPayResponseCode}";
                return View("PaymentError");
            }

            // Xử lý khi thành công (00)
            if (response.VnPayResponseCode == "00")
            {
                // VNPay trả về OrderId chính là MaDon nên dùng trực tiếp được
                bool ketQua = XuLyThanhToanThanhCong(response.OrderId, "VNPay");
                if (ketQua) return RedirectToAction("PaymentSuccess");
            }

            ViewBag.Message = $"Giao dịch thất bại. Mã lỗi: {response.VnPayResponseCode}";
            return View("PaymentError");
        }

        public IActionResult ZaloPaymentCallback(string maDon)
        {
            Console.WriteLine("--- ZaloPay Callback Called ---");
            Console.WriteLine($"Ma Don nhan duoc: {maDon}");
            Console.WriteLine($"Query String: {Request.QueryString}");

            // 1. Verify Checksum
            var result = _zaloPayService.PaymentExecute(Request.Query);

            if (!result.Success)
            {
                ViewBag.Message = $"Lỗi Checksum ZaloPay: {result.Message}";
                return View("PaymentError");
            }

            // 2. Kiểm tra status (1 = Thành công)
            if (result.Status == 1)
            {
                if (string.IsNullOrEmpty(maDon))
                {
                    ViewBag.Message = "Thanh toán thành công nhưng không tìm thấy mã đơn hàng.";
                    return View("PaymentError");
                }

                // 3. Xử lý lưu vé
                bool ketQua = XuLyThanhToanThanhCong(maDon, "ZaloPay");

                if (ketQua)
                {
                    return RedirectToAction("PaymentSuccess");
                }
                else
                {
                    // Trường hợp vé đã được xử lý từ trước hoặc lỗi DB
                    // Vẫn cho là thành công để khách đỡ hoang mang, nhưng ghi log lại
                    return RedirectToAction("PaymentSuccess");
                }
            }

            ViewBag.Message = "Giao dịch ZaloPay thất bại hoặc bị hủy.";
            return View("PaymentError");
        }

        private bool XuLyThanhToanThanhCong(string maDon, string phuongThuc)
        {
            var donHang = db.DonDatVes.FirstOrDefault(d => d.MaDon == maDon);
            if (donHang != null)
            {
                // Kiểm tra nếu đã thanh toán rồi thì thôi (tránh double request)
                if (donHang.TrangThai == "Đã thanh toán") return true;

                donHang.TrangThai = "Đã thanh toán";
                var thanhToan = new ThanhToan
                {
                    MaDon = maDon,
                    PhuongThuc = phuongThuc,
                    ThoiGian = DateTime.Now,
                    TrangThai = "Thành công"
                };
                db.ThanhToans.Add(thanhToan);
                db.SaveChanges();

                GuiEmailVePhim(maDon); // Gửi mail background
                return true;
            }
            return false;
        }


        public IActionResult PaymentSuccess()
        {
            return View(); // Tạo View báo thành công
        }

        // Hàm phụ trợ gửi email (Chạy ngầm)
        private void GuiEmailVePhim(string maDon)
        {
            Task.Run(async () =>
            {
                try
                {
                    // TẠO SCOPE MỚI -> TẠO DB CONTEXT MỚI
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<QuanLyDatVePhimContext>();

                        // 1. Query dữ liệu bằng dbContext MỚI này (Không dùng biến 'db' cũ)
                        var donHang = await dbContext.DonDatVes
                            .Include(d => d.ChiTietDonDatVes).ThenInclude(ct => ct.MaVeNavigation).ThenInclude(v => v.MaSuatChieuNavigation).ThenInclude(s => s.MaPhimNavigation)
                            .Include(d => d.ChiTietDonDatVes).ThenInclude(ct => ct.MaVeNavigation).ThenInclude(v => v.MaSuatChieuNavigation).ThenInclude(s => s.MaPhongNavigation).ThenInclude(r => r.MaRapNavigation)
                            .Include(d => d.ChiTietDonDatVes) .ThenInclude(ct => ct.MaVeNavigation).ThenInclude(v => v.MaGheNavigation)
                            .FirstOrDefaultAsync(d => d.MaDon == maDon);

                        if (donHang == null) return;

                        // Lấy user
                        var nguoiDung = await dbContext.NguoiDungs.FindAsync(donHang.MaNd);
                        if (nguoiDung == null || string.IsNullOrEmpty(nguoiDung.Email)) return;

                        // 2. Chuẩn bị dữ liệu email
                        var veDau = donHang.ChiTietDonDatVes.FirstOrDefault()?.MaVeNavigation;
                        string tenPhim = veDau?.MaSuatChieuNavigation.MaPhimNavigation.TenPhim ?? "Phim";
                        string rap = veDau?.MaSuatChieuNavigation.MaPhongNavigation.MaRapNavigation.TenRap ?? "";
                        string phong = veDau?.MaSuatChieuNavigation.MaPhongNavigation.TenPhong ?? "";
                        string suat = $"{veDau?.MaSuatChieuNavigation.GioChieu} - {veDau?.MaSuatChieuNavigation.NgayChieu:dd/MM/yyyy}";

                        var listGhe = donHang.ChiTietDonDatVes.Select(ct => ct.MaVeNavigation.MaGheNavigation.HangGhe + ct.MaVeNavigation.MaGheNavigation.SoGhe).ToList();
                        string gheStr = string.Join(", ", listGhe);
                        decimal tongTien = donHang.ChiTietDonDatVes.Sum(x => x.GiaVe);

                        // 3. Tạo QR Code
                        var emailHelper = new EmailHelper(_config);
                        string qrCodeImage = emailHelper.GenerateQrCode("MaDon:" + donHang.MaDon);

                        // 4. Nội dung HTML
                        string content = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; border: 1px solid #ddd; padding: 20px;'>
                            <h2 style='color: #28a745; text-align: center;'>ĐẶT VÉ THÀNH CÔNG</h2>
                            <p>Xin chào <strong>{nguoiDung.HoTen}</strong>,</p>
                            <p>Thông tin vé của bạn:</p>
                            <table style='width: 100%; border-collapse: collapse;'>
                                <tr><td><strong>Phim:</strong></td><td>{tenPhim}</td></tr>
                                <tr><td><strong>Rạp/Phòng:</strong></td><td>{rap} - {phong}</td></tr>
                                <tr><td><strong>Suất:</strong></td><td>{suat}</td></tr>
                                <tr><td><strong>Ghế:</strong></td><td style='color:red; font-weight:bold'>{gheStr}</td></tr>
                                <tr><td><strong>Tổng tiền:</strong></td><td>{tongTien:N0} VNĐ</td></tr>
                            </table>
                            <div style='text-align: center; margin: 20px 0;'>
                                <img src='cid:qrcode' alt='QR Code' width='200' style='border: 5px solid #eee;' />
                                <p>Mã đơn: {donHang.MaDon}</p>
                            </div>
                        </div>";

                        // 5. Gửi Email
                        await emailHelper.SendTicketEmail(nguoiDung.Email, "[Galactic Cinema] Vé điện tử", content, qrCodeImage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private string RandomTransId()
        {
            return DateTime.Now.ToString("yyMMdd") + "_" + Guid.NewGuid().ToString().Substring(0, 10);
        }

    }
}
