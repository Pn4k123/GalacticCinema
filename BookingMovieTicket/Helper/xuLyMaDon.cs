using BookingMovieTicket.Models;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaDon
    {
        private readonly QuanLyDatVePhimContext db;

        public xuLyMaDon(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        public string donId()
        {
            int count = db.DonDatVes.Where(v => v.MaDon.StartsWith("D")).Count();
            return "D" + count.ToString("D3");
        }
    }
}
