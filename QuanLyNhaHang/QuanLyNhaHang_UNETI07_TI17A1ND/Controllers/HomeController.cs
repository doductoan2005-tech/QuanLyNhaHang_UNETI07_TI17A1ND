// Nội dung thực hiện: Trang chủ
using Microsoft.AspNetCore.Mvc;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public IActionResult Index()
        {
            ViewBag.SoMonAn = _db.MonAns.Count();
            ViewBag.SoBanAn = _db.BanAns.Count();
            ViewBag.SoLoaiBan = _db.LoaiBans.Count();
            return View();
        }

        public IActionResult Error() => View();
    }
}
