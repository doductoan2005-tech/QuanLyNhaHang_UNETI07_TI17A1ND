// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: Entity Tài khoản - Đăng nhập - Phân quyền
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models
{
    public enum VaiTro
    {
        Admin = 1,
        KhachHang = 2
    }

    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Vai trò")]
        public VaiTro VaiTro { get; set; } = VaiTro.KhachHang;

        // true = đang hoạt động, false = bị khóa
        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public KhachHang? KhachHang { get; set; }
    }
}
