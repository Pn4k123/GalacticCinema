using BookingMovieTicket.Models;

namespace BookingMovieTicket.Helper
{
    public class xuLyMaVe
    {
        private readonly QuanLyDatVePhimContext db;

        public xuLyMaVe(QuanLyDatVePhimContext context)
        {
            db = context;
        }
        public string veId()
        {
            int count = db.Ves.Where(v => v.MaVe.StartsWith("V")).Count();
            return "V" + count.ToString("D3");
        }
    }
}
