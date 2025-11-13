using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class ThongBao
    {
        [Key]
        public int IdThongBao { get; set; }

        [ForeignKey("KhachHang")]
        public int IdKhachHang { get; set; }

        public string NoiDung { get; set; }

        [MaxLength(20)]
        public string Loai { get; set; } = "khac";

        public bool DaDoc { get; set; } = false;
        public DateTime NgayGui { get; set; } = DateTime.Now;

        // Navigation
        public virtual KhachHang KhachHang { get; set; }
    }
}
