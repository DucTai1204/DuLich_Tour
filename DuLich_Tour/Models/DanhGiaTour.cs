using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class DanhGiaTour
    {
        [Key]
        public int IdDanhGia { get; set; }

        [ForeignKey("TourDuLich")]
        public int IdTour { get; set; }

        [ForeignKey("KhachHang")]
        public int IdKhachHang { get; set; }

        [Range(1, 5)]
        public int DiemDanhGia { get; set; }

        public string NoiDung { get; set; }
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;
        public bool HienThi { get; set; } = true;

        // Navigation
        public virtual TourDuLich TourDuLich { get; set; }
        public virtual KhachHang KhachHang { get; set; }
    }
}
