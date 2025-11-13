using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class KhachHang
    {
        [Key]
        public int IdKhachHang { get; set; }

        [Required]
        // Đã xóa [ForeignKey("TaiKhoan")] để tránh gây nhầm lẫn 1-1 cho EF
        public int IdTaiKhoan { get; set; }

        [Required]
        [StringLength(255)]
        public string HoTen { get; set; }

        [Required]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(15)]
        public string SoDienThoai { get; set; }

        [StringLength(255)]
        public string DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string GioiTinh { get; set; } = "khac";

        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        // Thuộc tính điều hướng
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}