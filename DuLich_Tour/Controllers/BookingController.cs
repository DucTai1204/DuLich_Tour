using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DuLich_Tour.Models;
using DuLich_Tour.Models.ViewModels;
using DuLich_Tour.Attributes;
using System.Data.Entity;

namespace DuLich_Tour.Controllers
{
    [RequireLogin]
    public class BookingController : Controller
    {
        /// <summary>
        /// GET: /Booking/Book/5 - Form đặt tour
        /// </summary>
        public ActionResult Book(int? tourId)
        {
            if (tourId == null)
            {
                return RedirectToAction("Index", "Tour");
            }

            TourDuLich tour = null;
            int? idKhachHang = Session["IdKhachHang"] as int?;

            using (var db = new TourDbContext())
            {
                tour = db.TourDuLiches
                    .FirstOrDefault(t => t.IdTour == tourId);

                if (tour == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra tour còn mở bán và còn chỗ
                if (tour.TrangThai != "mo-ban" || tour.SoChoConLai <= 0)
                {
                    TempData["ErrorMessage"] = "Tour này hiện không còn chỗ hoặc đã đóng bán.";
                    return RedirectToAction("Details", "Tour", new { id = tourId });
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

                // Lấy thông tin khách hàng nếu đã có
                if (idKhachHang.HasValue)
                {
                    var khachHang = db.KhachHangs.Find(idKhachHang.Value);
                    ViewBag.KhachHang = khachHang;
                }
            }

            var viewModel = new BookTourViewModel
            {
                TourId = tourId.Value,
                SoNguoiLon = 1,
                SoTreEm = 0,
                PhuongThucThanhToan = "online"
            };

            ViewBag.Tour = tour;
            return View(viewModel);
        }

        /// <summary>
        /// POST: /Booking/Book - Xử lý đặt tour
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(BookTourViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload tour info
                using (var db = new TourDbContext())
                {
                    var tour = db.TourDuLiches.Find(model.TourId);
                    if (tour != null)
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
                // Lấy tour
                var tour = db.TourDuLiches.Find(model.TourId);
                if (tour == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra lại tour còn mở bán và còn chỗ
                if (tour.TrangThai != "mo-ban")
                {
                    TempData["ErrorMessage"] = "Tour này đã đóng bán.";
                    return RedirectToAction("Details", "Tour", new { id = model.TourId });
                }

                int tongSoNguoi = model.SoNguoiLon + model.SoTreEm;
                if (tour.SoChoConLai < tongSoNguoi)
                {
                    TempData["ErrorMessage"] = $"Tour chỉ còn {tour.SoChoConLai} chỗ, không đủ cho {tongSoNguoi} người.";
                    return RedirectToAction("Book", new { tourId = model.TourId });
                }

                // Lấy khách hàng
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                    return RedirectToAction("Profile", "Account");
                }

                // Tính tổng tiền
                decimal tongTien = (model.SoNguoiLon * tour.GiaNguoiLon) + (model.SoTreEm * tour.GiaTreEm);

                // Áp dụng khuyến mãi nếu có
                if (tour.KhuyenMai != null && tour.KhuyenMai.TrangThai && 
                    (!tour.KhuyenMai.NgayBatDau.HasValue || tour.KhuyenMai.NgayBatDau.Value <= DateTime.Now) &&
                    (!tour.KhuyenMai.NgayKetThuc.HasValue || tour.KhuyenMai.NgayKetThuc.Value >= DateTime.Now))
                {
                    if (tour.KhuyenMai.KieuGiam == "phan_tram" && tour.KhuyenMai.GiaTri.HasValue)
                    {
                        tongTien = tongTien * (1 - tour.KhuyenMai.GiaTri.Value / 100);
                    }
                    else if (tour.KhuyenMai.KieuGiam == "tien" && tour.KhuyenMai.GiaTri.HasValue)
                    {
                        tongTien = tongTien - tour.KhuyenMai.GiaTri.Value;
                        if (tongTien < 0) tongTien = 0;
                    }
                }

                // Tạo đặt tour
                var datTour = new DatTour
                {
                    IdKhachHang = khachHang.IdKhachHang,
                    IdTour = model.TourId,
                    SoNguoiLon = model.SoNguoiLon,
                    SoTreEm = model.SoTreEm,
                    TongTien = tongTien,
                    TrangThai = "cho-xac-nhan",
                    PhuongThucThanhToan = model.PhuongThucThanhToan,
                    GhiChu = model.GhiChu,
                    NgayDat = DateTime.Now
                };

                db.DatTours.Add(datTour);

                // Cập nhật số chỗ còn lại
                tour.SoChoConLai -= tongSoNguoi;

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đặt tour thành công! Vui lòng thanh toán để hoàn tất.";
                return RedirectToAction("MyBookings");
            }
        }

        /// <summary>
        /// GET: /Booking/MyBookings - Lịch sử đặt tour
        /// </summary>
        public ActionResult MyBookings()
        {
            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            List<DatTour> bookings = new List<DatTour>();

            using (var db = new TourDbContext())
            {
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang != null)
                {
                    bookings = db.DatTours
                        .Where(d => d.IdKhachHang == khachHang.IdKhachHang)
                        .OrderByDescending(d => d.NgayDat)
                        .ToList();

                    // Load navigation properties
                    foreach (var booking in bookings)
                    {
                        booking.TourDuLich = db.TourDuLiches.Find(booking.IdTour);
                        if (booking.TourDuLich != null)
                        {
                            if (booking.TourDuLich.IdDiaDiem.HasValue)
                            {
                                booking.TourDuLich.DiaDiemDuLich = db.DiaDiemDuLiches.Find(booking.TourDuLich.IdDiaDiem.Value);
                            }
                        }
                    }
                }
            }

            return View(bookings);
        }

        /// <summary>
        /// GET: /Booking/Details/5 - Chi tiết đặt tour
        /// </summary>
        public ActionResult Details(int? id)
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

            DatTour datTour = null;

            using (var db = new TourDbContext())
            {
                datTour = db.DatTours.Find(id);
                if (datTour == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra đặt tour thuộc về user hiện tại
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null || datTour.IdKhachHang != khachHang.IdKhachHang)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xem đặt tour này.";
                    return RedirectToAction("MyBookings");
                }

                // Load navigation properties
                datTour.TourDuLich = db.TourDuLiches.Find(datTour.IdTour);
                datTour.KhachHang = khachHang;

                if (datTour.TourDuLich != null)
                {
                    if (datTour.TourDuLich.IdDiaDiem.HasValue)
                    {
                        datTour.TourDuLich.DiaDiemDuLich = db.DiaDiemDuLiches.Find(datTour.TourDuLich.IdDiaDiem.Value);
                    }
                    if (datTour.TourDuLich.IdKhuyenMai.HasValue)
                    {
                        datTour.TourDuLich.KhuyenMai = db.KhuyenMais.Find(datTour.TourDuLich.IdKhuyenMai.Value);
                    }
                }

                // Lấy thanh toán
                var thanhToans = db.ThanhToans
                    .Where(t => t.IdDatTour == id)
                    .OrderByDescending(t => t.NgayThanhToan)
                    .ToList();

                // Kiểm tra có thể hủy không (trước 3 ngày khởi hành)
                bool canCancel = false;
                if (datTour.TourDuLich != null && datTour.TourDuLich.NgayBatDau.HasValue)
                {
                    var daysUntilDeparture = (datTour.TourDuLich.NgayBatDau.Value - DateTime.Now).Days;
                    canCancel = daysUntilDeparture >= 3 && datTour.TrangThai != "da-huy";
                }

                var viewModel = new BookingDetailsViewModel
                {
                    DatTour = datTour,
                    TourDuLich = datTour.TourDuLich,
                    KhachHang = datTour.KhachHang,
                    ThanhToans = thanhToans,
                    CanCancel = canCancel
                };

                return View(viewModel);
            }
        }

