using BookingMovieTicket.Models;

namespace BookingMovieTicket.ViewModels
{
    public class SuatChieuVM
    {
        public string MaSuatChieu { get; set; } = null!;

        public string MaPhong { get; set; } = null!;

        public string MaPhim { get; set; } = null!;

        public DateOnly NgayChieu { get; set; }

        public TimeOnly GioChieu { get; set; }

        public virtual Phong MaPhongNavigation { get; set; } = null!;

        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    }
}
