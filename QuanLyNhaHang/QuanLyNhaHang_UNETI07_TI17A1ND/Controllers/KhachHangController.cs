// Họ và tên: Phạm Ngọc Hưng (Sinh viên 3)
// Mã sinh viên: 23203100037
// Nội dung thực hiện: Quản lý khách hàng (Admin) + Thông tin cá nhân (Khách hàng)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _db;
        public KhachHangController(ApplicationDbContext db) => _db = db;

        // ---------------- ADMIN: danh sách + tìm kiếm ----------------
        [RequireAdmin]
        public IActionResult Index(string? tuKhoa)
        {
            var query = _db.KhachHangs.Include(k => k.TaiKhoan).AsQueryable();
            tuKhoa = tuKhoa?.Trim();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(k => k.HoTen.Contains(tuKhoa)
                    || k.Email.Contains(tuKhoa)
                    || k.SoDienThoai.Contains(tuKhoa));
            }

            ViewBag.TuKhoa = tuKhoa;
            return View(query.OrderBy(k => k.HoTen).ToList());
        }

        [RequireAdmin]
        public IActionResult Details(int id)
        {
            var kh = _db.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefault(k => k.MaKhachHang == id);
            if (kh == null) return NotFound();
            return View(kh);
        }

        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTrangThai(int id)
        {
            var kh = _db.KhachHangs.Include(k => k.TaiKhoan).FirstOrDefault(k => k.MaKhachHang == id);
            if (kh == null) return NotFound();

            kh.TrangThai = !kh.TrangThai;
            if (kh.TaiKhoan != null) kh.TaiKhoan.TrangThai = kh.TrangThai; // đồng bộ khóa tài khoản
            _db.SaveChanges();

            TempData["ThongBao"] = "Cập nhật trạng thái khách hàng thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ---------------- KHÁCH HÀNG: thông tin cá nhân ----------------
        [RequireLogin]
        public IActionResult ThongTinCaNhan()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
            var kh = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
            if (kh == null) return NotFound();
            return View(kh);
        }

        [RequireLogin, HttpGet]
        public IActionResult SuaThongTin()
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
            var kh = _db.KhachHangs.FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
            if (kh == null) return NotFound();
            return View(kh);
        }

        [RequireLogin, HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SuaThongTin(KhachHang model)
        {
            var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan)!.Value;
            var kh = _db.KhachHangs.Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.MaTaiKhoan == maTaiKhoan);
            if (kh == null) return NotFound();

            // Khách hàng chỉ được thao tác dữ liệu của chính mình
            if (kh.MaKhachHang != model.MaKhachHang) return Forbid();

            if (model.NgaySinh.Date > DateTime.Today)
                ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không được ở trong tương lai.");

            if (!ModelState.IsValid) return View(model);

            kh.HoTen = model.HoTen;
            kh.NgaySinh = model.NgaySinh;
            kh.GioiTinh = model.GioiTinh;
            kh.SoDienThoai = model.SoDienThoai;
            kh.Email = model.Email;
            kh.DiaChi = model.DiaChi;
            if (kh.TaiKhoan != null)
            {
                kh.TaiKhoan.HoTen = model.HoTen;
                kh.TaiKhoan.Email = model.Email;
                HttpContext.Session.SetString(SessionKeys.HoTen, model.HoTen);
            }
            _db.SaveChanges();

            TempData["ThongBao"] = "Cập nhật thông tin cá nhân thành công.";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }
    }
}
