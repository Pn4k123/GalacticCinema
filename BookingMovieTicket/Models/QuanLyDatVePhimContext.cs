using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BookingMovieTicket.Models;

public partial class QuanLyDatVePhimContext : DbContext
{
    public QuanLyDatVePhimContext()
    {
    }

    public QuanLyDatVePhimContext(DbContextOptions<QuanLyDatVePhimContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiTietDonDatVe> ChiTietDonDatVes { get; set; }

    public virtual DbSet<DonDatVe> DonDatVes { get; set; }

    public virtual DbSet<Ghe> Ghes { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<Phim> Phims { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<Rap> Raps { get; set; }

    public virtual DbSet<SuatChieu> SuatChieus { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    public virtual DbSet<TheLoai> TheLoais { get; set; }

    public virtual DbSet<Ve> Ves { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=Pn4k;Initial Catalog=QuanLyDatVePhim;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTietDonDatVe>(entity =>
        {
            entity.HasKey(e => new { e.MaDon, e.MaVe }).HasName("PK__ChiTietD__DFFBA46806DE897A");

            entity.ToTable("ChiTietDonDatVe");

            entity.Property(e => e.MaDon)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaVe)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.GiaVe).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDonNavigation).WithMany(p => p.ChiTietDonDatVes)
                .HasForeignKey(d => d.MaDon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDo__MaDon__656C112C");

            entity.HasOne(d => d.MaVeNavigation).WithMany(p => p.ChiTietDonDatVes)
                .HasForeignKey(d => d.MaVe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietDon__MaVe__66603565");
        });

        modelBuilder.Entity<DonDatVe>(entity =>
        {
            entity.HasKey(e => e.MaDon).HasName("PK__DonDatVe__3D89F568681024C0");

            entity.ToTable("DonDatVe");

            entity.Property(e => e.MaDon)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MaND");
            entity.Property(e => e.MaThanhToan)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ThoiGianDat)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TongTien).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.DonDatVes)
                .HasForeignKey(d => d.MaNd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDatVe__MaND__403A8C7D");

            entity.HasOne(d => d.MaThanhToanNavigation).WithMany(p => p.DonDatVes)
                .HasForeignKey(d => d.MaThanhToan)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DonDatVe__MaThan__412EB0B6");
        });

        modelBuilder.Entity<Ghe>(entity =>
        {
            entity.HasKey(e => e.MaGhe).HasName("PK__Ghe__3CD3C67B6D0872F1");

            entity.ToTable("Ghe");

            entity.Property(e => e.MaGhe)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LoaiGhe).HasMaxLength(20);
            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.Ghes)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ghe__MaPhong__4CA06362");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNd).HasName("PK__NguoiDun__2725D724AC58DEA0");

            entity.ToTable("NguoiDung");

            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("MaND");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Sdt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("SDT");
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Phim>(entity =>
        {
            entity.HasKey(e => e.MaPhim).HasName("PK__Phim__4AC03DE3D2935958");

            entity.ToTable("Phim");

            entity.Property(e => e.MaPhim)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DanhGia).HasMaxLength(50);
            entity.Property(e => e.DaoDien).HasMaxLength(100);
            entity.Property(e => e.NgayPhatHanh).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Poster).HasMaxLength(100);
            entity.Property(e => e.TenPhim).HasMaxLength(200);
            entity.Property(e => e.Trailer).HasMaxLength(500);

            entity.HasMany(d => d.MaTheLoais).WithMany(p => p.MaPhims)
                .UsingEntity<Dictionary<string, object>>(
                    "TheLoaiPhim",
                    r => r.HasOne<TheLoai>().WithMany()
                        .HasForeignKey("MaTheLoai")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TheLoai_P__MaThe__5535A963"),
                    l => l.HasOne<Phim>().WithMany()
                        .HasForeignKey("MaPhim")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__TheLoai_P__MaPhi__5441852A"),
                    j =>
                    {
                        j.HasKey("MaPhim", "MaTheLoai").HasName("PK__TheLoai___F7B3C2D7953BFAA4");
                        j.ToTable("TheLoai_Phim");
                        j.IndexerProperty<string>("MaPhim")
                            .HasMaxLength(10)
                            .IsUnicode(false);
                        j.IndexerProperty<string>("MaTheLoai")
                            .HasMaxLength(10)
                            .IsUnicode(false);
                    });
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5B981B891E");

            entity.ToTable("Phong");

            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaRap)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SoPhong)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaRapNavigation).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.MaRap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Phong__MaRap__47DBAE45");
        });

        modelBuilder.Entity<Rap>(entity =>
        {
            entity.HasKey(e => e.MaRap).HasName("PK__Rap__3961207FF20B915E");

            entity.ToTable("Rap");

            entity.Property(e => e.MaRap)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.TenRap).HasMaxLength(100);
            entity.Property(e => e.TrangThai).HasMaxLength(20);
        });

        modelBuilder.Entity<SuatChieu>(entity =>
        {
            entity.HasKey(e => e.MaSuatChieu).HasName("PK__SuatChie__CF5984D205AC6EB1");

            entity.ToTable("SuatChieu");

            entity.Property(e => e.MaSuatChieu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.GioChieu).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NgayChieu).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.SuatChieus)
                .HasForeignKey(d => d.MaPhong)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SuatChieu__MaPho__5AEE82B9");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaThanhToan).HasName("PK__ThanhToa__D4B258442838FD08");

            entity.ToTable("ThanhToan");

            entity.Property(e => e.MaThanhToan)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.PhuongThuc).HasMaxLength(100);
            entity.Property(e => e.ThoiGian).HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasMaxLength(20);
        });

        modelBuilder.Entity<TheLoai>(entity =>
        {
            entity.HasKey(e => e.MaTheLoai).HasName("PK__TheLoai__D73FF34AC39A8BE0");

            entity.ToTable("TheLoai");

            entity.Property(e => e.MaTheLoai)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenTheLoai).HasMaxLength(100);
        });

        modelBuilder.Entity<Ve>(entity =>
        {
            entity.HasKey(e => e.MaVe).HasName("PK__Ve__2725100FB4FAF5E0");

            entity.ToTable("Ve");

            entity.Property(e => e.MaVe)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaGhe)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaPhim)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaRap)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MaSuatChieu)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ThoiGianPhatHanh)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaGheNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaGhe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaGhe__628FA481");

            entity.HasOne(d => d.MaPhimNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaPhim)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaPhim__5FB337D6");

            entity.HasOne(d => d.MaRapNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaRap)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaRap__60A75C0F");

            entity.HasOne(d => d.MaSuatChieuNavigation).WithMany(p => p.Ves)
                .HasForeignKey(d => d.MaSuatChieu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ve__MaSuatChieu__619B8048");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
