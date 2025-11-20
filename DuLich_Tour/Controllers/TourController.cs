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
        /// <summary>
        /// GET: /Tour - Danh sách tất cả tour với tìm kiếm và lọc theo địa điểm, giá, ngày khởi hành
        /// </summary>
        public ActionResult Index(string search = "", int? diaDiemId = null, decimal? minPrice = null, decimal? maxPrice = null, DateTime? fromDate = null, DateTime? toDate = null)
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

                // Filter theo giá
                if (minPrice.HasValue)
                {
                    query = query.Where(t => t.GiaNguoiLon >= minPrice.Value);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(t => t.GiaNguoiLon <= maxPrice.Value);
                }

                // Filter theo ngày khởi hành
                if (fromDate.HasValue)
                {
                    query = query.Where(t => t.NgayBatDau.HasValue && t.NgayBatDau.Value >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(t => t.NgayBatDau.HasValue && t.NgayBatDau.Value <= toDate.Value);
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

                // Load navigation properties và thông tin đánh giá
                var tourIds = tours.Select(t => t.IdTour).ToList();
                var danhGias = db.DanhGiaTours
                    .Where(d => tourIds.Contains(d.IdTour) && d.HienThi == true)
                    .GroupBy(d => d.IdTour)
                    .Select(g => new
                    {
                        TourId = g.Key,
                        SoLuong = g.Count(),
                        DiemTrungBinh = g.Average(d => (double)d.DiemDanhGia)
                    })
                    .ToList();

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

                    // Load thông tin đánh giá
                    var danhGiaInfo = danhGias.FirstOrDefault(d => d.TourId == tour.IdTour);
                    if (danhGiaInfo != null)
                    {
                        ViewData[$"ReviewCount_{tour.IdTour}"] = danhGiaInfo.SoLuong;
                        ViewData[$"ReviewAvg_{tour.IdTour}"] = Math.Round(danhGiaInfo.DiemTrungBinh, 1);
                    }
                    else
                    {
                        ViewData[$"ReviewCount_{tour.IdTour}"] = 0;
                        ViewData[$"ReviewAvg_{tour.IdTour}"] = 0.0;
                    }
                }
            }

            ViewBag.DiaDiems = diaDiems;
            ViewBag.SearchTerm = search;
            ViewBag.DiaDiemId = diaDiemId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(tours);
        }

        /// <summary>
        /// GET: /Tour/Details/5 - Chi tiết tour
        /// </summary>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            TourDuLich tour = null;
            List<TourDuLich> relatedTours = new List<TourDuLich>();

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

                // Lấy tour tương tự (cùng địa điểm hoặc tour khác đang mở bán)
                var query = db.TourDuLiches
                    .Where(t => t.IdTour != id && 
                                t.TrangThai == "mo-ban" && 
                                t.SoChoConLai > 0);
                
                // Ưu tiên tour cùng địa điểm
                if (tour.IdDiaDiem.HasValue)
                {
                    query = query.Where(t => t.IdDiaDiem == tour.IdDiaDiem);
                }
                
                relatedTours = query
                    .OrderByDescending(t => t.NgayBatDau)
                    .Take(4)
                    .ToList();

                // Load navigation properties cho tour tương tự
                foreach (var relatedTour in relatedTours)
                {
                    if (relatedTour.IdDiaDiem.HasValue)
                    {
                        relatedTour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(relatedTour.IdDiaDiem.Value);
                    }
                    if (relatedTour.IdKhuyenMai.HasValue)
                    {
                        relatedTour.KhuyenMai = db.KhuyenMais.Find(relatedTour.IdKhuyenMai.Value);
                    }
                }

                // Lấy đánh giá tour
                var danhGias = db.DanhGiaTours
                    .Where(d => d.IdTour == id && d.HienThi == true)
                    .OrderByDescending(d => d.NgayDanhGia)
                    .ToList();

                // Load thông tin khách hàng cho đánh giá
                foreach (var danhGia in danhGias)
                {
                    danhGia.KhachHang = db.KhachHangs.Find(danhGia.IdKhachHang);
                }

                ViewBag.DanhGias = danhGias;

                // Kiểm tra user đã đặt tour và đã đánh giá chưa
                int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
                bool hasBooked = false;
                bool hasReviewed = false;

                if (idTaiKhoan.HasValue)
                {
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                    if (khachHang != null)
                    {
                        hasBooked = db.DatTours.Any(d => d.IdKhachHang == khachHang.IdKhachHang && 
                                                          d.IdTour == id && 
                                                          d.TrangThai == "da-xac-nhan");
                        if (hasBooked)
                        {
                            hasReviewed = db.DanhGiaTours.Any(d => d.IdTour == id && d.IdKhachHang == khachHang.IdKhachHang);
                        }
                    }
                }

                ViewBag.HasBooked = hasBooked;
                ViewBag.HasReviewed = hasReviewed;
            }

            ViewBag.RelatedTours = relatedTours;
            return View(tour);
        }
    }
}