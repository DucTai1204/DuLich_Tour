using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class ThanhToan
    {
        [Key]
        public int IdThanhToan { get; set; }

        [ForeignKey("DatTour")]
        public int IdDatTour { get; set; }

        public decimal SoTien { get; set; }
        public DateTime NgayThanhToan { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string TrangThai { get; set; } = "cho-xac-nhan";

        public string CongThanhToan { get; set; }

        // Navigation
        public virtual DatTour DatTour { get; set; }
    }
}
