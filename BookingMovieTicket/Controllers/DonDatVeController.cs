using AutoMapper;
using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace BookingMovieTicket.Controllers
{
    public class DonDatVeController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly xuLyMaDon _xlDon;

        public DonDatVeController(QuanLyDatVePhimContext context, xuLyMaDon xlDon)
        {
            db = context;
            _xlDon = xlDon;
        }
        public IActionResult chiTietDonHang(string id)
        {
            SuatChieu sc = db.SuatChieus
                                .Include(x => x.MaPhimNavigation)
                                .Include(x => x.MaPhongNavigation)
                                .Include(x => x.MaPhongNavigation.MaRapNavigation)
                                .FirstOrDefault(x => x.MaSuatChieu == id);

            if (sc == null) return NotFound();

            var tatCaGhe = db.Ghes
                            .Where(g => g.MaPhong == sc.MaPhong)
                            .ToList();

            var gheDaDat = db.Ves
                            .Where(v => v.MaSuatChieu == id)
                            .Where(v => v.TrangThai == "Chưa sử dụng" || v.TrangThai == "Đã sử dụng")
                            .Select(v => v.MaGhe)
                            .ToList();

            var model = new DatVeVM
            {
                MaSuatChieu = sc.MaSuatChieu,
                MaPhim = sc.MaPhim,
                TenPhim = sc.MaPhimNavigation.TenPhim,
                Poster = sc.MaPhimNavigation.Poster,
                TenPhong = sc.MaPhongNavigation.TenPhong,
                TenRap = sc.MaPhongNavigation.MaRapNavigation.TenRap,
                GioChieu = sc.GioChieu,
                NgayChieu = sc.NgayChieu,
                DanhSachGhe = tatCaGhe.Select(g => new GheHienThi
                {
                    MaGhe = g.MaGhe,
                    SoGhe = g.SoGhe,
                    HangGhe = g.HangGhe,
                    Gia = g.Gia,
                    LoaiGhe = g.LoaiGhe,
                    DaDat = gheDaDat.Contains(g.MaGhe)
                })
                .OrderBy(g => g.HangGhe)
                .ThenBy(g => g.SoGhe)
                .ToList()
            };

            return View(model);

        }

        [HttpPost]
        [Authorize]
        public IActionResult XacNhanDatVe(string maSuatChieu, List<string> danhSachMaGheChon)
        {
            if (danhSachMaGheChon == null || !danhSachMaGheChon.Any())
            {
                return RedirectToAction("chiTietDonHang", new { id = maSuatChieu });
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var gheBiTrung = db.Ves
                                     .Any(v => v.MaSuatChieu == maSuatChieu && danhSachMaGheChon.Contains(v.MaGhe));

                    if (gheBiTrung) return Content("Ghế đã bị người khác đặt!");

                    var donHang = new DonDatVe
                    {
                        MaDon = _xlDon.donId(),
                        MaNd = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        ThoiGianDat = DateTime.Now,
                        TrangThai = "Đang chờ"
                    };
                    db.DonDatVes.Add(donHang);
                    db.SaveChanges();

                    int soLuongVeHienTai = db.Ves.Count(v => v.MaVe.StartsWith("V"));

                    foreach (var maGhe in danhSachMaGheChon)
                    {
                        soLuongVeHienTai++;

                        string maVeTuSinh = "V" + soLuongVeHienTai.ToString("D3");

                        var gheInfo = db.Ghes.Find(maGhe);

                        var veMoi = new Ve
                        {
                            MaVe = maVeTuSinh,
                            MaSuatChieu = maSuatChieu,
                            MaGhe = maGhe,
                            TrangThai = "Chưa sử dụng",
                            ThoiGianPhatHanh = DateTime.Now
                        };
                        db.Ves.Add(veMoi);

                        var chiTiet = new ChiTietDonDatVe
                        {
                            MaDon = donHang.MaDon,
                            MaVe = veMoi.MaVe,
                            GiaVe = gheInfo.Gia
                        };
                        db.ChiTietDonDatVes.Add(chiTiet);
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    return RedirectToAction("Index","ThanhToan", new { maDon = donHang.MaDon });

                } catch (Exception ex) {
                    transaction.Rollback();
                    // Log lỗi ex.Message ra để debug
                    return Content("Lỗi đặt vé: " + ex.Message);
                }
            } 
        }
    }
}

