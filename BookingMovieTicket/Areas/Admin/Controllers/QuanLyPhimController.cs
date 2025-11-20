using BookingMovieTicket.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookingMovieTicket.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class QuanLyPhimController : Controller
    {
        private readonly QuanLyDatVePhimContext db;
        private readonly IWebHostEnvironment env;

        public QuanLyPhimController(QuanLyDatVePhimContext context, IWebHostEnvironment _env)
        {
            db = context;
            env = _env;
        }

        public IActionResult Index()
        {
            ViewBag.qlp = db.Phims.ToList();
            return View();
        }

        [HttpGet]
        public IActionResult them()
        {
            ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult them(Phim phim, List<string> MaTheLoais)
        {
            if (MaTheLoais == null || !MaTheLoais.Any())
            {
                ModelState.AddModelError("MaTheLoais", "Vui lòng chọn ít nhất 1 thể loại phim");
            }

            if (phim.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "Vui lòng chọn hình ảnh");
            }
           
            if (MaTheLoais != null && MaTheLoais.Any())
            {
                var theLoais = db.TheLoais.Where(tl => MaTheLoais.Contains(tl.MaTheLoai)).ToList();
                phim.MaTheLoais = theLoais;
            }

            if (phim.ImageFile != null)
            {
                string uploadFolder = Path.Combine(env.WebRootPath, "upload");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                string extension = Path.GetExtension(phim.ImageFile.FileName);

                string uniqueFileName = Guid.NewGuid().ToString() + extension;

                string filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    phim.ImageFile.CopyTo(fs);
                }

                phim.Poster = "/upload/" + uniqueFileName;

                db.Phims.Add(phim);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            ViewBag.DSTheLoai = new SelectList(db.TheLoais.ToList(), "MaTheLoai", "TenTheLoai", MaTheLoais);
            return View(phim);
        }

    }
}

