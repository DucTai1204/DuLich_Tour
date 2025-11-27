using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DuLich_Tour.Models
{
    public class TourDuLich
    {
        [Key]
        [Display(Name = "Mã tour")]
        public int IdTour { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tour")]
        [MaxLength(255)]
        [Display(Name = "Tên tour")]
        public string TenTour { get; set; }

        [ForeignKey("DiaDiemDuLich")]
        [Display(Name = "Địa điểm du lịch")]
        [Required(ErrorMessage = "Vui lòng chọn địa điểm")]
        public int? IdDiaDiem { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string MoTaNgan { get; set; }

        [Display(Name = "Lịch trình chi tiết")]
        public string LichTrinh { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime? NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Giá người lớn")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal GiaNguoiLon { get; set; }

        [Display(Name = "Giá trẻ em")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal GiaTreEm { get; set; }

        [Display(Name = "Tổng số chỗ")]
        [Range(1, int.MaxValue, ErrorMessage = "Số chỗ phải lớn hơn 0")]
        public int SoCho { get; set; }

        [Display(Name = "Số chỗ còn lại")]
        [Range(0, int.MaxValue, ErrorMessage = "Số chỗ còn lại không hợp lệ")]
        public int SoChoConLai { get; set; }

        [MaxLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "mo-ban";

        [ForeignKey("KhuyenMai")]
        [Display(Name = "Chương trình khuyến mãi")]
        public int? IdKhuyenMai { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string HinhAnh { get; set; }

        // Navigation
        public virtual DiaDiemDuLich DiaDiemDuLich { get; set; }
        public virtual KhuyenMai KhuyenMai { get; set; }
        public virtual ICollection<DatTour> DatTours { get; set; }
        public virtual ICollection<DanhGiaTour> DanhGiaTours { get; set; }
    }
}
