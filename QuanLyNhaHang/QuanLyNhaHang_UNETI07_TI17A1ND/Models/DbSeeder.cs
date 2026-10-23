// Nội dung thực hiện: Sinh dữ liệu mẫu (seed data) phục vụ kiểm thử

using Microsoft.AspNetCore.Identity;

namespace QuanLyNhaHang.Models
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            if (db.TaiKhoans.Any()) return; // đã có dữ liệu thì không seed lại

            var passwordHasher = new PasswordHasher<TaiKhoan>();

            // ---------- 1. Tài khoản Admin ----------
            var admin1 = new TaiKhoan { TenDangNhap = "admin", HoTen = "Quản trị viên", Email = "admin@nhahang.vn", VaiTro = VaiTro.Admin, TrangThai = true };
            admin1.MatKhau = passwordHasher.HashPassword(admin1, "123456");
            var admin2 = new TaiKhoan { TenDangNhap = "admin2", HoTen = "Quản trị viên 2", Email = "admin2@nhahang.vn", VaiTro = VaiTro.Admin, TrangThai = true };
            admin2.MatKhau = passwordHasher.HashPassword(admin2, "123456");
            db.TaiKhoans.AddRange(admin1, admin2);

            // ---------- 2. Loại bàn (4) ----------
            var loaiBans = new List<LoaiBan>
            {
                new() { TenLoaiBan = "Bàn thường", MoTa = "Bàn tiêu chuẩn cho khách lẻ", TrangThai = true },
                new() { TenLoaiBan = "Bàn VIP", MoTa = "Bàn phòng riêng cao cấp", TrangThai = true },
                new() { TenLoaiBan = "Bàn ngoài trời", MoTa = "Bàn sân vườn, view đẹp", TrangThai = true },
                new() { TenLoaiBan = "Bàn tiệc", MoTa = "Bàn lớn phục vụ tiệc/nhóm đông", TrangThai = true },
            };
            db.LoaiBans.AddRange(loaiBans);
            db.SaveChanges();

            // ---------- 3. Bàn ăn (20) ----------
            string[] viTri = { "Tầng 1", "Tầng 2", "Sân vườn", "Phòng riêng", "Ban công" };
            var banAns = new List<BanAn>();
            for (int i = 1; i <= 20; i++)
            {
                var loai = loaiBans[(i - 1) % loaiBans.Count];
                var trangThai = i % 7 == 0 ? TrangThaiBan.TamNgung : (i % 5 == 0 ? TrangThaiBan.DangSuDung : TrangThaiBan.SanSang);
                banAns.Add(new BanAn
                {
                    TenBan = $"Bàn {i:D2}",
                    MaLoaiBan = loai.MaLoaiBan,
                    SoChoNgoi = 2 + (i % 5) * 2, // 2,4,6,8,10 chỗ xoay vòng
                    ViTri = viTri[i % viTri.Length],
                    MoTa = $"Mô tả cho {("Bàn " + i.ToString("D2"))}",
                    TrangThai = trangThai
                });
            }
            db.BanAns.AddRange(banAns);

            // ---------- 4. Món ăn (20) ----------
            (string ten, string loai, decimal gia)[] monMau =
            {
                ("Gỏi cuốn tôm thịt","Khai vị",45000),("Chả giò hải sản","Khai vị",55000),
                ("Súp cua","Khai vị",40000),("Salad rau trộn","Khai vị",38000),
                ("Cơm chiên dương châu","Món chính",65000),("Cơm gà xối mỡ","Món chính",70000),
                ("Bún bò Huế","Món chính",60000),("Phở bò tái","Món chính",58000),
                ("Lẩu thái hải sản","Món chính",280000),("Lẩu gà lá é","Món chính",250000),
                ("Cá lóc nướng trui","Món chính",180000),("Tôm sú nướng muối ớt","Món chính",220000),
                ("Sườn nướng BBQ","Món chính",150000),("Gà nướng mật ong","Món chính",160000),
                ("Rau muống xào tỏi","Món xào",35000),("Đậu que xào thịt bò","Món xào",65000),
                ("Chè khúc bạch","Tráng miệng",30000),("Trái cây theo mùa","Tráng miệng",35000),
                ("Nước ép cam","Đồ uống",25000),("Trà đào cam sả","Đồ uống",28000),
            };
            var monAns = new List<MonAn>();
            for (int i = 0; i < monMau.Length; i++)
            {
                monAns.Add(new MonAn
                {
                    TenMon = monMau[i].ten,
                    LoaiMon = monMau[i].loai,
                    DonGia = monMau[i].gia,
                    MoTa = $"Món {monMau[i].ten} đặc trưng của nhà hàng",
                    TrangThai = (i % 9 == 0) ? TrangThaiMon.NgungPhucVu : TrangThaiMon.DangPhucVu
                });
            }
            db.MonAns.AddRange(monAns);

            // ---------- 5. Khách hàng (15) - mỗi khách 1 tài khoản ----------
            string[] hoTenMau =
            {
                "Nguyễn Văn An","Trần Thị Bình","Lê Hoàng Cường","Phạm Thị Dung","Hoàng Văn Em",
                "Vũ Thị Phương","Đặng Văn Giang","Bùi Thị Hoa","Ngô Văn Hùng","Đỗ Thị Lan",
                "Phan Văn Minh","Trịnh Thị Nga","Lý Văn Phát","Mai Thị Quyên","Đinh Văn Sơn"
            };
            var khachHangs = new List<KhachHang>();
            for (int i = 0; i < hoTenMau.Length; i++)
            {
                var tk = new TaiKhoan
                {
                    TenDangNhap = $"khachhang{i + 1}",
                    HoTen = hoTenMau[i],
                    Email = $"khachhang{i + 1}@gmail.com",
                    VaiTro = VaiTro.KhachHang,
                    TrangThai = i != 3 // 1 tài khoản bị khóa để test (index 3)
                };
                tk.MatKhau = passwordHasher.HashPassword(tk, "123456");
                db.TaiKhoans.Add(tk);
                db.SaveChanges(); // để lấy MaTaiKhoan

                var kh = new KhachHang
                {
                    MaTaiKhoan = tk.MaTaiKhoan,
                    HoTen = hoTenMau[i],
                    NgaySinh = new DateTime(1985 + i, (i % 12) + 1, (i % 27) + 1),
                    GioiTinh = i % 2 == 0 ? GioiTinh.Nam : GioiTinh.Nu,
                    SoDienThoai = $"09{(10000000 + i * 111):D8}",
                    Email = tk.Email,
                    DiaChi = $"{i + 1} Đường Số {i + 1}, Quận {(i % 12) + 1}, TP.HCM",
                    NgayDangKy = DateTime.Now.AddDays(-(i * 10 + 5)),
                    TrangThai = tk.TrangThai
                };
                db.KhachHangs.Add(kh);
            }
            db.SaveChanges();

            khachHangs = db.KhachHangs.ToList();

            // ---------- 6. Phiếu đặt bàn (25) ----------
            var trangThaiXoay = new[] { TrangThaiPhieu.ChoXacNhan, TrangThaiPhieu.DaXacNhan, TrangThaiPhieu.DaSuDung, TrangThaiPhieu.DaHuy };
            var rnd = new Random(2026);
            var phieus = new List<PhieuDatBan>();
            for (int i = 0; i < 25; i++)
            {
                var kh = khachHangs[i % khachHangs.Count];
                var ban = banAns[i % banAns.Count];
                var ngaySuDung = DateTime.Today.AddDays(rnd.Next(-10, 20));
                var gioBd = new TimeSpan(17 + (i % 5), (i % 2) * 30, 0); // 17:00 -> 21:30
                var gioKt = gioBd.Add(TimeSpan.FromHours(2));

                phieus.Add(new PhieuDatBan
                {
                    MaKhachHang = kh.MaKhachHang,
                    MaBan = ban.MaBan,
                    NgayDat = DateTime.Now.AddDays(-(30 - i)),
                    NgaySuDung = ngaySuDung,
                    GioBatDau = gioBd,
                    GioKetThuc = gioKt,
                    SoNguoi = Math.Min(ban.SoChoNgoi, 2 + (i % 4)),
                    GhiChu = i % 3 == 0 ? "Khách có yêu cầu bàn gần cửa sổ" : null,
                    TrangThai = trangThaiXoay[i % trangThaiXoay.Length]
                });
            }
            db.PhieuDatBans.AddRange(phieus);
            db.SaveChanges();
        }
    }
}
