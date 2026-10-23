// Họ và tên: Phạm Ngọc Hưng (Sinh viên 3)
// Mã sinh viên: 23203100037
// Nội dung thực hiện: Entity Khách hàng
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhaHang.Models
{
    public enum GioiTinh
    {
        Nam = 1,
        Nu = 2,
        Khac = 3
    }

    public class KhachHang : IValidatableObject
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required]
        [Display(Name = "Tài khoản")]
        public int MaTaiKhoan { get; set; }

        [ForeignKey(nameof(MaTaiKhoan))]
        public TaiKhoan? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }

        [Required]
        [Display(Name = "Giới tính")]
        public GioiTinh GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Số điện thoại phải gồm 10 số bắt đầu bằng 0 hoặc +84 và 9 số.")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<PhieuDatBan> DanhSachPhieu { get; set; } = new List<PhieuDatBan>();

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
