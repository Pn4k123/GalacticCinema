using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace BookingMovieTicket.Controllers
{
    public class DonDatVeController : Controller
    {
        QuanLyDatVePhimContext db = new QuanLyDatVePhimContext();
        public IActionResult chiTietDonHang(string id)
        {
            SuatChieu sc = db.SuatChieus
                                .Include(x => x.MaPhimNavigation)
                                .Include(x => x.MaPhongNavigation)
                                .Include(x => x.MaPhongNavigation.MaRapNavigation)
                                .FirstOrDefault(x => x.MaSuatChieu == id);

            return View(sc);
           
        }
    }
}

