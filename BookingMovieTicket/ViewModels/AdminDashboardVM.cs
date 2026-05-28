using System;
using System.Collections.Generic;

namespace BookingMovieTicket.ViewModels
{
    public class AdminDashboardVM
    {
        public int TongPhim { get; set; }
        public decimal TongDoanhThu { get; set; }
        public int TongKH { get; set; }
        public int TongVeBan { get; set; }
        public List<TopPhimVM> TopPhims { get; set; } = new List<TopPhimVM>();
        public List<DonHangMoiVM> DonHangMoi { get; set; } = new List<DonHangMoiVM>();
    }

    public class TopPhimVM
    {
        public string TenPhim { get; set; }
        public decimal DoanhThu { get; set; }
        public string Hinh { get; set; }
    }

    public class DonHangMoiVM
    {
        public string MaDon { get; set; }
        public string TenND { get; set; }
        public DateTime ThoiGianDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
    }
}
