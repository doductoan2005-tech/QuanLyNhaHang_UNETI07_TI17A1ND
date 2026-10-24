// Họ và tên: Đỗ Đức Toàn (Sinh viên 4)
// Mã sinh viên: 23203100077
// Nội dung thực hiện: Entity Phiếu đặt bàn
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhaHang.Models
{
    public enum TrangThaiPhieu
    {
        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan = 1,
        [Display(Name = "Đã xác nhận")]
        DaXacNhan = 2,
        [Display(Name = "Đã sử dụng")]
        DaSuDung = 3,
        [Display(Name = "Đã hủy")]
        DaHuy = 4
    }

    public class PhieuDatBan
    {
        [Key]
        public int MaPhieu { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [ForeignKey(nameof(MaKhachHang))]
        public KhachHang? KhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bàn")]
        [Display(Name = "Bàn ăn")]
        public int MaBan { get; set; }

        [ForeignKey(nameof(MaBan))]
        public BanAn? BanAn { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Ngày sử dụng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sử dụng")]
        public DateTime NgaySuDung { get; set; }

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; }

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; }

        [Required(ErrorMessage = "Số người không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người phải lớn hơn 0")]
        [Display(Name = "Số người")]
        public int SoNguoi { get; set; }

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string? GhiChu { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiPhieu TrangThai { get; set; } = TrangThaiPhieu.ChoXacNhan;
    }
}
