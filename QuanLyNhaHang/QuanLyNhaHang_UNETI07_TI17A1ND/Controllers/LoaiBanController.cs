// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: CRUD Quản lý loại bàn (chỉ Admin)
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    [RequireAdmin]
    public class LoaiBanController : Controller
    {
        private readonly ApplicationDbContext _db;
        public LoaiBanController(ApplicationDbContext db) => _db = db;

        public IActionResult Index()
        {
            var danhSach = _db.LoaiBans.OrderBy(l => l.TenLoaiBan).ToList();
            return View(danhSach);
        }

        public IActionResult Details(int id)
        {
            var lb = _db.LoaiBans.FirstOrDefault(l => l.MaLoaiBan == id);
            if (lb == null) return NotFound();
            return View(lb);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LoaiBan model)
        {
            if (_db.LoaiBans.Any(l => l.TenLoaiBan == model.TenLoaiBan))
                ModelState.AddModelError(nameof(model.TenLoaiBan), "Tên loại bàn đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            _db.LoaiBans.Add(model);
            _db.SaveChanges();
            TempData["ThongBao"] = "Thêm loại bàn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var lb = _db.LoaiBans.Find(id);
            if (lb == null) return NotFound();
            return View(lb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, LoaiBan model)
        {
            if (id != model.MaLoaiBan) return NotFound();

            if (_db.LoaiBans.Any(l => l.TenLoaiBan == model.TenLoaiBan && l.MaLoaiBan != id))
                ModelState.AddModelError(nameof(model.TenLoaiBan), "Tên loại bàn đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            _db.LoaiBans.Update(model);
            _db.SaveChanges();
            TempData["ThongBao"] = "Cập nhật loại bàn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var lb = _db.LoaiBans.FirstOrDefault(l => l.MaLoaiBan == id);
            if (lb == null) return NotFound();

            ViewBag.ConLienKet = _db.BanAns.Any(b => b.MaLoaiBan == id);
            return View(lb);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var lb = _db.LoaiBans.Find(id);
            if (lb == null) return NotFound();

            // Không được xóa loại bàn nếu còn BanAn liên quan
            if (_db.BanAns.Any(b => b.MaLoaiBan == id))
            {
                TempData["Loi"] = "Không thể xóa: vẫn còn bàn ăn thuộc loại bàn này.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _db.LoaiBans.Remove(lb);
            _db.SaveChanges();
            TempData["ThongBao"] = "Xóa loại bàn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleTrangThai(int id)
        {
            var lb = _db.LoaiBans.Find(id);
            if (lb == null) return NotFound();
            lb.TrangThai = !lb.TrangThai;
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
