// Nội dung thực hiện: DbContext - cấu hình Entity Framework Core (Code First)
using Microsoft.EntityFrameworkCore;

namespace QuanLyNhaHang.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<LoaiBan> LoaiBans { get; set; }
        public DbSet<BanAn> BanAns { get; set; }
        public DbSet<MonAn> MonAns { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<PhieuDatBan> PhieuDatBans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tên đăng nhập không trùng
            modelBuilder.Entity<TaiKhoan>()
                .HasIndex(t => t.TenDangNhap)
                .IsUnique();

            // Tên loại bàn không trùng
            modelBuilder.Entity<LoaiBan>()
                .HasIndex(l => l.TenLoaiBan)
                .IsUnique();

            // Tên bàn không trùng
            modelBuilder.Entity<BanAn>()
                .HasIndex(b => b.TenBan)
                .IsUnique();

            // TaiKhoan (1) - (1) KhachHang
            modelBuilder.Entity<KhachHang>()
                .HasIndex(k => k.MaTaiKhoan)
                .IsUnique();

            // LoaiBan (1) - (n) BanAn : không cho xóa LoaiBan nếu còn BanAn liên quan
            modelBuilder.Entity<BanAn>()
                .HasOne(b => b.LoaiBan)
                .WithMany(l => l.DanhSachBan)
                .HasForeignKey(b => b.MaLoaiBan)
                .OnDelete(DeleteBehavior.Restrict);

            // KhachHang (1) - (n) PhieuDatBan
            modelBuilder.Entity<PhieuDatBan>()
                .HasOne(p => p.KhachHang)
                .WithMany(k => k.DanhSachPhieu)
                .HasForeignKey(p => p.MaKhachHang)
                .OnDelete(DeleteBehavior.Restrict);

            // BanAn (1) - (n) PhieuDatBan
            modelBuilder.Entity<PhieuDatBan>()
                .HasOne(p => p.BanAn)
                .WithMany(b => b.DanhSachPhieu)
                .HasForeignKey(p => p.MaBan)
                .OnDelete(DeleteBehavior.Restrict);

            // TaiKhoan (1) - (1) KhachHang
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.TaiKhoan)
                .WithOne(t => t.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
