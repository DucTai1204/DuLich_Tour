using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class TourDuLich
    {
        [Key]
        public int IdTour { get; set; }

        [Required, MaxLength(255)]
        public string TenTour { get; set; }

        [ForeignKey("DiaDiemDuLich")]
        public int? IdDiaDiem { get; set; }

        public string MoTaNgan { get; set; }
        public string LichTrinh { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }

        public decimal GiaNguoiLon { get; set; }
        public decimal GiaTreEm { get; set; }

        public int SoCho { get; set; }
        public int SoChoConLai { get; set; }

        [MaxLength(20)]
        public string TrangThai { get; set; } = "mo-ban";

        [ForeignKey("KhuyenMai")]
        public int? IdKhuyenMai { get; set; }

        public string HinhAnh { get; set; }

        // Navigation
        public virtual DiaDiemDuLich DiaDiemDuLich { get; set; }
        public virtual KhuyenMai KhuyenMai { get; set; }
        public virtual ICollection<DatTour> DatTours { get; set; }
        public virtual ICollection<DanhGiaTour> DanhGiaTours { get; set; }
    }
}
