using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class KhuyenMai
    {
        [Key]
        public int IdKhuyenMai { get; set; }

        [Required, MaxLength(255)]
        public string TenChuongTrinh { get; set; }

        public string MoTa { get; set; }

        public decimal? GiaTri { get; set; }

        [MaxLength(20)]
        public string KieuGiam { get; set; } = "phan_tram";

        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public bool TrangThai { get; set; } = true;

        // Navigation
        public virtual ICollection<TourDuLich> TourDuLiches { get; set; }
    }
}
