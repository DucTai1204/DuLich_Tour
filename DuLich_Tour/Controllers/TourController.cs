using DuLich_Tour.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace DuLich_Tour.Controllers
{
    public class TourController : Controller
    {
        private readonly TourDbContext _context = new TourDbContext();

        // GET: Tour
        public ActionResult Index()
        {
            // Lấy danh sách Tour đang mở bán, include DiaDiemDuLich để hiển thị tên địa điểm
            var tours = _context.TourDuLiches
                                .Include(t => t.DiaDiemDuLich)
                                .Where(t => t.TrangThai == "mo-ban")
                                .OrderByDescending(t => t.NgayBatDau)
                                .ToList();

            return View(tours);
        }

        // GET: Tour/Details/5
        public ActionResult Details(int id)
        {
            var tour = _context.TourDuLiches
                               .Include(t => t.DiaDiemDuLich)
                               .Include(t => t.KhuyenMai)
                               .FirstOrDefault(t => t.IdTour == id);

            if (tour == null)
                return HttpNotFound();

            return View(tour);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
