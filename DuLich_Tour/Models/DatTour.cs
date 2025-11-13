using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class DatTour
    {
        [Key]
        public int IdDatTour { get; set; }

        [ForeignKey("KhachHang")]
        public int IdKhachHang { get; set; }

        [ForeignKey("TourDuLich")]
        public int IdTour { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        public int SoNguoiLon { get; set; } = 1;
        public int SoTreEm { get; set; } = 0;

        public decimal TongTien { get; set; }

        [MaxLength(30)]
        public string TrangThai { get; set; } = "cho-xac-nhan";

        [MaxLength(20)]
        public string PhuongThucThanhToan { get; set; } = "online";

        public string MaThanhToan { get; set; }
        public string GhiChu { get; set; }

        // Navigation
        public virtual KhachHang KhachHang { get; set; }
        public virtual TourDuLich TourDuLich { get; set; }
        public virtual ICollection<ThanhToan> ThanhToans { get; set; }
        public virtual ICollection<TheoDoiChuyenDi> TheoDoiChuyenDis { get; set; }
    }
}
