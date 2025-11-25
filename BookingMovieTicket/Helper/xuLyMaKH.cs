using BookingMovieTicket.Models;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaKH
    {
        private readonly QuanLyDatVePhimContext db;

        public xuLyMaKH(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        public string khachHangId()
        {
            int count = db.NguoiDungs.Where(kh => kh.MaNd.StartsWith("KH")).Count();
            return "KH" + count.ToString("D3");
        }
    }
}
