// Họ và tên: Đỗ Đức Toàn
// Mã sinh viên: 23203100077
// Nội dung thực hiện: Dashboard và Thống kê (LINQ: Count, GroupBy, OrderBy...)
using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [RequireAdmin]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ThongKeController(ApplicationDbContext db) => _db = db;

        public IActionResult Index()
        {
            var vm = new DashboardVM
            {
                TongSoLoaiBan = _db.LoaiBans.Count(),
                TongSoBanAn = _db.BanAns.Count(),
                TongSoMonAn = _db.MonAns.Count(),
                TongSoKhachHang = _db.KhachHangs.Count(),
                TongSoPhieu = _db.PhieuDatBans.Count(),
                SoPhieuChoXacNhan = _db.PhieuDatBans.Count(p => p.TrangThai == TrangThaiPhieu.ChoXacNhan),
                SoPhieuDaXacNhan = _db.PhieuDatBans.Count(p => p.TrangThai == TrangThaiPhieu.DaXacNhan),
                SoBanDangSuDung = _db.BanAns.Count(b => b.TrangThai == TrangThaiBan.DangSuDung),
            };

            // Số bàn theo từng loại
            vm.BanTheoLoai = _db.BanAns
                .GroupBy(b => b.LoaiBan!.TenLoaiBan)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            // Số bàn theo từng trạng thái
            // Nhóm + đếm bằng SQL trước (EF Core dịch tốt), rồi mới gọi enum.ToString() ở phía client
            // để tránh phụ thuộc vào khả năng dịch ToString() khác nhau giữa các phiên bản EF Core.
            vm.BanTheoTrangThai = _db.BanAns
                .GroupBy(b => b.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToList()
                .Select(x => new ThongKeItem { Nhan = x.TrangThai.ToString(), SoLuong = x.SoLuong })
                .ToList();

            // Số phiếu đặt theo từng trạng thái
            vm.PhieuTheoTrangThai = _db.PhieuDatBans
                .GroupBy(p => p.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToList()
                .Select(x => new ThongKeItem { Nhan = x.TrangThai.ToString(), SoLuong = x.SoLuong })
                .ToList();

            // Số lượt đặt theo từng bàn (top 10)
            vm.LuotDatTheoBan = _db.PhieuDatBans
                .GroupBy(p => p.BanAn!.TenBan)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Take(10)
                .ToList();

            // Bàn có số lượt đặt nhiều nhất
            vm.BanNhieuLuotDatNhat = vm.LuotDatTheoBan.FirstOrDefault()?.Nhan;

            // Số lượt đặt theo từng loại bàn
            vm.LuotDatTheoLoaiBan = _db.PhieuDatBans
                .GroupBy(p => p.BanAn!.LoaiBan!.TenLoaiBan)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            // Số lượt đặt theo từng khách hàng (top 10)
            vm.LuotDatTheoKhachHang = _db.PhieuDatBans
                .GroupBy(p => p.KhachHang!.HoTen)
                .Select(g => new ThongKeItem { Nhan = g.Key, SoLuong = g.Count() })
                .OrderByDescending(x => x.SoLuong)
                .Take(10)
                .ToList();

            // Số lượt đặt theo tháng
            // Lưu ý: GroupBy theo Year/Month rồi Select trực tiếp thành chuỗi tiếng Việt + OrderBy chuỗi đó
            // khiến EF Core không dịch được sang SQL (InvalidOperationException: could not be translated).
            // => Tách làm 2 bước: (1) lấy dữ liệu thô Year/Month/SoLuong từ SQL Server (EF Core dịch được),
            //    (2) sắp xếp theo Year/Month và định dạng chuỗi hiển thị ở phía client (trong bộ nhớ .NET).
            var theoThangTho = _db.PhieuDatBans
                .GroupBy(p => new { p.NgaySuDung.Year, p.NgaySuDung.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, SoLuong = g.Count() })
                .ToList();

            vm.LuotDatTheoThang = theoThangTho
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(x => new ThongKeItem { Nhan = $"Tháng {x.Month}/{x.Year}", SoLuong = x.SoLuong })
                .ToList();

            return View(vm);
        }
    }
}
