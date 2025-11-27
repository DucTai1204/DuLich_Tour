using System.Data.Entity;

namespace DuLich_Tour.Models
{
    public class TourDbContext : DbContext
    {
        public TourDbContext() : base("name=TourDbContext")
        {
            // Tối ưu cho concurrent access
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = true;
            this.Database.CommandTimeout = 60; // Tăng timeout để tránh timeout khi có nhiều kết nối
        }

        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<DiaDiemDuLich> DiaDiemDuLiches { get; set; }
        // ... (các DbSet khác)
        public DbSet<KhuyenMai> KhuyenMais { get; set; }
        public DbSet<TourDuLich> TourDuLiches { get; set; }
        public DbSet<DatTour> DatTours { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }
        public DbSet<ThongBao> ThongBaos { get; set; }
        public DbSet<TheoDoiChuyenDi> TheoDoiChuyenDis { get; set; }
        public DbSet<DanhGiaTour> DanhGiaTours { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình rõ ràng mối quan hệ 1-N: 
            // KhachHang (N) có 1 TaiKhoan (1) thông qua IdTaiKhoan
            modelBuilder.Entity<KhachHang>()
                .HasRequired(k => k.TaiKhoan)
                .WithMany(t => t.KhachHangs)
                .HasForeignKey(k => k.IdTaiKhoan)
                .WillCascadeOnDelete(true);

            // ... (Thêm các cấu hình quan hệ khác nếu cần)
        }
    }
}