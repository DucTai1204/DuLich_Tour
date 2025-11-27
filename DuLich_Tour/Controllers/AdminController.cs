using DuLich_Tour.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;
using System.Data.Entity.Validation;

namespace DuLich_Tour.Controllers
{
    public class AdminController : Controller
    {
        private readonly TourDbContext _context = new TourDbContext();

        private bool IsAdmin()
        {
            return Session["VaiTro"] != null && Session["VaiTro"].ToString() == "admin";
        }

        private ActionResult ForbidIfNotAdmin()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }
            return null;
        }

        private void NotifySuccess(string message)
        {
            TempData["Success"] = message;
        }

        private void NotifyError(string message)
        {
            TempData["Error"] = message;
        }

        private int GetTotalGuests(DatTour booking)
        {
            return (booking?.SoNguoiLon ?? 0) + (booking?.SoTreEm ?? 0);
        }

        private void PopulateTourLookups(int? selectedDiaDiem = null, int? selectedKhuyenMai = null)
        {
            var diaDiems = _context.DiaDiemDuLiches
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDiaDiem)
                .ToList();
            var khuyenMais = _context.KhuyenMais
                .Where(k => k.TrangThai)
                .OrderByDescending(k => k.NgayBatDau)
                .ToList();

            ViewBag.IdDiaDiem = new SelectList(diaDiems, "IdDiaDiem", "TenDiaDiem", selectedDiaDiem);
            ViewBag.IdKhuyenMai = new SelectList(khuyenMais, "IdKhuyenMai", "TenChuongTrinh", selectedKhuyenMai);
        }

        private void ValidateTourRules(TourDuLich model)
        {
            if (model.NgayBatDau.HasValue && model.NgayKetThuc.HasValue && model.NgayKetThuc < model.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
            }

            if (model.SoChoConLai < 0 || model.SoChoConLai > model.SoCho)
            {
                ModelState.AddModelError("SoChoConLai", "Số chỗ còn lại phải nhỏ hơn hoặc bằng tổng số chỗ và không âm.");
            }

            if (model.GiaTreEm > model.GiaNguoiLon)
            {
                ModelState.AddModelError("GiaTreEm", "Giá trẻ em không được lớn hơn giá người lớn.");
            }

            if (model.IdDiaDiem.HasValue)
            {
                var diaDiem = _context.DiaDiemDuLiches.FirstOrDefault(d => d.IdDiaDiem == model.IdDiaDiem && d.TrangThai);
                if (diaDiem == null)
                {
                    ModelState.AddModelError("IdDiaDiem", "Địa điểm không tồn tại hoặc đang bị khóa.");
                }
            }

            if (model.IdKhuyenMai.HasValue)
            {
                var km = _context.KhuyenMais.FirstOrDefault(k => k.IdKhuyenMai == model.IdKhuyenMai && k.TrangThai);
                if (km == null)
                {
                    ModelState.AddModelError("IdKhuyenMai", "Khuyến mãi không tồn tại hoặc đã tắt.");
                }
                else if (km.NgayKetThuc.HasValue && model.NgayBatDau.HasValue && km.NgayKetThuc.Value < model.NgayBatDau.Value)
                {
                    ModelState.AddModelError("IdKhuyenMai", "Khuyến mãi đã hết hạn trước ngày tour bắt đầu.");
                }
            }
        }

        private void ValidateKhuyenMaiRules(KhuyenMai model)
        {
            if (model.NgayBatDau.HasValue && model.NgayKetThuc.HasValue && model.NgayKetThuc < model.NgayBatDau)
            {
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.");
            }
        }

        private void ValidateKhachHangRules(KhachHang model)
        {
            if (string.IsNullOrWhiteSpace(model.GioiTinh))
            {
                model.GioiTinh = "khac";
            }
            if (model.NgayDangKy == default(DateTime))
            {
                model.NgayDangKy = DateTime.Now;
            }
        }

        private void ValidateBookingEntities(DatTour booking)
        {
            // Vì view model có thể thiếu navigation nên dùng EF để lấy đầy đủ
            if (booking.KhachHang == null && booking.IdKhachHang > 0)
            {
                booking.KhachHang = _context.KhachHangs.Find(booking.IdKhachHang);
            }
            if (booking.TourDuLich == null && booking.IdTour > 0)
            {
                booking.TourDuLich = _context.TourDuLiches.Find(booking.IdTour);
            }
        }

        private string BuildValidationMessage(DbEntityValidationException ex)
        {
            var errors = ex.EntityValidationErrors
                .SelectMany(e => e.ValidationErrors)
                .Select(e => $"{e.PropertyName}: {e.ErrorMessage}");
            return string.Join("; ", errors);
        }

        // Trang tổng quan Admin
        public ActionResult Index()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ViewBag.TourCount = _context.TourDuLiches.Count();
            ViewBag.DiaDiemCount = _context.DiaDiemDuLiches.Count();
            ViewBag.KhachHangCount = _context.KhachHangs.Count();
            ViewBag.KhuyenMaiCount = _context.KhuyenMais.Count();
            ViewBag.TaiKhoanCount = _context.TaiKhoans.Count();

            return View();
        }

        #region TourDuLich

        public ActionResult Tours()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var tours = _context.TourDuLiches
                .Include(t => t.DiaDiemDuLich)
                .Include(t => t.KhuyenMai)
                .OrderByDescending(t => t.NgayBatDau)
                .ToList();
            return View(tours);
        }

        public ActionResult CreateTour()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            PopulateTourLookups();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTour(TourDuLich model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ValidateTourRules(model);

            if (ModelState.IsValid)
            {
                _context.TourDuLiches.Add(model);
                _context.SaveChanges();
                NotifySuccess($"Đã tạo tour \"{model.TenTour}\".");
                return RedirectToAction("Tours");
            }

            PopulateTourLookups(model.IdDiaDiem, model.IdKhuyenMai);
            return View(model);
        }

        public ActionResult EditTour(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var tour = _context.TourDuLiches.Find(id);
            if (tour == null) return HttpNotFound();

            PopulateTourLookups(tour.IdDiaDiem, tour.IdKhuyenMai);
            return View(tour);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTour(TourDuLich model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ValidateTourRules(model);

            if (ModelState.IsValid)
            {
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
                NotifySuccess($"Đã cập nhật tour \"{model.TenTour}\".");
                return RedirectToAction("Tours");
            }

            PopulateTourLookups(model.IdDiaDiem, model.IdKhuyenMai);
            return View(model);
        }

        public ActionResult DeleteTour(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var tour = _context.TourDuLiches
                .Include(t => t.DiaDiemDuLich)
                .FirstOrDefault(t => t.IdTour == id);
            if (tour == null) return HttpNotFound();

            return View(tour);
        }

        [HttpPost, ActionName("DeleteTour")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTourConfirmed(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var tour = _context.TourDuLiches.Find(id);
            if (tour != null)
            {
                var tourName = tour.TenTour;
                _context.TourDuLiches.Remove(tour);
                _context.SaveChanges();
                NotifySuccess($"Đã xóa tour \"{tourName}\".");
            }
            return RedirectToAction("Tours");
        }

        #endregion

        #region DiaDiemDuLich

        public ActionResult DiaDiems()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var list = _context.DiaDiemDuLiches
                .OrderBy(d => d.TenDiaDiem)
                .ToList();
            return View(list);
        }

        public ActionResult CreateDiaDiem()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDiaDiem(DiaDiemDuLich model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (_context.DiaDiemDuLiches.Any(d => d.TenDiaDiem == model.TenDiaDiem))
            {
                ModelState.AddModelError("TenDiaDiem", "Tên địa điểm đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.DiaDiemDuLiches.Add(model);
                _context.SaveChanges();
                NotifySuccess($"Đã thêm địa điểm \"{model.TenDiaDiem}\".");
                return RedirectToAction("DiaDiems");
            }
            return View(model);
        }

        public ActionResult EditDiaDiem(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.DiaDiemDuLiches.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDiaDiem(DiaDiemDuLich model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (_context.DiaDiemDuLiches.Any(d => d.TenDiaDiem == model.TenDiaDiem && d.IdDiaDiem != model.IdDiaDiem))
            {
                ModelState.AddModelError("TenDiaDiem", "Tên địa điểm đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
                NotifySuccess($"Đã cập nhật địa điểm \"{model.TenDiaDiem}\".");
                return RedirectToAction("DiaDiems");
            }
            return View(model);
        }

        public ActionResult DeleteDiaDiem(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.DiaDiemDuLiches.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost, ActionName("DeleteDiaDiem")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDiaDiemConfirmed(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            bool isUsed = _context.TourDuLiches.Any(t => t.IdDiaDiem == id);
            if (isUsed)
            {
                NotifyError("Không thể xóa địa điểm vì đang được sử dụng trong tour.");
                return RedirectToAction("DiaDiems");
            }

            var item = _context.DiaDiemDuLiches.Find(id);
            if (item != null)
            {
                var name = item.TenDiaDiem;
                _context.DiaDiemDuLiches.Remove(item);
                _context.SaveChanges();
                NotifySuccess($"Đã xóa địa điểm \"{name}\".");
            }
            return RedirectToAction("DiaDiems");
        }

        #endregion

        #region KhuyenMai

        public ActionResult KhuyenMais()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var list = _context.KhuyenMais
                .OrderByDescending(k => k.NgayBatDau)
                .ToList();
            return View(list);
        }

        public ActionResult CreateKhuyenMai()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKhuyenMai(KhuyenMai model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ValidateKhuyenMaiRules(model);

            if (ModelState.IsValid)
            {
                _context.KhuyenMais.Add(model);
                _context.SaveChanges();
                NotifySuccess($"Đã tạo khuyến mãi \"{model.TenChuongTrinh}\".");
                return RedirectToAction("KhuyenMais");
            }
            return View(model);
        }

        public ActionResult EditKhuyenMai(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.KhuyenMais.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKhuyenMai(KhuyenMai model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ValidateKhuyenMaiRules(model);

            if (ModelState.IsValid)
            {
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
                NotifySuccess($"Đã cập nhật khuyến mãi \"{model.TenChuongTrinh}\".");
                return RedirectToAction("KhuyenMais");
            }
            return View(model);
        }

        public ActionResult DeleteKhuyenMai(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.KhuyenMais.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost, ActionName("DeleteKhuyenMai")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteKhuyenMaiConfirmed(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            bool inUse = _context.TourDuLiches.Any(t => t.IdKhuyenMai == id);
            if (inUse)
            {
                NotifyError("Không thể xóa khuyến mãi vì đang áp dụng cho tour.");
                return RedirectToAction("KhuyenMais");
            }

            var item = _context.KhuyenMais.Find(id);
            if (item != null)
            {
                var name = item.TenChuongTrinh;
                _context.KhuyenMais.Remove(item);
                _context.SaveChanges();
                NotifySuccess($"Đã xóa khuyến mãi \"{name}\".");
            }
            return RedirectToAction("KhuyenMais");
        }

        #endregion

        #region KhachHang

        public ActionResult KhachHangs()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var list = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .OrderByDescending(k => k.NgayDangKy)
                .ToList();
            return View(list);
        }

        public ActionResult CreateKhachHang()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            ViewBag.IdTaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IdTaiKhoan", "TenDangNhap");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateKhachHang(KhachHang model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (!_context.TaiKhoans.Any(t => t.IdTaiKhoan == model.IdTaiKhoan))
            {
                ModelState.AddModelError("IdTaiKhoan", "Tài khoản không tồn tại.");
            }

            if (_context.KhachHangs.Any(k => k.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                ValidateKhachHangRules(model);
                _context.KhachHangs.Add(model);
                _context.SaveChanges();
                NotifySuccess($"Đã thêm khách hàng \"{model.HoTen}\".");
                return RedirectToAction("KhachHangs");
            }

            ViewBag.IdTaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IdTaiKhoan", "TenDangNhap", model.IdTaiKhoan);
            return View(model);
        }

        public ActionResult EditKhachHang(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.KhachHangs.Find(id);
            if (item == null) return HttpNotFound();

            ViewBag.IdTaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IdTaiKhoan", "TenDangNhap", item.IdTaiKhoan);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKhachHang(KhachHang model)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (!_context.TaiKhoans.Any(t => t.IdTaiKhoan == model.IdTaiKhoan))
            {
                ModelState.AddModelError("IdTaiKhoan", "Tài khoản không tồn tại.");
            }

            if (_context.KhachHangs.Any(k => k.Email == model.Email && k.IdKhachHang != model.IdKhachHang))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            }

            if (ModelState.IsValid)
            {
                ValidateKhachHangRules(model);
                _context.Entry(model).State = EntityState.Modified;
                _context.SaveChanges();
                NotifySuccess($"Đã cập nhật khách hàng \"{model.HoTen}\".");
                return RedirectToAction("KhachHangs");
            }

            ViewBag.IdTaiKhoan = new SelectList(_context.TaiKhoans.ToList(), "IdTaiKhoan", "TenDangNhap", model.IdTaiKhoan);
            return View(model);
        }

        public ActionResult DeleteKhachHang(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.IdKhachHang == id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost, ActionName("DeleteKhachHang")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteKhachHangConfirmed(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            bool hasBooking = _context.DatTours.Any(dt => dt.IdKhachHang == id);
            if (hasBooking)
            {
                NotifyError("Không thể xóa khách hàng vì đã có đơn đặt tour.");
                return RedirectToAction("KhachHangs");
            }

            var item = _context.KhachHangs.Find(id);
            if (item != null)
            {
                var name = item.HoTen;
                _context.KhachHangs.Remove(item);
                _context.SaveChanges();
                NotifySuccess($"Đã xóa khách hàng \"{name}\".");
            }
            return RedirectToAction("KhachHangs");
        }

        #endregion

        #region TaiKhoan

        public ActionResult TaiKhoans()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var list = _context.TaiKhoans
                .OrderByDescending(t => t.NgayTao)
                .ToList();
            return View(list);
        }

        public ActionResult CreateTaiKhoan()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTaiKhoan(TaiKhoan model, string matKhauPlain)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (_context.TaiKhoans.Any(t => t.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            }

            if (string.IsNullOrWhiteSpace(matKhauPlain))
            {
                ModelState.AddModelError("matKhauPlain", "Vui lòng nhập mật khẩu.");
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrWhiteSpace(matKhauPlain))
                {
                    model.MatKhau = HashPasswordMd5(matKhauPlain);
                }
                _context.TaiKhoans.Add(model);
                _context.SaveChanges();
                NotifySuccess($"Đã tạo tài khoản \"{model.TenDangNhap}\".");
                return RedirectToAction("TaiKhoans");
            }
            return View(model);
        }

        public ActionResult EditTaiKhoan(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.TaiKhoans.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTaiKhoan(TaiKhoan model, string matKhauPlain)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            if (_context.TaiKhoans.Any(t => t.TenDangNhap == model.TenDangNhap && t.IdTaiKhoan != model.IdTaiKhoan))
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var existing = _context.TaiKhoans.Find(model.IdTaiKhoan);
                if (existing == null) return HttpNotFound();

                existing.TenDangNhap = model.TenDangNhap;
                existing.VaiTro = model.VaiTro;
                existing.TrangThai = model.TrangThai;

                if (!string.IsNullOrWhiteSpace(matKhauPlain))
                {
                    existing.MatKhau = HashPasswordMd5(matKhauPlain);
                }

                _context.SaveChanges();
                NotifySuccess($"Đã cập nhật tài khoản \"{existing.TenDangNhap}\".");
                return RedirectToAction("TaiKhoans");
            }
            return View(model);
        }

        public ActionResult DeleteTaiKhoan(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var item = _context.TaiKhoans.Find(id);
            if (item == null) return HttpNotFound();
            return View(item);
        }

        [HttpPost, ActionName("DeleteTaiKhoan")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTaiKhoanConfirmed(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            bool hasCustomer = _context.KhachHangs.Any(k => k.IdTaiKhoan == id);
            if (hasCustomer)
            {
                NotifyError("Không thể xóa tài khoản vì đang gắn với khách hàng.");
                return RedirectToAction("TaiKhoans");
            }

            var item = _context.TaiKhoans.Find(id);
            if (item != null)
            {
                var username = item.TenDangNhap;
                _context.TaiKhoans.Remove(item);
                _context.SaveChanges();
                NotifySuccess($"Đã xóa tài khoản \"{username}\".");
            }
            return RedirectToAction("TaiKhoans");
        }

#endregion

        #region DatTour

        public ActionResult DatTours()
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var list = _context.DatTours
                .Include(d => d.KhachHang)
                .Include(d => d.TourDuLich)
                .OrderByDescending(d => d.NgayDat)
                .ToList();
            return View(list);
        }

        public ActionResult DatTourDetails(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var booking = _context.DatTours
                .Include(d => d.KhachHang)
                .Include(d => d.TourDuLich)
                .Include(d => d.TourDuLich.DiaDiemDuLich)
                .Include(d => d.TourDuLich.KhuyenMai)
                .FirstOrDefault(d => d.IdDatTour == id);
            if (booking == null) return HttpNotFound();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveBooking(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var booking = _context.DatTours
                .Include(d => d.TourDuLich)
                .Include(d => d.KhachHang)
                .FirstOrDefault(d => d.IdDatTour == id);
            if (booking == null) return HttpNotFound();

            ValidateBookingEntities(booking);

            if (booking.TrangThai == "da-xac-nhan")
            {
                NotifyError("Đơn đặt tour đã được duyệt trước đó.");
                return RedirectToAction("DatTours");
            }

            var tour = booking.TourDuLich;
            if (tour == null)
            {
                NotifyError("Không tìm thấy thông tin tour để duyệt.");
                return RedirectToAction("DatTours");
            }

            int guests = GetTotalGuests(booking);
            if (tour.SoChoConLai < guests)
            {
                NotifyError("Không đủ số chỗ còn lại để duyệt đơn này.");
                return RedirectToAction("DatTours");
            }

            try
            {
                tour.SoChoConLai -= guests;
                booking.TrangThai = "da-xac-nhan";
                _context.SaveChanges();
                NotifySuccess($"Đã duyệt đơn đặt tour #{booking.IdDatTour}.");
            }
            catch (DbEntityValidationException ex)
            {
                NotifyError("Không thể duyệt đơn: " + BuildValidationMessage(ex));
            }
            return RedirectToAction("DatTours");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectBooking(int id)
        {
            var forbid = ForbidIfNotAdmin();
            if (forbid != null) return forbid;

            var booking = _context.DatTours
                .Include(d => d.TourDuLich)
                .Include(d => d.KhachHang)
                .FirstOrDefault(d => d.IdDatTour == id);
            if (booking == null) return HttpNotFound();

            ValidateBookingEntities(booking);

            if (booking.TrangThai == "da-huy")
            {
                NotifyError("Đơn đặt tour đã bị hủy trước đó.");
                return RedirectToAction("DatTours");
            }

            var tour = booking.TourDuLich;
            if (tour != null && booking.TrangThai == "da-xac-nhan")
            {
                tour.SoChoConLai += GetTotalGuests(booking);
            }

            try
            {
                booking.TrangThai = "da-huy";
                _context.SaveChanges();
                NotifySuccess($"Đã hủy đơn đặt tour #{booking.IdDatTour}.");
            }
            catch (DbEntityValidationException ex)
            {
                NotifyError("Không thể hủy đơn: " + BuildValidationMessage(ex));
            }
            return RedirectToAction("DatTours");
        }

        #endregion

        private string HashPasswordMd5(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(password);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
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


