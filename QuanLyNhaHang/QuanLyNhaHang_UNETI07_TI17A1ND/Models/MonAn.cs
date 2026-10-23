// Họ và tên: Phạm Ngọc Hưng (Sinh viên 3)
// Mã sinh viên: 23203100037
// Nội dung thực hiện: Entity Món ăn
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyNhaHang.Models
{
    public enum TrangThaiMon
    {
        [Display(Name = "Đang phục vụ")]
        DangPhucVu = 1,
        [Display(Name = "Ngừng phục vụ")]
        NgungPhucVu = 2
    }

    public class MonAn
    {
        [Key]
        public int MaMon { get; set; }

        [Required(ErrorMessage = "Tên món không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên món")]
        public string TenMon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại món không được để trống")]
        [StringLength(50)]
        [Display(Name = "Loại món")]
        public string LoaiMon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Đơn giá không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string? MoTa { get; set; }

        [Display(Name = "Ảnh món ăn")]
        public byte[] AnhMon { get; set; }

        [StringLength(100)]
        public string AnhMonContentType { get; set; }

        [Display(Name = "Trạng thái")]
        public TrangThaiMon TrangThai { get; set; } = TrangThaiMon.DangPhucVu;
    }
}
