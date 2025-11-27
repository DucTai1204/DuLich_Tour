using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class TaiKhoan
    {
        [Key]
        [Display(Name = "Mã tài khoản")]
        public int IdTaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(100)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; }

        [StringLength(20)]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } = "khach_hang";

        [Display(Name = "Hoạt động")]
        public bool TrangThai { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Lần đăng nhập cuối")]
        public DateTime? LanDangNhapCuoi { get; set; }

        // Quan hệ 1-N: 1 TaiKhoan có thể có nhiều KhachHang (Đã đúng)
        public virtual ICollection<KhachHang> KhachHangs { get; set; }
    }
}