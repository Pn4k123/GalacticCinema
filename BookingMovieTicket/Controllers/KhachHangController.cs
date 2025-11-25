using AutoMapper;
using BookingMovieTicket.Helper;
using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BookingMovieTicket.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IMapper _mapper;
        private readonly xuLyMaKH _xl;

        public KhachHangController(QuanLyDatVePhimContext context,IMapper mapper ,xuLyMaKH xl) {
            db = context;
            _mapper = mapper;
            _xl = xl;
        }
        [HttpGet]
        public IActionResult DangKy()
        {
            return PartialView("_DangKy", new DangKyVM());
        }

        [HttpPost]
        public IActionResult DangKy(DangKyVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var khachHang = _mapper.Map<NguoiDung>(model);
                    string randomKey = MyUtil.GenerateRandomKey();
                    khachHang.MatKhau = model.MatKhau.ToMd5Hash(randomKey);
                    khachHang.MaNd = _xl.khachHangId();
                    khachHang.VaiTro = "KhachHang";

                    db.NguoiDungs.Add(khachHang);
                    db.SaveChanges();
                    
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {

                }
            }
            return PartialView("_DangKy",model);
        }
    }
}
