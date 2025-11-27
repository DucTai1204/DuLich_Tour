using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models
{
    public class DiaDiemDuLich
    {
        [Key]
        [Display(Name = "Mã địa điểm")]
        public int IdDiaDiem { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên địa điểm")]
        [MaxLength(255)]
        [Display(Name = "Tên địa điểm")]
        public string TenDiaDiem { get; set; }

        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Vị trí")]
        public string ViTri { get; set; }

        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Đang sử dụng")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        public virtual ICollection<TourDuLich> TourDuLiches { get; set; }
    }
}
