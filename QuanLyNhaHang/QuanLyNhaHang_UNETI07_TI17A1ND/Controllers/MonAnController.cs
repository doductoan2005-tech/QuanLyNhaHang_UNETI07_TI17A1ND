// Họ và tên: Phạm Ngọc Hưng (Sinh viên 3)
// Mã sinh viên: 23203100037
// Nội dung thực hiện: Quản lý món ăn (thực đơn)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuanLyNhaHang.Models;
using System.Globalization;
using System.Text;

namespace QuanLyNhaHang.Controllers
{
    public class MonAnController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;

        public MonAnController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        // Thực đơn công khai - khách hàng chỉ thấy món đang phục vụ
        public IActionResult Index()
        {
            var vaiTro = HttpContext.Session.GetString(SessionKeys.VaiTro);
            var query = _db.MonAns.AsQueryable();

            if (vaiTro != VaiTro.Admin.ToString())
                query = query.Where(m => m.TrangThai == TrangThaiMon.DangPhucVu);

            var danhSach = query.OrderBy(m => m.LoaiMon).ThenBy(m => m.TenMon)
                .Select(m => new MonAn
                {
                    MaMon = m.MaMon,
                    TenMon = m.TenMon,
                    LoaiMon = m.LoaiMon,
                    DonGia = m.DonGia,
                    MoTa = m.MoTa,
                    TrangThai = m.TrangThai
                })
                .ToList();
            return View(danhSach);
        }

        [HttpGet]
        public IActionResult Image(int id)
        {
            var imageFolder = GetImageFolder();
            if (Directory.Exists(imageFolder))
            {
                var imagePath = Directory.EnumerateFiles(imageFolder, $"{id}-*")
                    .FirstOrDefault();
                if (imagePath != null)
                {
                    var contentType = GetImageContentType(imagePath);
                    if (contentType != null)
                    {
                        Response.Headers.CacheControl = "no-cache, no-store";
                        return PhysicalFile(imagePath, contentType);
                    }
                }
            }

            // Keep existing database images available for records created before folder storage.
            var image = _db.MonAns.AsNoTracking()
                .Where(m => m.MaMon == id)
                .Select(m => new { m.TenMon, m.AnhMon, m.AnhMonContentType })
                .FirstOrDefault();

            if (image == null)
                return NotFound();

            if (image.AnhMon?.Length > 0)
            {
                Response.Headers.CacheControl = "no-cache, no-store";
                return File(image.AnhMon, image.AnhMonContentType ?? "image/png");
            }

            var staticImageFolder = GetStaticImageFolder();
            if (Directory.Exists(staticImageFolder))
            {
                var imageSlug = Slugify(image.TenMon);
                if (imageSlug == "dau-que-xao-thit-bo")
                    imageSlug = "dau-dua-xao-thit-bo";

                foreach (var staticImagePath in Directory.EnumerateFiles(staticImageFolder))
                {
                    if (Slugify(Path.GetFileNameWithoutExtension(staticImagePath)) != imageSlug)
                        continue;

                    var contentType = GetImageContentType(staticImagePath);
                    if (contentType == null)
                        continue;

                    Response.Headers.CacheControl = "no-cache, no-store";
                    return PhysicalFile(staticImagePath, contentType);
                }
            }

            return Redirect(Url.Content("~/images/logo-zaku.png")!);
        }

        public IActionResult Details(int id)
        {
            var mon = _db.MonAns.Find(id);
            if (mon == null) return NotFound();

            var laAdmin = HttpContext.Session.GetString(SessionKeys.VaiTro) == VaiTro.Admin.ToString();
            if (!laAdmin && mon.TrangThai != TrangThaiMon.DangPhucVu)
                return NotFound();

            return View(mon);
        }

