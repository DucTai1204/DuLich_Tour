using System;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required, Display(Name = "Tên đăng nhập"), StringLength(100)]
        public string ten_dang_nhap { get; set; }

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 6)]
        public string mat_khau { get; set; }

        [DataType(DataType.Password), Compare("mat_khau", ErrorMessage = "Mật khẩu không khớp")]
        public string nhap_lai_mat_khau { get; set; }

        [Required, Display(Name = "Họ và tên")]
        public string ho_ten { get; set; }

        [Required, EmailAddress]
        public string email { get; set; }

        [Phone, Display(Name = "Số điện thoại")]
        public string so_dien_thoai { get; set; }

        public string dia_chi { get; set; }

        [DataType(DataType.Date), Display(Name = "Ngày sinh")]
        public DateTime? ngay_sinh { get; set; }

        [Display(Name = "Giới tính")]
        public string gioi_tinh { get; set; }
    }

    public class LoginViewModel
    {
        [Required, Display(Name = "Tên đăng nhập")]
        public string ten_dang_nhap { get; set; }

        [Required, DataType(DataType.Password)]
        public string mat_khau { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }
    }
}