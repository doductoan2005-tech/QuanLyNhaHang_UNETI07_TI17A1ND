namespace QuanLyNhaHang.Models
{
    public class DashboardVM
    {
        public int TongSoLoaiBan { get; set; }
        public int TongSoBanAn { get; set; }
        public int TongSoMonAn { get; set; }
        public int TongSoKhachHang { get; set; }
        public int TongSoPhieu { get; set; }
        public int SoPhieuChoXacNhan { get; set; }
        public int SoPhieuDaXacNhan { get; set; }
        public int SoBanDangSuDung { get; set; }

        public List<ThongKeItem> BanTheoLoai { get; set; } = new();
        public List<ThongKeItem> BanTheoTrangThai { get; set; } = new();
        public List<ThongKeItem> PhieuTheoTrangThai { get; set; } = new();
        public List<ThongKeItem> LuotDatTheoBan { get; set; } = new();
        public List<ThongKeItem> LuotDatTheoLoaiBan { get; set; } = new();
        public List<ThongKeItem> LuotDatTheoKhachHang { get; set; } = new();
        public List<ThongKeItem> LuotDatTheoThang { get; set; } = new();
        public string? BanNhieuLuotDatNhat { get; set; }
    }

    public class ThongKeItem
    {
        public string Nhan { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
}
