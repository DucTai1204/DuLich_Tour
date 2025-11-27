using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class KhuyenMai
    {
        [Key]
        [Display(Name = "Mã khuyến mãi")]
        public int IdKhuyenMai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương trình")]
        [MaxLength(255)]
        [Display(Name = "Tên chương trình")]
        public string TenChuongTrinh { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Giá trị giảm")]
        public decimal? GiaTri { get; set; }

        [MaxLength(20)]
        [Display(Name = "Kiểu giảm giá")]
        public string KieuGiam { get; set; } = "phan_tram";

        [Display(Name = "Ngày bắt đầu")]
        public DateTime? NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        public virtual ICollection<TourDuLich> TourDuLiches { get; set; }
    }
}
