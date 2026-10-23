#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [RequireAdmin]
    public class QuanLyTaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<TaiKhoan> _passwordHasher = new();

        public QuanLyTaiKhoanController(ApplicationDbContext db) => _db = db;

        private int? MaDangNhap => HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);

        public IActionResult Index(string? tuKhoa, VaiTro? vaiTro)
        {
            var query = _db.TaiKhoans.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                tuKhoa = tuKhoa.Trim();
                query = query.Where(t => t.TenDangNhap.Contains(tuKhoa)
                    || t.HoTen.Contains(tuKhoa) || t.Email.Contains(tuKhoa));
            }
            if (vaiTro.HasValue)
                query = query.Where(t => t.VaiTro == vaiTro.Value);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.MaDangNhap = MaDangNhap;
            return View(query.OrderBy(t => t.VaiTro).ThenBy(t => t.TenDangNhap).ToList());
        }

        public IActionResult Details(int id)
        {
            var taiKhoan = _db.TaiKhoans.Find(id);
            return taiKhoan == null ? NotFound() : View(taiKhoan);
        }

        [HttpGet]
        public IActionResult Create() => View(new TaiKhoanVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TaiKhoanVM model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau))
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không được để trống.");

            var tenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(tenDangNhap)
                && _db.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");

            if (!ModelState.IsValid)
                return View(model);

            var taiKhoan = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                VaiTro = VaiTro.Admin,
                TrangThai = model.TrangThai
            };
            taiKhoan.MatKhau = _passwordHasher.HashPassword(taiKhoan, model.MatKhau!);
            _db.TaiKhoans.Add(taiKhoan);
            _db.SaveChanges();

            TempData["ThongBao"] = "Thêm tài khoản Admin thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var taiKhoan = _db.TaiKhoans.Find(id);
            if (taiKhoan == null)
                return NotFound();

            return View(new TaiKhoanVM
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                HoTen = taiKhoan.HoTen,
                Email = taiKhoan.Email,
                VaiTro = taiKhoan.VaiTro,
                TrangThai = taiKhoan.TrangThai
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TaiKhoanVM model)
        {
            if (id != model.MaTaiKhoan)
                return NotFound();

            var taiKhoan = _db.TaiKhoans.Find(id);
            if (taiKhoan == null)
                return NotFound();

            ModelState.Remove(nameof(model.TenDangNhap));
            model.TenDangNhap = taiKhoan.TenDangNhap;
            model.VaiTro = taiKhoan.VaiTro;

            if (id == MaDangNhap && !model.TrangThai)
                ModelState.AddModelError(nameof(model.TrangThai), "Không thể tự khóa tài khoản đang đăng nhập.");

            if (!ModelState.IsValid)
                return View(model);

            taiKhoan.HoTen = model.HoTen.Trim();
            taiKhoan.Email = model.Email.Trim();
            taiKhoan.TrangThai = model.TrangThai;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                taiKhoan.MatKhau = _passwordHasher.HashPassword(taiKhoan, model.MatKhau);

            var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == id);
            if (khachHang != null)
            {
                khachHang.HoTen = taiKhoan.HoTen;
                khachHang.Email = taiKhoan.Email;
                khachHang.TrangThai = taiKhoan.TrangThai;
            }

            _db.SaveChanges();
            if (id == MaDangNhap)
                HttpContext.Session.SetString(SessionKeys.HoTen, taiKhoan.HoTen);

            TempData["ThongBao"] = "Cập nhật tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTrangThai(int id)
        {
            var taiKhoan = _db.TaiKhoans.Find(id);
            if (taiKhoan == null)
                return NotFound();

            if (id == MaDangNhap)
            {
                TempData["Loi"] = "Không thể khóa tài khoản đang đăng nhập.";
                return RedirectToAction(nameof(Index));
            }

            taiKhoan.TrangThai = !taiKhoan.TrangThai;
            var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == id);
            if (khachHang != null)
                khachHang.TrangThai = taiKhoan.TrangThai;
            _db.SaveChanges();

            TempData["ThongBao"] = taiKhoan.TrangThai ? "Đã mở khóa tài khoản." : "Đã khóa tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var taiKhoan = _db.TaiKhoans.Find(id);
            if (taiKhoan == null)
                return NotFound();

            ViewBag.LyDoChan = LyDoKhongXoa(taiKhoan);
            return View(taiKhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var taiKhoan = _db.TaiKhoans.Find(id);
            if (taiKhoan == null)
                return NotFound();

            var lyDo = LyDoKhongXoa(taiKhoan);
            if (lyDo != null)
            {
                TempData["Loi"] = lyDo;
                return RedirectToAction(nameof(Delete), new { id });
            }

            _db.TaiKhoans.Remove(taiKhoan);
            _db.SaveChanges();
            TempData["ThongBao"] = "Xóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        private string? LyDoKhongXoa(TaiKhoan taiKhoan)
        {
            if (taiKhoan.MaTaiKhoan == MaDangNhap)
                return "Không thể xóa tài khoản đang đăng nhập.";

            var khachHang = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == taiKhoan.MaTaiKhoan);
            if (khachHang != null && _db.PhieuDatBans.Any(p => p.MaKhachHang == khachHang.MaKhachHang))
                return "Không thể xóa: khách hàng này đã có phiếu đặt bàn. Hãy khóa tài khoản thay vì xóa.";

            return null;
        }
    }
}
