namespace DuLich_Tour.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class KhachHangModelFix : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DanhGiaTours",
                c => new
                    {
                        IdDanhGia = c.Int(nullable: false, identity: true),
                        IdTour = c.Int(nullable: false),
                        IdKhachHang = c.Int(nullable: false),
                        DiemDanhGia = c.Int(nullable: false),
                        NoiDung = c.String(),
                        NgayDanhGia = c.DateTime(nullable: false),
                        HienThi = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdDanhGia)
                .ForeignKey("dbo.KhachHangs", t => t.IdKhachHang, cascadeDelete: true)
                .ForeignKey("dbo.TourDuLiches", t => t.IdTour, cascadeDelete: true)
                .Index(t => t.IdTour)
                .Index(t => t.IdKhachHang);
            
            CreateTable(
                "dbo.KhachHangs",
                c => new
                    {
                        IdKhachHang = c.Int(nullable: false, identity: true),
                        IdTaiKhoan = c.Int(nullable: false),
                        HoTen = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false, maxLength: 255),
                        SoDienThoai = c.String(maxLength: 15),
                        DiaChi = c.String(maxLength: 255),
                        NgaySinh = c.DateTime(),
                        GioiTinh = c.String(maxLength: 10),
                        NgayDangKy = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdKhachHang)
                .ForeignKey("dbo.TaiKhoans", t => t.IdTaiKhoan, cascadeDelete: true)
                .Index(t => t.IdTaiKhoan);
            
            CreateTable(
                "dbo.TaiKhoans",
                c => new
                    {
                        IdTaiKhoan = c.Int(nullable: false, identity: true),
                        TenDangNhap = c.String(nullable: false, maxLength: 100),
                        MatKhau = c.String(nullable: false, maxLength: 255),
                        VaiTro = c.String(maxLength: 20),
                        TrangThai = c.Boolean(nullable: false),
                        NgayTao = c.DateTime(nullable: false),
                        LanDangNhapCuoi = c.DateTime(),
                    })
                .PrimaryKey(t => t.IdTaiKhoan);
            
            CreateTable(
                "dbo.TourDuLiches",
                c => new
                    {
                        IdTour = c.Int(nullable: false, identity: true),
                        TenTour = c.String(nullable: false, maxLength: 255),
                        IdDiaDiem = c.Int(),
                        MoTaNgan = c.String(),
                        LichTrinh = c.String(),
                        NgayBatDau = c.DateTime(),
                        NgayKetThuc = c.DateTime(),
                        GiaNguoiLon = c.Decimal(nullable: false, precision: 18, scale: 2),
                        GiaTreEm = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SoCho = c.Int(nullable: false),
                        SoChoConLai = c.Int(nullable: false),
                        TrangThai = c.String(maxLength: 20),
                        IdKhuyenMai = c.Int(),
                        HinhAnh = c.String(),
                    })
                .PrimaryKey(t => t.IdTour)
                .ForeignKey("dbo.DiaDiemDuLiches", t => t.IdDiaDiem)
                .ForeignKey("dbo.KhuyenMais", t => t.IdKhuyenMai)
                .Index(t => t.IdDiaDiem)
                .Index(t => t.IdKhuyenMai);
            
            CreateTable(
                "dbo.DatTours",
                c => new
                    {
                        IdDatTour = c.Int(nullable: false, identity: true),
                        IdKhachHang = c.Int(nullable: false),
                        IdTour = c.Int(nullable: false),
                        NgayDat = c.DateTime(nullable: false),
                        SoNguoiLon = c.Int(nullable: false),
                        SoTreEm = c.Int(nullable: false),
                        TongTien = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TrangThai = c.String(maxLength: 30),
                        PhuongThucThanhToan = c.String(maxLength: 20),
                        MaThanhToan = c.String(),
                        GhiChu = c.String(),
                    })
                .PrimaryKey(t => t.IdDatTour)
                .ForeignKey("dbo.KhachHangs", t => t.IdKhachHang, cascadeDelete: true)
                .ForeignKey("dbo.TourDuLiches", t => t.IdTour, cascadeDelete: true)
                .Index(t => t.IdKhachHang)
                .Index(t => t.IdTour);
            
            CreateTable(
                "dbo.ThanhToans",
                c => new
                    {
                        IdThanhToan = c.Int(nullable: false, identity: true),
                        IdDatTour = c.Int(nullable: false),
                        SoTien = c.Decimal(nullable: false, precision: 18, scale: 2),
                        NgayThanhToan = c.DateTime(nullable: false),
                        TrangThai = c.String(maxLength: 20),
                        CongThanhToan = c.String(),
                    })
                .PrimaryKey(t => t.IdThanhToan)
                .ForeignKey("dbo.DatTours", t => t.IdDatTour, cascadeDelete: true)
                .Index(t => t.IdDatTour);
            
            CreateTable(
                "dbo.TheoDoiChuyenDis",
                c => new
                    {
                        IdTheoDoi = c.Int(nullable: false, identity: true),
                        IdDatTour = c.Int(nullable: false),
                        TrangThaiHienTai = c.String(),
                        ThoiGianCapNhat = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdTheoDoi)
                .ForeignKey("dbo.DatTours", t => t.IdDatTour, cascadeDelete: true)
                .Index(t => t.IdDatTour);
            
            CreateTable(
                "dbo.DiaDiemDuLiches",
                c => new
                    {
                        IdDiaDiem = c.Int(nullable: false, identity: true),
                        TenDiaDiem = c.String(nullable: false, maxLength: 255),
                        MoTa = c.String(),
                        ViTri = c.String(),
                        HinhAnh = c.String(),
                        TrangThai = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdDiaDiem);
            
            CreateTable(
                "dbo.KhuyenMais",
                c => new
                    {
                        IdKhuyenMai = c.Int(nullable: false, identity: true),
                        TenChuongTrinh = c.String(nullable: false, maxLength: 255),
                        MoTa = c.String(),
                        GiaTri = c.Decimal(precision: 18, scale: 2),
                        KieuGiam = c.String(maxLength: 20),
                        NgayBatDau = c.DateTime(),
                        NgayKetThuc = c.DateTime(),
                        TrangThai = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.IdKhuyenMai);
            
            CreateTable(
                "dbo.ThongBaos",
                c => new
                    {
                        IdThongBao = c.Int(nullable: false, identity: true),
                        IdKhachHang = c.Int(nullable: false),
                        NoiDung = c.String(),
                        Loai = c.String(maxLength: 20),
                        DaDoc = c.Boolean(nullable: false),
                        NgayGui = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.IdThongBao)
                .ForeignKey("dbo.KhachHangs", t => t.IdKhachHang, cascadeDelete: true)
                .Index(t => t.IdKhachHang);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ThongBaos", "IdKhachHang", "dbo.KhachHangs");
            DropForeignKey("dbo.DanhGiaTours", "IdTour", "dbo.TourDuLiches");
            DropForeignKey("dbo.TourDuLiches", "IdKhuyenMai", "dbo.KhuyenMais");
            DropForeignKey("dbo.TourDuLiches", "IdDiaDiem", "dbo.DiaDiemDuLiches");
            DropForeignKey("dbo.DatTours", "IdTour", "dbo.TourDuLiches");
            DropForeignKey("dbo.TheoDoiChuyenDis", "IdDatTour", "dbo.DatTours");
            DropForeignKey("dbo.ThanhToans", "IdDatTour", "dbo.DatTours");
            DropForeignKey("dbo.DatTours", "IdKhachHang", "dbo.KhachHangs");
            DropForeignKey("dbo.DanhGiaTours", "IdKhachHang", "dbo.KhachHangs");
            DropForeignKey("dbo.KhachHangs", "IdTaiKhoan", "dbo.TaiKhoans");
            DropIndex("dbo.ThongBaos", new[] { "IdKhachHang" });
            DropIndex("dbo.TheoDoiChuyenDis", new[] { "IdDatTour" });
            DropIndex("dbo.ThanhToans", new[] { "IdDatTour" });
            DropIndex("dbo.DatTours", new[] { "IdTour" });
            DropIndex("dbo.DatTours", new[] { "IdKhachHang" });
            DropIndex("dbo.TourDuLiches", new[] { "IdKhuyenMai" });
            DropIndex("dbo.TourDuLiches", new[] { "IdDiaDiem" });
            DropIndex("dbo.KhachHangs", new[] { "IdTaiKhoan" });
            DropIndex("dbo.DanhGiaTours", new[] { "IdKhachHang" });
            DropIndex("dbo.DanhGiaTours", new[] { "IdTour" });
            DropTable("dbo.ThongBaos");
            DropTable("dbo.KhuyenMais");
            DropTable("dbo.DiaDiemDuLiches");
            DropTable("dbo.TheoDoiChuyenDis");
            DropTable("dbo.ThanhToans");
            DropTable("dbo.DatTours");
            DropTable("dbo.TourDuLiches");
            DropTable("dbo.TaiKhoans");
            DropTable("dbo.KhachHangs");
            DropTable("dbo.DanhGiaTours");
        }
    }
}
