namespace BookingMovieTicket.ViewModels
{
    public class LichSuDatVeVM
    {
        public string MaDon { get; set; }
        public DateTime NgayDat { get; set; }
        public string TenPhim { get; set; }

        public string Poster { get; set; }
        public DateOnly? NgayChieu { get; set; }
        public TimeOnly? GioChieu { get; set; }
        public string TenRapvaPhong { get; set; }
        public string Ghe { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }
}