        [RequireAdmin, HttpGet]
        public IActionResult Create() => View();

        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MonAn model, IFormFile anhMonFile)
        {
            await SetUploadedImageAsync(model, anhMonFile);
            if (!ModelState.IsValid) return View(model);
            _db.MonAns.Add(model);
            _db.SaveChanges();
            await SaveImageCopyAsync(model);
            TempData["ThongBao"] = "Thêm món ăn thành công.";
            return RedirectToAction(nameof(Index));
        }

        [RequireAdmin, HttpGet]
        public IActionResult Edit(int id)
        {
            var mon = _db.MonAns.Find(id);
            if (mon == null) return NotFound();
            return View(mon);
        }

        [RequireAdmin, HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MonAn model, IFormFile anhMonFile, bool xoaAnh)
        {
            if (id != model.MaMon) return NotFound();

            var mon = _db.MonAns.Find(id);
            if (mon == null) return NotFound();
            var tenMonCu = mon.TenMon;

            await SetUploadedImageAsync(mon, anhMonFile);
            if (xoaAnh && anhMonFile == null)
            {
                mon.AnhMon = null;
                mon.AnhMonContentType = null;
            }

            if (!ModelState.IsValid)
            {
                model.AnhMon = mon.AnhMon;
                model.AnhMonContentType = mon.AnhMonContentType;
                return View(model);
            }

            mon.TenMon = model.TenMon;
            mon.LoaiMon = model.LoaiMon;
            mon.DonGia = model.DonGia;
            mon.MoTa = model.MoTa;
            mon.TrangThai = model.TrangThai;
            _db.SaveChanges();

            if (anhMonFile != null || xoaAnh || tenMonCu != mon.TenMon)
            {
                if (mon.AnhMon == null || mon.AnhMon.Length == 0)
                    DeleteImageCopies(mon.MaMon);
                else
                    await SaveImageCopyAsync(mon);
            }

            TempData["ThongBao"] = "Cập nhật món ăn thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task SetUploadedImageAsync(MonAn mon, IFormFile file)
        {
            if (file == null) return;

            if (file.Length == 0 || file.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("AnhMonFile", "Ảnh phải có dung lượng từ 1 byte đến 5 MB.");
                return;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => null
            };

            if (contentType == null)
            {
                ModelState.AddModelError("AnhMonFile", "Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.");
                return;
            }

            await using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            var imageBytes = stream.ToArray();
            if (!HasValidImageSignature(imageBytes, extension))
            {
                ModelState.AddModelError("AnhMonFile", "Nội dung file không phải ảnh hợp lệ theo định dạng đã chọn.");
                return;
            }

            mon.AnhMon = imageBytes;
            mon.AnhMonContentType = contentType;
        }

        private async Task SaveImageCopyAsync(MonAn mon)
        {
            if (mon.AnhMon == null || mon.AnhMon.Length == 0) return;

            var extension = mon.AnhMonContentType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => null
            };
            if (extension == null) return;

            var folder = GetImageFolder();
            Directory.CreateDirectory(folder);
            DeleteImageCopies(mon.MaMon);

            var fileName = $"{mon.MaMon}-{Slugify(mon.TenMon)}{extension}";
            var filePath = Path.Combine(folder, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, mon.AnhMon);
        }

        private void DeleteImageCopies(int maMon)
        {
            var folder = GetImageFolder();
            if (!Directory.Exists(folder)) return;

            foreach (var filePath in Directory.GetFiles(folder, $"{maMon}-*"))
                System.IO.File.Delete(filePath);
        }

        private string GetImageFolder()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(GetStaticImageFolder(), "monan");
        }

        private string GetStaticImageFolder()
        {
            var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(webRoot, "images");
        }

        private static string? GetImageContentType(string imagePath)
        {
            return Path.GetExtension(imagePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => null
            };
        }

        private static string Slugify(string value)
        {
            var normalized = value.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder();

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                var normalizedCharacter = character is 'đ' or 'Đ' ? 'd' : char.ToLowerInvariant(character);
                if (char.IsLetterOrDigit(normalizedCharacter))
                    result.Append(normalizedCharacter);
                else if (result.Length > 0 && result[result.Length - 1] != '-')
                    result.Append('-');
            }

            return result.ToString().Trim('-');
        }

        private static bool HasValidImageSignature(byte[] bytes, string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF,
                ".png" => bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A,
                ".webp" => bytes.Length >= 12 && bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50,
                _ => false
            };
        }

        [RequireAdmin, HttpGet]
        public IActionResult Delete(int id)
        {
            var mon = _db.MonAns.Find(id);
            if (mon == null) return NotFound();
            return View(mon);
        }

        [RequireAdmin, HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var mon = _db.MonAns.Find(id);
            if (mon == null) return NotFound();
            _db.MonAns.Remove(mon);
            _db.SaveChanges();
            DeleteImageCopies(mon.MaMon);
            TempData["ThongBao"] = "Xóa món ăn thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
