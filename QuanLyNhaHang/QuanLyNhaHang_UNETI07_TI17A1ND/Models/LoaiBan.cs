// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: Entity Loại bàn
using System.ComponentModel.DataAnnotations;

namespace QuanLyNhaHang.Models
{
    public class LoaiBan
    {
        [Key]
        public int MaLoaiBan { get; set; }

        [Required(ErrorMessage = "Tên loại bàn không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên loại bàn")]
        public string TenLoaiBan { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? MoTa { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        public ICollection<BanAn> DanhSachBan { get; set; } = new List<BanAn>();
    }
}
