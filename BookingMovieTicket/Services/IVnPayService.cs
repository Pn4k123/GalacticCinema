using BookingMovieTicket.ViewModels;

namespace BookingMovieTicket.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context,VnPaymentRequestModel model);

        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
