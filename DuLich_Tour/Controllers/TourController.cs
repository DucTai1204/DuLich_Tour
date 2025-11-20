using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DuLich_Tour.Models;
using System.Data.Entity;

namespace DuLich_Tour.Controllers
{
    public class TourController : Controller
    {
        // GET: /Tour - Danh sách tất cả tour
        public ActionResult Index(string search = "", int? diaDiemId = null)
        {
            List<TourDuLich> tours = new List<TourDuLich>();
            List<DiaDiemDuLich> diaDiems = new List<DiaDiemDuLich>();

            using (var db = new TourDbContext())
            {
                // Lấy danh sách địa điểm để filter
                diaDiems = db.DiaDiemDuLiches
                    .Where(d => d.TrangThai == true)
                    .OrderBy(d => d.TenDiaDiem)
                    .ToList();

                // Query tour
                var query = db.TourDuLiches
                    .Where(t => t.TrangThai == "mo-ban" && t.SoChoConLai > 0)
                    .AsQueryable();

                // Filter theo địa điểm
                if (diaDiemId.HasValue)
                {
                    query = query.Where(t => t.IdDiaDiem == diaDiemId.Value);
                }

                // Search theo tên tour hoặc mô tả
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(t => 
                        t.TenTour.Contains(search) || 
                        (t.MoTaNgan != null && t.MoTaNgan.Contains(search)));
                }

                tours = query
                    .OrderByDescending(t => t.NgayBatDau)
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

            ViewBag.DiaDiems = diaDiems;
            ViewBag.SearchTerm = search;
            ViewBag.DiaDiemId = diaDiemId;

            return View(tours);
        }

        // GET: /Tour/Details/5 - Chi tiết tour
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            TourDuLich tour = null;

            using (var db = new TourDbContext())
            {
                tour = db.TourDuLiches
                    .FirstOrDefault(t => t.IdTour == id);

                if (tour == null)
                {
                    return HttpNotFound();
                }

                // Load navigation properties
                if (tour.IdDiaDiem.HasValue)
                {
                    tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);
                }
                if (tour.IdKhuyenMai.HasValue)
                {
                    tour.KhuyenMai = db.KhuyenMais.Find(tour.IdKhuyenMai.Value);
                }
            }

            return View(tour);
        }
    }
}

