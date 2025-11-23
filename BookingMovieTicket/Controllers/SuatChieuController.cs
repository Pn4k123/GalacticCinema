using Microsoft.AspNetCore.Mvc;
using System;
using System.Globalization;

namespace BookingMovieTicket.Controllers
{
    public class SuatChieuController : Controller
    {
        // Accept the date as a string from AJAX and parse to DateOnly
        public IActionResult LoadByDate(string maphim, string maRap, string ngay)
        {
            // Parse DateOnly an toàn
            DateOnly parsedDate;
            if (!DateOnly.TryParse(ngay, out parsedDate))
            {
                parsedDate = default;
            }

            return ViewComponent("SuatChieu", new { maphim, maRap, ngay = parsedDate });
        }
    }
}
