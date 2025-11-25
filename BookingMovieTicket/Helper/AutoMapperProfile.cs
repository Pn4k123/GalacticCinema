using AutoMapper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;

namespace BookingMovieTicket.Helper
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile() {
            CreateMap<DangKyVM, NguoiDung>();
        }
    }
}
