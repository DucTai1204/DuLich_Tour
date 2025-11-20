using System;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho form thanh toán
    /// </summary>
    public class PaymentViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
        [Display(Name = "Số tiền")]
        public decimal SoTien { get; set; }

        [MaxLength(20)]
        [Display(Name = "Cổng thanh toán")]
        public string CongThanhToan { get; set; } = "vnpay";

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }
    }
}

