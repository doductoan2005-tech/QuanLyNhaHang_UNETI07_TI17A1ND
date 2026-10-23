using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _db;
        public TaiKhoanController(ApplicationDbContext db) => _db = db;


        [HttpGet]
        public IActionResult Login(string? returnUrl, string? thongBao)
        {
            if (HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan) != null)
                return RedirectToAction("Index", "Home");

            ViewBag.ThongBao = thongBao;
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var tk = _db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == model.TenDangNhap);
            var passwordHasher = new PasswordHasher<TaiKhoan>();
            var storedPasswordIsHashed = tk?.MatKhau.StartsWith("AQAAAA", StringComparison.Ordinal) == true;
            var verificationResult = tk == null || !storedPasswordIsHashed
                ? PasswordVerificationResult.Failed
                : passwordHasher.VerifyHashedPassword(tk, tk.MatKhau, model.MatKhau);
            var passwordIsValid = tk != null && (storedPasswordIsHashed
                ? verificationResult != PasswordVerificationResult.Failed
                : string.Equals(tk.MatKhau, model.MatKhau, StringComparison.Ordinal));

            if (!passwordIsValid)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!tk.TrangThai)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa.");
                return View(model);
            }

            if (!storedPasswordIsHashed || verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                tk.MatKhau = passwordHasher.HashPassword(tk, model.MatKhau);
                _db.SaveChanges();
            }

            HttpContext.Session.SetInt32(SessionKeys.MaTaiKhoan, tk.MaTaiKhoan);
            HttpContext.Session.SetString(SessionKeys.HoTen, tk.HoTen);
            HttpContext.Session.SetString(SessionKeys.VaiTro, tk.VaiTro.ToString());

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return tk.VaiTro == VaiTro.Admin
                ? RedirectToAction("Index", "ThongKe")
                : RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult DangKy() => View(new DangKyVM());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangKy(DangKyVM model)
        {
            if (!ModelState.IsValid) return View(model);

            if (_db.TaiKhoans.Any(t => t.TenDangNhap == model.TenDangNhap))
            {
                ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<TaiKhoan>();
            var tk = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap,
                HoTen = model.HoTen,
                Email = model.Email,
                VaiTro = VaiTro.KhachHang,
                TrangThai = true
            };
            tk.MatKhau = passwordHasher.HashPassword(tk, model.MatKhau);
            _db.TaiKhoans.Add(tk);
            _db.SaveChanges();

            var kh = new KhachHang
            {
                MaTaiKhoan = tk.MaTaiKhoan,
                HoTen = model.HoTen,
                NgaySinh = model.NgaySinh,
                GioiTinh = model.GioiTinh,
                SoDienThoai = model.SoDienThoai,
                Email = model.Email,
                DiaChi = model.DiaChi,
                NgayDangKy = DateTime.Now,
                TrangThai = true
            };
            _db.KhachHangs.Add(kh);
            _db.SaveChanges();

            TempData["ThongBao"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}