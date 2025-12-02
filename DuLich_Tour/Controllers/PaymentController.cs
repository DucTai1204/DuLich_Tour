using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DuLich_Tour.Models;
using DuLich_Tour.Models.ViewModels;
using DuLich_Tour.Attributes;
using System.Configuration;
using System.Data.Entity;
using Stripe;

namespace DuLich_Tour.Controllers
{
    [RequireLogin]
    public class PaymentController : Controller
    {
        /// <summary>
        /// GET: /Payment/Index?bookingId=5 - Form thanh toán
        /// </summary>
        public ActionResult Index(int? bookingId)
        {
            if (bookingId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đặt tour.";
                return RedirectToAction("MyBookings", "Booking");
            }

            int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
            if (!idTaiKhoan.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            DatTour datTour = null;
            using (var db = new TourDbContext())
            {
                datTour = db.DatTours.Find(bookingId);
                if (datTour == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đặt tour.";
                    return RedirectToAction("MyBookings", "Booking");
                }

                // Kiểm tra quyền
                var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                if (khachHang == null || datTour.IdKhachHang != khachHang.IdKhachHang)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thanh toán đặt tour này.";
                    return RedirectToAction("MyBookings", "Booking");
                }

                // Kiểm tra đã thanh toán chưa
                if (datTour.TrangThai == "da-thanh-toan")
                {
                    TempData["InfoMessage"] = "Đặt tour này đã được thanh toán.";
                    return RedirectToAction("Details", "Booking", new { id = bookingId });
                }

                // Load tour info
                datTour.TourDuLich = db.TourDuLiches.Find(datTour.IdTour);
                if (datTour.TourDuLich != null && datTour.TourDuLich.IdDiaDiem.HasValue)
                {
                    datTour.TourDuLich.DiaDiemDuLich = db.DiaDiemDuLiches.Find(datTour.TourDuLich.IdDiaDiem.Value);
                }
            }

            var viewModel = new PaymentViewModel
            {
                BookingId = bookingId.Value,
                SoTien = datTour.TongTien,
                CongThanhToan = "stripe"
            };

            ViewBag.DatTour = datTour;
            ViewBag.StripePublishableKey = ConfigurationManager.AppSettings["Stripe:PublishableKey"] ?? "";

            return View(viewModel);
        }

        /// <summary>
        /// POST: /Payment/CreatePaymentIntent - Tạo Payment Intent từ Stripe
        /// </summary>
        [HttpPost]
        public ActionResult CreatePaymentIntent(int bookingId, decimal amount)
        {
            try
            {
                int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
                if (!idTaiKhoan.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                using (var db = new TourDbContext())
                {
                    var datTour = db.DatTours.Find(bookingId);
                    if (datTour == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đặt tour." });
                    }

                    // Kiểm tra quyền
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                    if (khachHang == null || datTour.IdKhachHang != khachHang.IdKhachHang)
                    {
                        return Json(new { success = false, message = "Bạn không có quyền thanh toán đặt tour này." });
                    }

                    // Kiểm tra số tiền
                    if (amount != datTour.TongTien)
                    {
                        return Json(new { success = false, message = "Số tiền không khớp với đặt tour." });
                    }

                    // Lấy Stripe Secret Key
                    var stripeSecretKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
                    if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey == "YOUR_STRIPE_SECRET_KEY")
                    {
                        return Json(new { success = false, message = "Stripe Secret Key chưa được cấu hình." });
                    }

                    // Khởi tạo Stripe
                    StripeConfiguration.ApiKey = stripeSecretKey;

                    // Tạo Payment Intent
                    var options = new PaymentIntentCreateOptions
                    {
                        Amount = (long)(amount * 100), // Stripe sử dụng cents, VND không có decimal nên nhân 100
                        Currency = "vnd",
                        PaymentMethodTypes = new List<string> { "card" },
                        Metadata = new Dictionary<string, string>
                        {
                            { "bookingId", bookingId.ToString() },
                            { "customerId", khachHang.IdKhachHang.ToString() }
                        }
                    };

                    var service = new PaymentIntentService();
                    var paymentIntent = service.Create(options);

                    return Json(new { 
                        success = true, 
                        clientSecret = paymentIntent.ClientSecret,
                        paymentIntentId = paymentIntent.Id
                    });
                }
            }
            catch (StripeException ex)
            {
                System.Diagnostics.Debug.WriteLine("Stripe Error: " + ex.Message);
                return Json(new { success = false, message = "Lỗi Stripe: " + ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error creating payment intent: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo payment intent." });
            }
        }

        /// <summary>
        /// POST: /Payment/ConfirmPayment - Xác nhận thanh toán thành công
        /// </summary>
        [HttpPost]
        public ActionResult ConfirmPayment(int bookingId, string paymentIntentId, string paymentMethodId)
        {
            try
            {
                int? idTaiKhoan = Session["IdTaiKhoan"] as int?;
                if (!idTaiKhoan.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập." });
                }

                using (var db = new TourDbContext())
                {
                    var datTour = db.DatTours.Find(bookingId);
                    if (datTour == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đặt tour." });
                    }

                    // Kiểm tra quyền
                    var khachHang = db.KhachHangs.FirstOrDefault(k => k.IdTaiKhoan == idTaiKhoan.Value);
                    if (khachHang == null || datTour.IdKhachHang != khachHang.IdKhachHang)
                    {
                        return Json(new { success = false, message = "Bạn không có quyền thanh toán đặt tour này." });
                    }

                    // Kiểm tra đã thanh toán chưa
                    if (datTour.TrangThai == "da-thanh-toan")
                    {
                        return Json(new { success = false, message = "Đặt tour này đã được thanh toán." });
                    }

                    // Lấy Stripe Secret Key để verify payment intent
                    var stripeSecretKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
                    if (!string.IsNullOrEmpty(stripeSecretKey) && stripeSecretKey != "YOUR_STRIPE_SECRET_KEY")
                    {
                        StripeConfiguration.ApiKey = stripeSecretKey;
                        
                        // Verify payment intent với Stripe
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(paymentIntentId);

                        if (paymentIntent.Status != "succeeded")
                        {
                            return Json(new { success = false, message = "Thanh toán chưa được xác nhận từ Stripe." });
                        }
                    }

                    // Tạo bản ghi thanh toán
                    var thanhToan = new ThanhToan
                    {
                        IdDatTour = bookingId,
                        SoTien = datTour.TongTien,
                        NgayThanhToan = DateTime.Now,
                        TrangThai = "thanh-cong",
                        CongThanhToan = "stripe"
                    };

                    db.ThanhToans.Add(thanhToan);

                    // Cập nhật trạng thái đặt tour
                    datTour.TrangThai = "da-thanh-toan";
                    datTour.MaThanhToan = paymentIntentId;

                    db.SaveChanges();

                    return Json(new { 
                        success = true, 
                        message = "Thanh toán thành công!",
                        redirectUrl = Url.Action("Details", "Booking", new { id = bookingId })
                    });
                }
            }
            catch (StripeException ex)
            {
                System.Diagnostics.Debug.WriteLine("Stripe Error: " + ex.Message);
                return Json(new { success = false, message = "Lỗi Stripe: " + ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error confirming payment: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xác nhận thanh toán." });
            }
        }
    }
}
