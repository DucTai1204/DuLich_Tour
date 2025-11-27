using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class KhachHang
    {
        [Key]
        [Display(Name = "Mã khách hàng")]
        public int IdKhachHang { get; set; }

        [Required]
        // Đã xóa [ForeignKey("TaiKhoan")] để tránh gây nhầm lẫn 1-1 cho EF
        [Display(Name = "Tài khoản liên kết")]
        public int IdTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(255)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = "khac";

        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Thuộc tính điều hướng
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}