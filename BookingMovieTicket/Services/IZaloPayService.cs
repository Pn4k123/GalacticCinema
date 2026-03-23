using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;

namespace BookingMovieTicket.Services
{
    public interface IZaloPayService
    {
        Task<ZaloPayOrderResponse> CreateOrderAsync(DonDatVe donHang);
        ZaloPayCallbackResult PaymentExecute(IQueryCollection collection);
    }
}
