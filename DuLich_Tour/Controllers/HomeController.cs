using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DuLich_Tour.Models;

namespace DuLich_Tour.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            List<TourDuLich> tours = new List<TourDuLich>();
            
            using (var db = new TourDbContext())
            {
                // Lấy các tour đang mở bán, có chỗ trống
                tours = db.TourDuLiches
                    .Where(t => t.TrangThai == "mo-ban" && t.SoChoConLai > 0)
                    .OrderByDescending(t => t.NgayBatDau)
                    .Take(6) // Lấy 6 tour đầu tiên
                    .ToList();

                // Load navigation properties
                foreach (var tour in tours)
                {
                    if (tour.IdDiaDiem.HasValue)
                    {
                        tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);
                    }
                    if (tour.IdKhuyenMai.HasValue)
                    {
                        tour.KhuyenMai = db.KhuyenMais.Find(tour.IdKhuyenMai.Value);
                    }
                }
            }
            
            return View(tours);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}