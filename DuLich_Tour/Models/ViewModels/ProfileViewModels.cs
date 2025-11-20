using System;
using System.ComponentModel.DataAnnotations;

namespace DuLich_Tour.Models.ViewModels
{
    /// <summary>
    /// ViewModel cho cập nhật thông tin cá nhân
    /// </summary>
    public class ProfileViewModel
    {
        public int IdKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(255, ErrorMessage = "Họ tên không được quá 255 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được quá 255 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại không được quá 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; } = "khac";

        [Display(Name = "Avatar")]
        public string Avatar { get; set; }
    }

    /// <summary>
    /// ViewModel cho đổi mật khẩu
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string MatKhauCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string MatKhauMoi { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string XacNhanMatKhauMoi { get; set; }
    }
}

