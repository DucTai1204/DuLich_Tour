using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DuLich_Tour.Models;
using DuLich_Tour.Attributes;
using System.Data.Entity;

namespace DuLich_Tour.Controllers
{
    [RequireLogin]
    public class ReviewController : Controller
    {
        /// <summary>
        /// GET: /Review/Create/5 - Form đánh giá tour
        /// </summary>
        public ActionResult Create(int? tourId)
        {
            if (tourId == null)
            {
                return RedirectToAction("Index", "Tour");
            }

            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            TourDuLich tour = null;
            bool hasBooked = false;

            using (var db = new TourDbContext())
            {
                // Tắt lazy loading
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                tour = db.TourDuLiches.Find(tourId);
                if (tour == null)
                {
                    return HttpNotFound();
                }

                // Load navigation property trước khi DbContext bị dispose
                if (tour.IdDiaDiem.HasValue)
                {
                    tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);
                }

                // Kiểm tra khách hàng đã đặt tour này chưa
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang != null)
                {
                    hasBooked = db.DatTours.Any(d => d.IdKhachHang == khachHang.IdKhachHang && 
                                                      d.IdTour == tourId.Value && 
                                                      d.TrangThai == "da-xac-nhan");
                }

                // Kiểm tra đã đánh giá chưa
                if (khachHang != null)
                {
                    var existingReview = db.DanhGiaTours.FirstOrDefault(d => d.IdTour == tourId.Value && d.IdKhachHang == khachHang.IdKhachHang);
                    if (existingReview != null)
                    {
                        TempData["InfoMessage"] = "Bạn đã đánh giá tour này rồi. Bạn có thể chỉnh sửa đánh giá của mình.";
                        return RedirectToAction("Edit", new { id = existingReview.IdDanhGia });
                    }
                }

                if (!hasBooked)
                {
                    TempData["ErrorMessage"] = "Bạn cần đặt và hoàn thành tour này trước khi đánh giá.";
                    return RedirectToAction("Details", "Tour", new { id = tourId });
                }
            }

            ViewBag.Tour = tour;
            ViewBag.HasBooked = hasBooked;

            var danhGia = new DanhGiaTour
            {
                IdTour = tourId.Value,
                DiemDanhGia = 5,
                NgayDanhGia = DateTime.Now
            };

            return View(danhGia);
        }

        /// <summary>
        /// POST: /Review/Create - Lưu đánh giá
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DanhGiaTour model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new TourDbContext())
                {
                    // Tắt lazy loading
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var tour = db.TourDuLiches.Find(model.IdTour);
                    if (tour != null && tour.IdDiaDiem.HasValue)
                    {
                        tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);
                    }
                    ViewBag.Tour = tour;
                }
                return View(model);
            }

            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new TourDbContext())
            {
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("Profile", "Account");
                }

                // Kiểm tra đã đánh giá chưa
                var existingReview = db.DanhGiaTours.FirstOrDefault(d => d.IdTour == model.IdTour && d.IdKhachHang == khachHang.IdKhachHang);
                if (existingReview != null)
                {
                    TempData["InfoMessage"] = "Bạn đã đánh giá tour này rồi.";
                    return RedirectToAction("Edit", new { id = existingReview.IdDanhGia });
                }

                model.IdKhachHang = khachHang.IdKhachHang;
                model.NgayDanhGia = DateTime.Now;
                model.HienThi = true;

                db.DanhGiaTours.Add(model);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá tour!";
                return RedirectToAction("Details", "Tour", new { id = model.IdTour });
            }
        }

        /// <summary>
        /// GET: /Review/Edit/5 - Chỉnh sửa đánh giá
        /// </summary>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new TourDbContext())
            {
                // Tắt lazy loading
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                var danhGia = db.DanhGiaTours.Find(id);
                if (danhGia == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra quyền
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null || danhGia.IdKhachHang != khachHang.IdKhachHang)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                    return RedirectToAction("MyReviews");
                }

                // Load tour info
                danhGia.TourDuLich = db.TourDuLiches.Find(danhGia.IdTour);
                if (danhGia.TourDuLich != null && danhGia.TourDuLich.IdDiaDiem.HasValue)
                {
                    danhGia.TourDuLich.DiaDiemDuLich = db.DiaDiemDuLiches.Find(danhGia.TourDuLich.IdDiaDiem.Value);
                }
                ViewBag.Tour = danhGia.TourDuLich;

                return View(danhGia);
            }
        }

        /// <summary>
        /// POST: /Review/Edit - Cập nhật đánh giá
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DanhGiaTour model)
        {
            if (!ModelState.IsValid)
            {
                using (var db = new TourDbContext())
                {
                    // Tắt lazy loading
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ProxyCreationEnabled = false;

                    var tour = db.TourDuLiches.Find(model.IdTour);
                    if (tour != null && tour.IdDiaDiem.HasValue)
                    {
                        tour.DiaDiemDuLich = db.DiaDiemDuLiches.Find(tour.IdDiaDiem.Value);
                    }
                    ViewBag.Tour = tour;
                }
                return View(model);
            }

            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new TourDbContext())
            {
                var danhGia = db.DanhGiaTours.Find(model.IdDanhGia);
                if (danhGia == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra quyền
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null || danhGia.IdKhachHang != khachHang.IdKhachHang)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa đánh giá này.";
                    return RedirectToAction("MyReviews");
                }

                // Cập nhật
                danhGia.DiemDanhGia = model.DiemDanhGia;
                danhGia.NoiDung = model.NoiDung;
                danhGia.NgayDanhGia = DateTime.Now;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã cập nhật đánh giá thành công!";
                return RedirectToAction("Details", "Tour", new { id = danhGia.IdTour });
            }
        }

        /// <summary>
        /// GET: /Review/MyReviews - Đánh giá của tôi
        /// </summary>
        public ActionResult MyReviews()
        {
            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            List<DanhGiaTour> reviews = new List<DanhGiaTour>();

            using (var db = new TourDbContext())
            {
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang != null)
                {
                    reviews = db.DanhGiaTours
                        .Where(d => d.IdKhachHang == khachHang.IdKhachHang)
                        .OrderByDescending(d => d.NgayDanhGia)
                        .ToList();

                    // Load navigation properties
                    foreach (var review in reviews)
                    {
                        review.TourDuLich = db.TourDuLiches.Find(review.IdTour);
                        if (review.TourDuLich != null && review.TourDuLich.IdDiaDiem.HasValue)
                        {
                            review.TourDuLich.DiaDiemDuLich = db.DiaDiemDuLiches.Find(review.TourDuLich.IdDiaDiem.Value);
                        }
                    }
                }
            }

            return View(reviews);
        }
    }
}

