using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BookingMovieTicket.Models;

public partial class Rap
{
    [DisplayName("Mã rạp")]
    public string MaRap { get; set; } = null!;
    [DisplayName("Tên rạp")]
    public string TenRap { get; set; } = null!;
    [DisplayName("Địa chỉ")]
    public string DiaChi { get; set; } = null!;
    [DisplayName("Trạng thái")]
    public string TrangThai { get; set; } = null!;

    public virtual ICollection<Phong> Phongs { get; set; } = new List<Phong>();
}
