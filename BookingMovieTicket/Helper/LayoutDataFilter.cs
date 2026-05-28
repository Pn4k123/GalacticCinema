using BookingMovieTicket.Models;
using BookingMovieTicket.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BookingMovieTicket.Helper
{
    public class LayoutDataFilter : IAsyncActionFilter
    {
        private readonly QuanLyDatVePhimContext _db;

        public LayoutDataFilter(QuanLyDatVePhimContext db)
        {
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Result is ViewResult viewResult)
            {
                var area = context.RouteData.Values["area"]?.ToString();
                if (area != "Admin")
                {
                    var controller = context.Controller as Controller;
                    if (controller != null)
                    {
                        controller.ViewBag.DSRap = _db.Raps.ToList();

                        controller.ViewBag.PhimSapChieu = _db.Phims.Select(phim => new PhimVM
                        {
                            MaPhim = phim.MaPhim,
                            TenPhim = phim.TenPhim,
                            Poster = phim.Poster,
                            NgayPhatHanh = phim.NgayPhatHanh
                        }).Where(p => p.NgayPhatHanh > DateTime.Now).OrderByDescending(p => p.NgayPhatHanh).Take(4).ToList();

                        controller.ViewBag.PhimDangChieu = _db.Phims.Select(phim => new PhimVM
                        {
                            MaPhim = phim.MaPhim,
                            TenPhim = phim.TenPhim,
                            Poster = phim.Poster,
                            NgayPhatHanh = phim.NgayPhatHanh
                        }).Where(p => p.NgayPhatHanh <= DateTime.Now).OrderByDescending(p => p.NgayPhatHanh).Take(4).ToList();
                    }
                }
            }
        }
    }
}
