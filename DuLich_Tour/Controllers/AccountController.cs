using DuLich_Tour.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace DuLich_Tour.Controllers
{
    public class AccountController : Controller
    {
        // Loại bỏ TourDbContext db = new TourDbContext();
        // Thay thế bằng using block trong từng action để đảm bảo Dispose

        // GET: /Account/Register
        public ActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển hướng về trang chủ
            if (Session["TenDangNhap"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string tenDangNhap, string matKhau, string email, string hoTen, string soDienThoai)
        {
            // Kiểm tra nếu là AJAX request
            string requestedWith = Request.Headers["X-Requested-With"];
            if (!string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return RegisterAjax(tenDangNhap, matKhau, email, hoTen, soDienThoai);
            }

            if (ModelState.IsValid)
            {
                using (var db = new TourDbContext()) // Đảm bảo Dispose
                {
                    // 1. Kiểm tra username/email đã tồn tại
                    if (db.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                    {
                        ModelState.AddModelError("", "Tên đăng nhập đã tồn tại!");
                        return View();
                    }
                    if (db.KhachHangs.Any(k => k.Email == email))
                    {
                        ModelState.AddModelError("", "Email đã được đăng ký!");
                        return View();
                    }

                    // 2. Hash mật khẩu (Lưu ý: Nên dùng PBKDF2/BCrypt thay vì MD5)
                    string hashedPassword = HashPassword(matKhau);

                    // 3. Tạo TaiKhoan
                    TaiKhoan tk = new TaiKhoan
                    {
                        TenDangNhap = tenDangNhap,
                        MatKhau = hashedPassword,
                        VaiTro = "khach_hang",
                        TrangThai = true,
                        NgayTao = DateTime.Now
                    };
                    db.TaiKhoans.Add(tk);
                    db.SaveChanges(); // Lưu để có IdTaiKhoan

                    // 4. Tạo KhachHang
                    KhachHang kh = new KhachHang
                    {
                        IdTaiKhoan = tk.IdTaiKhoan, // Lấy ID sau khi SaveChanges()
                        HoTen = hoTen,
                        Email = email,
                        SoDienThoai = soDienThoai,
                        NgayDangKy = DateTime.Now
                    };
                    db.KhachHangs.Add(kh);
                    db.SaveChanges();
                }

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View();
        }

        // Xử lý đăng ký qua AJAX
        private ActionResult RegisterAjax(string tenDangNhap, string matKhau, string email, string hoTen, string soDienThoai)
        {
            if (string.IsNullOrEmpty(tenDangNhap) || string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(hoTen))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc!" }, JsonRequestBehavior.AllowGet);
            }

            using (var db = new TourDbContext())
            {
                // 1. Kiểm tra username/email đã tồn tại
                if (db.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                {
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" }, JsonRequestBehavior.AllowGet);
                }
                if (db.KhachHangs.Any(k => k.Email == email))
                {
                    return Json(new { success = false, message = "Email đã được đăng ký!" }, JsonRequestBehavior.AllowGet);
                }

                // 2. Hash mật khẩu
                string hashedPassword = HashPassword(matKhau);

                // 3. Tạo TaiKhoan
                TaiKhoan tk = new TaiKhoan
                {
                    TenDangNhap = tenDangNhap,
                    MatKhau = hashedPassword,
                    VaiTro = "khach_hang",
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };
                db.TaiKhoans.Add(tk);
                db.SaveChanges();

                // 4. Tạo KhachHang
                KhachHang kh = new KhachHang
                {
                    IdTaiKhoan = tk.IdTaiKhoan,
                    HoTen = hoTen,
                    Email = email,
                    SoDienThoai = soDienThoai ?? "",
                    NgayDangKy = DateTime.Now
                };
                db.KhachHangs.Add(kh);
                db.SaveChanges();
            }

            return Json(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            // Nếu đã đăng nhập, chuyển hướng về trang chủ
            if (Session["TenDangNhap"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string tenDangNhap, string matKhau)
        {
            // Kiểm tra nếu là AJAX request
            string requestedWith = Request.Headers["X-Requested-With"];
            if (!string.IsNullOrEmpty(requestedWith) && requestedWith.Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
            {
                return LoginAjax(tenDangNhap, matKhau);
            }

            // 1. Hash mật khẩu và tìm kiếm
            string hashedPassword = HashPassword(matKhau);

            using (var db = new TourDbContext())
            {
                var user = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == tenDangNhap && t.MatKhau == hashedPassword);

                if (user != null)
                {
                    // 2. Kiểm tra trạng thái
                    if (!user.TrangThai)
                    {
                        ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                        return View();
                    }

                    // 3. Thiết lập Session
                    Session["IdTaiKhoan"] = user.IdTaiKhoan;
                    Session["TenDangNhap"] = user.TenDangNhap;
                    Session["VaiTro"] = user.VaiTro;

                    // 4. Cập nhật lần đăng nhập cuối
                    user.LanDangNhapCuoi = DateTime.Now;
                    db.SaveChanges();

                    return RedirectToAction("Index", "Home"); // chuyển đến trang chính
                }
            } // db.Dispose() được gọi tự động ở đây

            ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
            return View();
        }

        // Xử lý login qua AJAX
        private ActionResult LoginAjax(string tenDangNhap, string matKhau)
        {
            // 1. Hash mật khẩu và tìm kiếm
            string hashedPassword = HashPassword(matKhau);

            using (var db = new TourDbContext())
            {
                var user = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == tenDangNhap && t.MatKhau == hashedPassword);

                if (user != null)
                {
                    // 2. Kiểm tra trạng thái
                    if (!user.TrangThai)
                    {
                        return Json(new { success = false, message = "Tài khoản của bạn đã bị khóa." }, JsonRequestBehavior.AllowGet);
                    }

                    // 3. Thiết lập Session
                    Session["IdTaiKhoan"] = user.IdTaiKhoan;
                    Session["TenDangNhap"] = user.TenDangNhap;
                    Session["VaiTro"] = user.VaiTro;

                    // 4. Cập nhật lần đăng nhập cuối
                    user.LanDangNhapCuoi = DateTime.Now;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Đăng nhập thành công!" }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { success = false, message = "Tên đăng nhập hoặc mật khẩu không đúng!" }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            // Nếu muốn loại bỏ Cookie Authentication cũng có thể thêm FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }

        // Hàm hash mật khẩu (MD5 - Giữ nguyên logic của bạn)
        private string HashPassword(string password)
        {
            // LƯU Ý QUAN TRỌNG: MD5 không an toàn cho mật khẩu.
            // Trong ứng dụng thực tế, nên sử dụng thuật toán hash mạnh hơn như PBKDF2 hoặc BCrypt.
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
    }
}