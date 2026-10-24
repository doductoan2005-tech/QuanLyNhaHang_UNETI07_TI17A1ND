
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models
{
    public class DatBanVM
    {
        [Required(ErrorMessage = "Vui lòng chọn bàn")]
        [Display(Name = "Bàn ăn")]
        public int MaBan { get; set; }

        [Required(ErrorMessage = "Ngày sử dụng không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sử dụng")]
        public DateTime NgaySuDung { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Giờ bắt đầu không được để trống")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; } = new TimeSpan(18, 0, 0);

        [Required(ErrorMessage = "Giờ kết thúc không được để trống")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; } = new TimeSpan(20, 0, 0);

        [Required(ErrorMessage = "Số người không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số người phải lớn hơn 0")]
        [Display(Name = "Số người")]
        public int SoNguoi { get; set; } = 2;

        [StringLength(300)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public List<BanAn>? DanhSachBan { get; set; }
    }
}
