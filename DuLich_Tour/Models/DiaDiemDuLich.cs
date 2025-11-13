using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class DiaDiemDuLich
    {
        [Key]
        public int IdDiaDiem { get; set; }

        [Required, MaxLength(255)]
        public string TenDiaDiem { get; set; }

        public string MoTa { get; set; }
        public string ViTri { get; set; }
        public string HinhAnh { get; set; }
        public bool TrangThai { get; set; } = true;

        // Navigation
        public virtual ICollection<TourDuLich> TourDuLiches { get; set; }
    }
}
