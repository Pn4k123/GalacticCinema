using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class DonDatVe
{
    [DisplayName("Mã đơn")]
    public string MaDon { get; set; } = null!;
    [DisplayName("Mã người dùng")]

    public string MaNd { get; set; } = null!;
    [DisplayName("Thời gian đặt vé")]

    public DateTime ThoiGianDat { get; set; }
    [DisplayName("Trạng thái")]

    public string TrangThai { get; set; } = null!;

    public virtual ICollection<ChiTietDonDatVe> ChiTietDonDatVes { get; set; } = new List<ChiTietDonDatVe>();

    public virtual NguoiDung MaNdNavigation { get; set; } = null!;

    public virtual ThanhToan? ThanhToan { get; set; }
}
