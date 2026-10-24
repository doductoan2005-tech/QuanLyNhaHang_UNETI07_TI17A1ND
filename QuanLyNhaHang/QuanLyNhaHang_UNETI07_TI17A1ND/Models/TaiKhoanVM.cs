// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: Entity Tài khoản - Đăng nhập - Phân quyền
#nullable enable
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models
{
    public class TaiKhoanVM
    {
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 4, ErrorMessage = "Mật khẩu từ 4 đến 100 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? MatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Vai trò")]
        public VaiTro VaiTro { get; set; } = VaiTro.Admin;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;
    }
}
