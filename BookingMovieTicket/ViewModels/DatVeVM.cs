namespace BookingMovieTicket.ViewModels
{
    public class DatVeVM
    {
        public string MaSuatChieu { get; set; }

        public string MaPhim { get; set; }
        public string TenPhim { get; set; }
        public string? Poster { get; set; }
        public string TenPhong { get; set; }
        public string TenRap { get; set; }
        public TimeOnly GioChieu { get; set; }
        public DateOnly NgayChieu { get; set; }

        public List<GheHienThi> DanhSachGhe { get; set; }
    }

    public class GheHienThi
    {
        public string MaGhe { get; set; } 
        public int SoGhe { get; set; } 
        public string HangGhe { get; set; } 
        public decimal Gia { get; set; }

        public string LoaiGhe { get; set; } 
        public bool DaDat { get; set; }
    }
}
