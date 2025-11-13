using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class TheoDoiChuyenDi
    {
        [Key]
        public int IdTheoDoi { get; set; }

        [ForeignKey("DatTour")]
        public int IdDatTour { get; set; }

        public string TrangThaiHienTai { get; set; }
        public DateTime ThoiGianCapNhat { get; set; } = DateTime.Now;

        // Navigation
        public virtual DatTour DatTour { get; set; }
    }
}
