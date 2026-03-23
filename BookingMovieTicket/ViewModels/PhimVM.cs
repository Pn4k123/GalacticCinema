namespace BookingMovieTicket.ViewModels
{
    public class PhimVM
    {
        public string MaPhim { get; set; } 

        public string TenPhim { get; set; } = null!;
        public string Poster { get; set; } = null!;

        public string Trailer {  get; set; } = null!;
        public DateTime? NgayPhatHanh { get; set; }
    }
}
