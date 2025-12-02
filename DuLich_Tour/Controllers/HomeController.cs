using DuLich_Tour.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic; // 👉 bổ sung dòng này

namespace DuLich_Tour.Controllers
{
    public class HomeController : Controller
    {
        private readonly TourDbContext _context = new TourDbContext();

        public ActionResult Index()
        {
            // Kiểm tra nếu đăng nhập với role admin thì redirect đến Admin/Index
            if (Session["VaiTro"] != null && Session["VaiTro"].ToString() == "admin")
            {
                return Redirect("/Admin/Index");
            }

            // Nếu là khach_hang hoặc chưa đăng nhập, hiển thị trang chủ như bình thường
            List<TourDuLich> tours = new List<TourDuLich>();

            using (var db = new TourDbContext())
            {
                tours = db.TourDuLiches
                    .Where(t => t.TrangThai == "mo-ban" && t.SoChoConLai > 0)
                    .OrderByDescending(t => t.NgayBatDau)
                    .Take(6)
                    .ToList();

                foreach (var tour in tours)
                {
                    if (tour.IdDiaDiem.HasValue)
                        tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);

                    if (tour.IdKhuyenMai.HasValue)
                        tour.KhuyenMai = db.KhuyenMais.Find(tour.IdKhuyenMai.Value);
                }
            }

            return View(tours);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _context.Dispose();
            base.Dispose(disposing);
        }
    }
}
