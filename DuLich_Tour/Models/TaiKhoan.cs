using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class TaiKhoan
    {
        [Key]
        public int IdTaiKhoan { get; set; }

        [Required]
        [StringLength(100)]
        public string TenDangNhap { get; set; }

        [Required]
        [StringLength(255)]
        public string MatKhau { get; set; }

        [StringLength(20)]
        public string VaiTro { get; set; } = "khach_hang";

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public DateTime? LanDangNhapCuoi { get; set; }

        // Quan hệ 1-N: 1 TaiKhoan có thể có nhiều KhachHang (Đã đúng)
        public virtual ICollection<KhachHang> KhachHangs { get; set; }
    }
}