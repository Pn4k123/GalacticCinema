using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookingMovieTicket.Models;

public partial class Phim
{
    [Key]
    [DisplayName("Mã Phim")]
    [Required(ErrorMessage ="Vui lòng nhập mã phim")]
    public string MaPhim { get; set; } = null!;
    [DisplayName("Tên Phim")]
    [Required(ErrorMessage ="Vui lòng nhập tên phim")]
    public string TenPhim { get; set; } = null!;
    [DisplayName("Nội dung")]
    [Required(ErrorMessage = "Vui lòng nhập nội dung phim")]
    public string MoTa { get; set; } = null!;

    [DisplayName("Thời lượng")]
    [Required(ErrorMessage = "Vui lòng nhập thời lượng")]
    public int? ThoiLuong { get; set; }
    [DisplayName("Đạo diễn")]
    [Required(ErrorMessage = "Vui lòng nhập đạo diễn")]
    public string DaoDien { get; set; } = null!;
    [DisplayName("Đánh giá")]
    [Required(ErrorMessage = "Vui lòng nhập đánh giá")]
    public string DanhGia { get; set; } = null!;
    [DisplayName("Ngày Phát Hành")]
    [Required(ErrorMessage = "Vui lòng nhập ngày phát hành")]
    public DateTime? NgayPhatHanh { get; set; }
    [DisplayName("Trailer")]
    [Required(ErrorMessage = "Vui lòng nhập trailer")]
    public string Trailer { get; set; } = null!;
    [DisplayName("Poster")]
    public string Poster { get; set; } = null!;
    [NotMapped]
    [DisplayName("Upload File")]
    [Required(ErrorMessage = "Vui lòng chọn hình ảnh")]
    public IFormFile ImageFile {  get; set; }

    public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    [DisplayName("Danh sách thể loại phim")]
    public virtual ICollection<TheLoai> MaTheLoais { get; set; } = new List<TheLoai>();
}