        /// <summary>
        /// POST: /Booking/Cancel/5 - Hủy tour
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int? id)
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
                var datTour = db.DatTours.Find(id);
                if (datTour == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra quyền
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null || datTour.IdKhachHang != khachHang.IdKhachHang)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền hủy đặt tour này.";
                    return RedirectToAction("MyBookings");
                }

                // Kiểm tra có thể hủy không
                var tour = db.TourDuLiches.Find(datTour.IdTour);
                if (tour != null && tour.NgayBatDau.HasValue)
                {
                    var daysUntilDeparture = (tour.NgayBatDau.Value - DateTime.Now).Days;
                    if (daysUntilDeparture < 3)
                    {
                        TempData["ErrorMessage"] = "Không thể hủy tour. Chỉ có thể hủy trước 3 ngày khởi hành.";
                        return RedirectToAction("Details", new { id = id });
                    }
                }

                if (datTour.TrangThai == "da-huy")
                {
                    TempData["ErrorMessage"] = "Đặt tour này đã được hủy trước đó.";
                    return RedirectToAction("Details", new { id = id });
                }

                // Cập nhật trạng thái
                datTour.TrangThai = "da-huy";

                // Hoàn lại số chỗ
                if (tour != null)
                {
                    tour.SoChoConLai += (datTour.SoNguoiLon + datTour.SoTreEm);
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "Đã hủy đặt tour thành công.";
                return RedirectToAction("MyBookings");
            }
        }

        /// <summary>
        /// GET: /Booking/Invoice/5 - Xem hóa đơn
        /// </summary>
        public ActionResult Invoice(int? id)
        {
            return Details(id); // Dùng chung view với Details, chỉ khác layout
        }
    }
}

