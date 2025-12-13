using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookingMovieTicket.Models;

public partial class Ghe
{
    [DisplayName("Mã ghế")]
    public string MaGhe { get; set; } = null!;
    [DisplayName("Mã phòng")]
    public string MaPhong { get; set; } = null!;
    [DisplayName("Hàng ghế")]
    public string HangGhe { get; set; } = null!;
    [DisplayName("Số ghế")]
    public int SoGhe { get; set; }
    [DisplayName("Loại ghế")]
    public string LoaiGhe { get; set; } = null!;
    [DisplayName("Giá")]
    public decimal Gia { get; set; }

    public virtual Phong MaPhongNavigation { get; set; } = null!;

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
}
