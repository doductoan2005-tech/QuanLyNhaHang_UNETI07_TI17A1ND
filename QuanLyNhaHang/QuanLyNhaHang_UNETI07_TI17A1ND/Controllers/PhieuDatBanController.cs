// Họ và tên: Đỗ Đức Toàn
// Mã sinh viên: 23203100077
// Nội dung thực hiện: Đặt bàn - Kiểm tra trùng lịch - Xác nhận/Hủy - Cập nhật sử dụng bàn
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class PhieuDatBanController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PhieuDatBanController(ApplicationDbContext db) => _db = db;

        // ============  PHÍA KHÁCH HÀNG  =========================

        [RequireLogin, HttpGet]
        public IActionResult Create(int? maBan)
        {
            var vm = new DatBanVM
            {
                MaBan = maBan ?? 0,
                DanhSachBan = _db.BanAns.Include(b => b.LoaiBan)
                    .Where(b => b.TrangThai != TrangThaiBan.TamNgung)
                    .OrderBy(b => b.TenBan).ToList()
            };
            return View(vm);
        }

        [RequireLogin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DatBanVM model)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
            var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
            var ban = _db.BanAns.Find(model.MaBan);

            KiemTraNghiepVuDatBan(model.NgaySuDung, model.GioBatDau, model.GioKetThuc,
                model.SoNguoi, ban, khachHang, maPhieuDangSua: null);

            if (!ModelState.IsValid)
            {
                model.DanhSachBan = _db.BanAns.Include(b => b.LoaiBan)
                    .Where(b => b.TrangThai != TrangThaiBan.TamNgung).OrderBy(b => b.TenBan).ToList();
                return View(model);
            }

            var phieu = new PhieuDatBan
            {
                MaKhachHang = khachHang!.MaKhachHang,
                MaBan = model.MaBan,
                NgayDat = DateTime.Now, // hệ thống tự xác định
                NgaySuDung = model.NgaySuDung,
                GioBatDau = model.GioBatDau,
                GioKetThuc = model.GioKetThuc,
                SoNguoi = model.SoNguoi,
                GhiChu = model.GhiChu,
                TrangThai = TrangThaiPhieu.ChoXacNhan
            };
            _db.PhieuDatBans.Add(phieu);
            _db.SaveChanges();

            TempData["ThongBao"] = "Đặt bàn thành công. Vui lòng chờ nhà hàng xác nhận.";
            return RedirectToAction(nameof(LichSu));
        }

        [RequireLogin]
        public IActionResult LichSu(TrangThaiPhieu? trangThai)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
            var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
            if (khachHang == null) return NotFound();

            var query = _db.PhieuDatBans.Include(p => p.BanAn)
                .Where(p => p.MaKhachHang == khachHang.MaKhachHang);

            if (trangThai.HasValue)
                query = query.Where(p => p.TrangThai == trangThai.Value);

            ViewBag.TrangThaiLoc = trangThai;
            return View(query.OrderByDescending(p => p.NgayDat).ToList());
        }

        [RequireLogin]
        public IActionResult Details(int id)
        {
            var phieu = _db.PhieuDatBans.Include(p => p.BanAn).ThenInclude(b => b!.LoaiBan)
                .Include(p => p.KhachHang)
                .FirstOrDefault(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();

            var vaiTro = HttpContext.Session.GetString(SessionKeys.VaiTro);
            if (vaiTro != VaiTro.Admin.ToString())
            {
                // Khách hàng chỉ được xem phiếu của chính mình
                var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
                var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
                if (khachHang == null || phieu.MaKhachHang != khachHang.MaKhachHang)
                    return Forbid();
            }

            return View(phieu);
        }

        // =========================================================
        // ================  PHÍA ADMIN  ==========================
        // =========================================================

        [RequireAdmin]
        public IActionResult Index(string? tenKhach, string? tenBan, TrangThaiPhieu? trangThai, DateTime? ngaySuDung)
        {
            tenKhach = tenKhach?.Trim();
            tenBan = tenBan?.Trim();
            var query = _db.PhieuDatBans
                .Include(p => p.KhachHang)
                .Include(p => p.BanAn)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tenKhach))
                query = query.Where(p => p.KhachHang!.HoTen.Contains(tenKhach));

            if (!string.IsNullOrWhiteSpace(tenBan))
                query = query.Where(p => p.BanAn!.TenBan.Contains(tenBan));

            if (trangThai.HasValue)
                query = query.Where(p => p.TrangThai == trangThai.Value);

            if (ngaySuDung.HasValue)
                query = query.Where(p => p.NgaySuDung.Date == ngaySuDung.Value.Date);

            ViewBag.TenKhach = tenKhach;
            ViewBag.TenBan = tenBan;
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.NgaySuDungLoc = ngaySuDung?.ToString("yyyy-MM-dd");

            return View(query.OrderByDescending(p => p.NgayDat).ToList());
        }

        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XacNhan(int id)
        {
            var phieu = _db.PhieuDatBans.Include(p => p.BanAn).Include(p => p.KhachHang)
                .FirstOrDefault(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.ChoXacNhan)
            {
                TempData["Loi"] = "Không thể xác nhận phiếu đã hủy hoặc đã sử dụng.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra lại toàn bộ điều kiện nghiệp vụ trước khi xác nhận
            KiemTraNghiepVuDatBan(phieu.NgaySuDung, phieu.GioBatDau, phieu.GioKetThuc,
                phieu.SoNguoi, phieu.BanAn, phieu.KhachHang, maPhieuDangSua: phieu.MaPhieu);

            if (!ModelState.IsValid)
            {
                TempData["Loi"] = "Không thể xác nhận: " + string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return RedirectToAction(nameof(Index));
            }

            phieu.TrangThai = TrangThaiPhieu.DaXacNhan;
            _db.SaveChanges();
            TempData["ThongBao"] = "Xác nhận đặt bàn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Huy(int id)
        {
            var phieu = _db.PhieuDatBans.Find(id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai == TrangThaiPhieu.DaSuDung || phieu.TrangThai == TrangThaiPhieu.DaHuy)
            {
                TempData["Loi"] = "Không thể hủy phiếu ở trạng thái hiện tại.";
                return RedirectToAction(nameof(Index));
            }

            phieu.TrangThai = TrangThaiPhieu.DaHuy;
            _db.SaveChanges();
            TempData["ThongBao"] = "Hủy đặt bàn thành công.";
            return RedirectToAction(nameof(Index));
        }

        // Khách đến sử dụng bàn theo phiếu đã xác nhận
        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BatDauSuDung(int id)
        {
            var phieu = _db.PhieuDatBans.Include(p => p.BanAn).FirstOrDefault(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.DaXacNhan)
            {
                TempData["Loi"] = "Chỉ có thể cập nhật sử dụng bàn cho phiếu đã xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            if (phieu.BanAn != null) phieu.BanAn.TrangThai = TrangThaiBan.DangSuDung;
            _db.SaveChanges();
            TempData["ThongBao"] = "Đã cập nhật bàn sang trạng thái Đang sử dụng.";
            return RedirectToAction(nameof(Index));
        }

        // Kết thúc lượt sử dụng: phiếu -> Đã sử dụng, bàn -> Sẵn sàng
        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KetThucSuDung(int id)
        {
            var phieu = _db.PhieuDatBans.Include(p => p.BanAn).FirstOrDefault(p => p.MaPhieu == id);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai != TrangThaiPhieu.DaXacNhan)
            {
                TempData["Loi"] = "Chỉ có thể hoàn tất phiếu đã xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            phieu.TrangThai = TrangThaiPhieu.DaSuDung;

            // Trả bàn về Sẵn sàng nếu không còn nghiệp vụ nào khác đang chiếm bàn
            if (phieu.BanAn != null)
            {
                bool conPhieuKhacDangDung = _db.PhieuDatBans.Any(p =>
                    p.MaBan == phieu.MaBan && p.MaPhieu != phieu.MaPhieu &&
                    p.TrangThai == TrangThaiPhieu.DaXacNhan &&
                    p.NgaySuDung.Date == DateTime.Today);

                if (!conPhieuKhacDangDung)
                    phieu.BanAn.TrangThai = TrangThaiBan.SanSang;
            }

            _db.SaveChanges();
            TempData["ThongBao"] = "Đã hoàn tất lượt sử dụng bàn.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // ==========  KIỂM TRA NGHIỆP VỤ ĐẶT BÀN  =================
        // =========================================================
        private void KiemTraNghiepVuDatBan(DateTime ngaySuDung, TimeSpan gioBatDau, TimeSpan gioKetThuc,
            int soNguoi, BanAn? ban, KhachHang? khachHang, int? maPhieuDangSua)
        {
            if (khachHang == null || !khachHang.TrangThai)
                ModelState.AddModelError(string.Empty, "Khách hàng không tồn tại hoặc không còn hoạt động.");

            if (ban == null || ban.TrangThai == TrangThaiBan.TamNgung)
                ModelState.AddModelError(string.Empty, "Bàn không tồn tại hoặc đang tạm ngừng phục vụ.");

            if (ngaySuDung.Date < DateTime.Today)
                ModelState.AddModelError(string.Empty, "Ngày sử dụng không được trước ngày hiện tại.");

            if (ngaySuDung.Date == DateTime.Today && gioBatDau <= DateTime.Now.TimeOfDay)
                ModelState.AddModelError(string.Empty, "Giờ bắt đầu phải sau thời điểm hiện tại.");

            if (gioKetThuc <= gioBatDau)
                ModelState.AddModelError(string.Empty, "Giờ kết thúc phải sau giờ bắt đầu.");

            if (soNguoi <= 0)
                ModelState.AddModelError(string.Empty, "Số người phải lớn hơn 0.");

            if (ban != null && soNguoi > ban.SoChoNgoi)
                ModelState.AddModelError(string.Empty, "Số người vượt quá sức chứa của bàn.");

            // ----- Kiểm tra trùng lịch bằng Entity Framework Core + LINQ -----
            // Bàn không được có 2 phiếu còn hiệu lực (khác Đã hủy) trùng khoảng thời gian trong cùng 1 ngày
            if (ban != null)
            {
                bool trungLich = _db.PhieuDatBans.Any(p =>
                    p.MaBan == ban.MaBan &&
                    p.MaPhieu != (maPhieuDangSua ?? -1) &&
                    p.TrangThai != TrangThaiPhieu.DaHuy &&
                    p.NgaySuDung.Date == ngaySuDung.Date &&
                    p.GioBatDau < gioKetThuc && gioBatDau < p.GioKetThuc);

                if (trungLich)
                    ModelState.AddModelError(string.Empty, "Bàn đã có phiếu đặt khác trùng khoảng thời gian này.");
            }
        }
    }
}
