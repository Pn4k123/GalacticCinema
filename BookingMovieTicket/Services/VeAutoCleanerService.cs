using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BookingMovieTicket.Models;

namespace BookingMovieTicket.Services
{
    public class VeAutoCleanerService : BackgroundService
    {
        // Dùng ScopeFactory để tạo scope lấy DbContext
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VeAutoCleanerService> _logger;

        public VeAutoCleanerService(IServiceScopeFactory scopeFactory, ILogger<VeAutoCleanerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động hủy vé bắt đầu chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Tạo một scope mới để xử lý DB
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<QuanLyDatVePhimContext>();

                        // 1. Định nghĩa thời gian hết hạn (Ví dụ: 10 phút trước)
                        var timeLimit = DateTime.Now.AddMinutes(-10);

                        // 2. Tìm các đơn hàng "Chờ thanh toán" đã quá hạn
                        var donHangQuaHan = await context.DonDatVes
                            .Where(d => d.TrangThai == "Đang chờ" && d.ThoiGianDat < timeLimit)
                            .Include(d => d.ChiTietDonDatVes) // Kèm chi tiết để lấy mã vé
                            .ToListAsync();

                        if (donHangQuaHan.Any())
                        {
                            foreach (var donHang in donHangQuaHan)
                            {
                                // 1. Lấy danh sách ChiTietDonDatVe cần xóa
                                // (Đây là bảng trung gian nối Đơn và Vé, phải xóa nó trước)
                                var chiTietCanXoa = donHang.ChiTietDonDatVes.ToList();

                                // 2. Lấy danh sách Vé cần xóa
                                var maVeList = chiTietCanXoa.Select(ct => ct.MaVe).ToList();
                                var veCanXoa = await context.Ves
                                    .Where(v => maVeList.Contains(v.MaVe))
                                    .ToListAsync();

                                // 3. THỰC HIỆN XÓA THEO THỨ TỰ
                                // Bước A: Xóa Chi Tiết Đơn Đặt Vé trước (Gỡ ràng buộc khóa ngoại)
                                if (chiTietCanXoa.Any())
                                {
                                    context.ChiTietDonDatVes.RemoveRange(chiTietCanXoa);
                                }

                                // Bước B: Bây giờ mới được xóa Vé (Nhả ghế)
                                if (veCanXoa.Any())
                                {
                                    context.Ves.RemoveRange(veCanXoa);
                                }

                                // 4. Cập nhật trạng thái đơn hàng
                                donHang.TrangThai = "Đã hủy";

                                // (Tùy chọn) Nếu bạn muốn lưu lại lịch sử là đơn này từng đặt vé nào
                                // thì không được xóa ChiTietDonDatVe, nhưng Database của bạn phải
                                // cấu hình cho phép MaVe trong ChiTietDonDatVe được phép NULL (Set Null on Delete).
                                // Nhưng với cấu trúc hiện tại, xóa cả 2 là giải pháp nhanh nhất.

                                _logger.LogInformation($"Đã hủy đơn hàng {donHang.MaDon}, xóa {chiTietCanXoa.Count} chi tiết và giải phóng {veCanXoa.Count} ghế.");
                            }

                            // Lưu thay đổi vào DB
                            await context.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi chạy quét vé quá hạn.");
                }

                // Chờ 1 phút (60000ms) trước khi quét lần tiếp theo
                await Task.Delay(60000, stoppingToken);
            }
        }
    }
}