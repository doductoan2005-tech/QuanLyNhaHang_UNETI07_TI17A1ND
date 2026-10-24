// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: Entity Tài khoản - Đăng nhập - Phân quyền
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models
{
    // Dùng khi khách hàng tự đăng ký tài khoản
    public class DangKyVM : IValidatableObject
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; } = DateTime.Today.AddYears(-20);

        [Required]
        [Display(Name = "Giới tính")]
        public GioiTinh GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone]
        [RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số bắt đầu bằng 0 hoặc +84 và 9 số.")]
        [StringLength(12)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (NgaySinh.Date < new DateTime(1900, 1, 1) || NgaySinh.Date > DateTime.Today)
            {
                yield return new ValidationResult(
                    "Ngày sinh phải từ 01/01/1900 đến ngày hiện tại.",
                    new[] { nameof(NgaySinh) });
            }
        }
    }
}
