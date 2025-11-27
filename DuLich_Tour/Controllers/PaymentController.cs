using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DuLich_Tour.Models;
using DuLich_Tour.Attributes;
using DuLich_Tour.Models.ViewModels;
using System.Configuration;
using System.Data.Entity;
// Stripe.NET package - Đã cài đặt version 50.0.0
using Stripe;

namespace DuLich_Tour.Controllers
{
    [RequireLogin]
    public class PaymentController : Controller
    {
        /// <summary>
        /// GET: /Payment/Pay/5 - Form thanh toán
        /// </summary>
        public ActionResult Pay(int? bookingId)
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
                        return Json(new { success = false, message = "Số tiền thanh toán không khớp." });
                    }

                    // Lấy Stripe Secret Key
                    string stripeSecretKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
                    if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey.Contains("YOUR_SECRET_KEY"))
                    {
                        return Json(new { success = false, message = "Cấu hình Stripe chưa được thiết lập. Vui lòng cập nhật Stripe:SecretKey trong Web.config." });
                    }

                    // Khởi tạo Stripe
                    StripeConfiguration.ApiKey = stripeSecretKey;

                    // Tạo Payment Intent từ Stripe API
                    string clientSecret;
                    string paymentIntentId = "";
                    try
                    {
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
                        clientSecret = paymentIntent.ClientSecret;
                        paymentIntentId = paymentIntent.Id;
                        
                        // Log để debug (có thể xóa sau)
                        System.Diagnostics.Debug.WriteLine($"Stripe Payment Intent created: {paymentIntentId}, Status: {paymentIntent.Status}");
                    }
                    catch (StripeException stripeEx)
                    {
                        return Json(new { success = false, message = "Lỗi Stripe: " + stripeEx.Message + " (Code: " + stripeEx.StripeError?.Code + ")" });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Lỗi khi tạo payment intent: " + ex.Message });
                    }

                    return Json(new
                    {
                        success = true,
                        clientSecret = clientSecret,
                        paymentIntentId = paymentIntentId,
                        message = "Tạo payment intent thành công."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: /Payment/ConfirmPayment - Xác nhận thanh toán từ Stripe
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

                    // Lấy Stripe Secret Key
                    string stripeSecretKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
                    if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey.Contains("YOUR_SECRET_KEY"))
                    {
                        return Json(new { success = false, message = "Cấu hình Stripe chưa được thiết lập. Vui lòng cập nhật Stripe:SecretKey trong Web.config." });
                    }

                    // Xác nhận Payment Intent từ Stripe API
                    bool paymentSucceeded = false;
                    try
                    {
                        StripeConfiguration.ApiKey = stripeSecretKey;
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(paymentIntentId);

                        if (paymentIntent == null)
                        {
                            return Json(new { success = false, message = "Không tìm thấy payment intent." });
                        }

                        if (paymentIntent.Status != "succeeded")
                        {
                            return Json(new { 
                                success = false, 
                                message = $"Thanh toán chưa được xác nhận. Trạng thái: {paymentIntent.Status}" 
                            });
                        }

                        paymentSucceeded = true;
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Lỗi khi xác nhận thanh toán: " + ex.Message });
                    }

                    if (paymentSucceeded)
                    {
                        // Tạo bản ghi thanh toán
                        var thanhToan = new ThanhToan
                        {
                            IdDatTour = bookingId,
                            SoTien = datTour.TongTien,
                            NgayThanhToan = DateTime.Now,
                            TrangThai = "da-xac-nhan",
                            CongThanhToan = "stripe"
                        };
                        db.ThanhToans.Add(thanhToan);

                        // Cập nhật trạng thái đặt tour
                        datTour.TrangThai = "da-thanh-toan";
                        datTour.MaThanhToan = paymentIntentId;

                        db.SaveChanges();

                        return Json(new
                        {
                            success = true,
                            message = "Thanh toán thành công!",
                            redirectUrl = Url.Action("Details", "Booking", new { id = bookingId })
                        });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Thanh toán không thành công. Vui lòng thử lại." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        /// <summary>
        /// POST: /Payment/Webhook - Webhook từ Stripe để cập nhật trạng thái thanh toán
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Webhook()
        {
            try
            {
                var json = new System.IO.StreamReader(Request.InputStream).ReadToEnd();
                
                // Lấy Stripe Secret Key và Webhook Secret
                string stripeSecretKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
                string webhookSecret = ConfigurationManager.AppSettings["Stripe:WebhookSecret"] ?? "";

                if (string.IsNullOrEmpty(stripeSecretKey) || stripeSecretKey.Contains("YOUR_SECRET_KEY"))
                {
                    return new HttpStatusCodeResult(400, "Stripe Secret Key chưa được cấu hình");
                }

                // Webhook secret là tùy chọn, chỉ cần khi thiết lập webhook
                if (string.IsNullOrEmpty(webhookSecret))
                {
                    // Nếu chưa có webhook secret, có thể bỏ qua xác thực hoặc trả về lỗi
                    // Tùy chọn: return new HttpStatusCodeResult(400, "Webhook secret chưa được cấu hình");
                }

                // Xác thực webhook từ Stripe (Uncomment sau khi cài đặt Stripe.NET package)
                /*
                StripeConfiguration.ApiKey = stripeSecretKey;
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    webhookSecret
                );

                // Xử lý các event từ Stripe
                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        if (paymentIntent?.Metadata != null && paymentIntent.Metadata.ContainsKey("bookingId"))
                        {
                            int bookingId = int.Parse(paymentIntent.Metadata["bookingId"]);
                            UpdatePaymentStatus(bookingId, paymentIntent.Id, "da-xac-nhan");
                        }
                        break;
                    case Events.PaymentIntentPaymentFailed:
                        // Xử lý thanh toán thất bại nếu cần
                        var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                        if (failedPayment?.Metadata != null && failedPayment.Metadata.ContainsKey("bookingId"))
                        {
                            // Có thể ghi log hoặc thông báo cho khách hàng
                        }
                        break;
                }
                */

                // Tạm thời: Trả về 200 OK (XÓA sau khi uncomment code trên)
                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                // Log error nếu cần
                return new HttpStatusCodeResult(400, ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán
        /// </summary>
        private void UpdatePaymentStatus(int bookingId, string paymentIntentId, string status)
        {
            using (var db = new TourDbContext())
            {
                var datTour = db.DatTours.Find(bookingId);
                if (datTour != null)
                {
                    // Kiểm tra đã có thanh toán chưa
                    var existingPayment = db.ThanhToans
                        .FirstOrDefault(t => t.IdDatTour == bookingId && t.CongThanhToan == "stripe");

                    if (existingPayment == null)
                    {
                        // Tạo mới
                        var thanhToan = new ThanhToan
                        {
                            IdDatTour = bookingId,
                            SoTien = datTour.TongTien,
                            NgayThanhToan = DateTime.Now,
                            TrangThai = status,
                            CongThanhToan = "stripe"
                        };
                        db.ThanhToans.Add(thanhToan);
                    }
                    else
                    {
                        // Cập nhật
                        existingPayment.TrangThai = status;
                    }

                    if (status == "da-xac-nhan")
                    {
                        datTour.TrangThai = "da-thanh-toan";
                        datTour.MaThanhToan = paymentIntentId;
                    }

                    db.SaveChanges();
                }
            }
        }
    }
}

