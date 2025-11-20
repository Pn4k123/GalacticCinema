using Microsoft.AspNetCore.Mvc;

namespace BookingMovieTicket.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }


    }

}
