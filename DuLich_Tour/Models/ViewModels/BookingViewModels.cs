using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form đặt tour
    /// </summary>
    public class BookTourViewModel
    {
        [Required]
        public int TourId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số người lớn")]
        [Range(1, 50, ErrorMessage = "Số người lớn phải từ 1 đến 50")]
        [Display(Name = "Số người lớn")]
        public int SoNguoiLon { get; set; } = 1;

        [Range(0, 50, ErrorMessage = "Số trẻ em phải từ 0 đến 50")]
        [Display(Name = "Số trẻ em")]
        public int SoTreEm { get; set; } = 0;

        [MaxLength(20)]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = "online";

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }

    /// <summary>
    /// ViewModel cho chi tiết đặt tour
    /// </summary>
    public class BookingDetailsViewModel
    {
        public DatTour DatTour { get; set; }
        public TourDuLich TourDuLich { get; set; }
        public KhachHang KhachHang { get; set; }
        public List<ThanhToan> ThanhToans { get; set; }
        public bool CanCancel { get; set; }
    }
}

