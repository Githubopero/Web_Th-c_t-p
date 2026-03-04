using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace WebCauHinhXe.Models;

public partial class TtContext : DbContext
{
    public TtContext()
    {
    }

    public TtContext(DbContextOptions<TtContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<ModelOption> ModelOptions { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<OptionCategory> OptionCategories { get; set; }

    public virtual DbSet<OptionDependency> OptionDependencies { get; set; }

    public virtual DbSet<Series> Series { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=tt;user=root;password=ms0388@", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.5.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("activity_logs");

            entity.Property(e => e.Id)
                .HasComment("ID log")
                .HasColumnName("id");
            entity.Property(e => e.ChiTiet)
                .HasComment("Chi tiết hành động dạng JSON")
                .HasColumnType("json")
                .HasColumnName("chi_tiet");
            entity.Property(e => e.DiaChiIp)
                .HasMaxLength(45)
                .HasComment("IP người dùng")
                .HasColumnName("dia_chi_ip");
            entity.Property(e => e.HanhDong)
                .HasMaxLength(100)
                .HasComment("Tên hành động (tao_cau_hinh, cap_nhat_tuy_chon...)")
                .HasColumnName("hanh_dong");
            entity.Property(e => e.IdThucThe)
                .HasComment("ID đối tượng")
                .HasColumnName("id_thuc_the");
            entity.Property(e => e.LoaiThucThe)
                .HasMaxLength(50)
                .HasComment("Loại đối tượng (mau_xe, tuy_chon...)")
                .HasColumnName("loai_thuc_the");
            entity.Property(e => e.NgayThucHien)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian thực hiện")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_thuc_hien");
            entity.Property(e => e.NguoiDungId)
                .HasComment("ID người thực hiện")
                .HasColumnName("nguoi_dung_id");
            entity.Property(e => e.TrinhDuyet)
                .HasMaxLength(500)
                .HasComment("User agent")
                .HasColumnName("trinh_duyet");
        });

        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("configurations");

            entity.HasIndex(e => e.MaChiaSe, "ma_chia_se").IsUnique();

            entity.HasIndex(e => e.MauXeId, "mau_xe_id");

            entity.HasIndex(e => e.NguoiDungId, "nguoi_dung_id");

            entity.Property(e => e.Id)
                .HasComment("ID cấu hình")
                .HasColumnName("id");
            entity.Property(e => e.CongKhai)
                .HasDefaultValueSql("'0'")
                .HasComment("1 = công khai, ai cũng xem được")
                .HasColumnName("cong_khai");
            entity.Property(e => e.DonViTienTe)
                .HasMaxLength(10)
                .HasDefaultValueSql("'VND'")
                .HasComment("Đơn vị tiền tệ")
                .HasColumnName("don_vi_tien_te");
            entity.Property(e => e.MaChiaSe)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasComment("Mã để chia sẻ link cấu hình")
                .HasColumnName("ma_chia_se");
            entity.Property(e => e.MauXeId)
                .HasComment("ID mẫu xe được chọn")
                .HasColumnName("mau_xe_id");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian cập nhật")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_cap_nhat");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NguoiDungId)
                .HasComment("ID người dùng (NULL nếu khách vãng lai)")
                .HasColumnName("nguoi_dung_id");
            entity.Property(e => e.TenCauHinh)
                .HasMaxLength(150)
                .HasDefaultValueSql("'Xe mơ ước của tôi'")
                .HasComment("Tên người dùng đặt cho cấu hình")
                .HasColumnName("ten_cau_hinh");
            entity.Property(e => e.TongGia)
                .HasPrecision(12, 2)
                .HasComment("Tổng giá sau khi tùy chỉnh")
                .HasColumnName("tong_gia");
            entity.Property(e => e.TuyChonDaChon)
                .HasComment("Mảng ID tùy chọn đã chọn [3,15,27,...]")
                .HasColumnType("json")
                .HasColumnName("tuy_chon_da_chon");

            entity.HasOne(d => d.MauXe).WithMany(p => p.Configurations)
                .HasForeignKey(d => d.MauXeId)
                .HasConstraintName("configurations_ibfk_2");

            entity.HasOne(d => d.NguoiDung).WithMany(p => p.Configurations)
                .HasForeignKey(d => d.NguoiDungId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("configurations_ibfk_1");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("images");

            entity.Property(e => e.Id)
                .HasComment("ID ảnh")
                .HasColumnName("id");
            entity.Property(e => e.DuongDanAnh)
                .HasMaxLength(500)
                .HasComment("Link ảnh")
                .HasColumnName("duong_dan_anh");
            entity.Property(e => e.GocChup)
                .HasMaxLength(50)
                .HasComment("Góc chụp: front, rear, side, interior...")
                .HasColumnName("goc_chup");
            entity.Property(e => e.IdThucThe)
                .HasComment("ID của dòng/mẫu/tùy chọn")
                .HasColumnName("id_thuc_the");
            entity.Property(e => e.LoaiThucThe)
                .HasComment("Loại: dòng xe, mẫu xe hay tùy chọn")
                .HasColumnType("enum('dong_xe','mau_xe','tuy_chon')")
                .HasColumnName("loai_thuc_the");
            entity.Property(e => e.MaMau)
                .HasMaxLength(20)
                .HasComment("Mã màu liên quan (dùng filter theo màu sơn)")
                .HasColumnName("ma_mau");
            entity.Property(e => e.MoTaAnh)
                .HasMaxLength(255)
                .HasComment("Mô tả ảnh (alt text)")
                .HasColumnName("mo_ta_anh");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.ThuTu)
                .HasDefaultValueSql("'0'")
                .HasComment("Thứ tự hiển thị")
                .HasColumnName("thu_tu");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("models");

            entity.HasIndex(e => e.DongXeId, "dong_xe_id");

            entity.HasIndex(e => e.DuongDanSlug, "duong_dan_slug").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("ID mẫu xe tự tăng")
                .HasColumnName("id");
            entity.Property(e => e.AnhDaiDien)
                .HasMaxLength(500)
                .HasComment("Link ảnh đại diện mẫu xe")
                .HasColumnName("anh_dai_dien");
            entity.Property(e => e.DongXeId)
                .HasComment("ID dòng xe mà mẫu này thuộc về")
                .HasColumnName("dong_xe_id");
            entity.Property(e => e.DuongDanSlug)
                .HasMaxLength(150)
                .HasComment("Slug dùng cho URL")
                .HasColumnName("duong_dan_slug");
            entity.Property(e => e.GiaCoBan)
                .HasPrecision(12, 2)
                .HasComment("Giá khởi điểm (MSRP)")
                .HasColumnName("gia_co_ban");
            entity.Property(e => e.MoTa)
                .HasComment("Mô tả chi tiết mẫu xe")
                .HasColumnType("text")
                .HasColumnName("mo_ta");
            entity.Property(e => e.NamSanXuat)
                .HasDefaultValueSql("'2026'")
                .HasComment("Năm sản xuất/model year")
                .HasColumnType("year")
                .HasColumnName("nam_san_xuat");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenMauXe)
                .HasMaxLength(150)
                .HasComment("Tên mẫu xe (ví dụ: 330i Sedan, X5 xDrive40i)")
                .HasColumnName("ten_mau_xe");
            entity.Property(e => e.ThongSoKyThuat)
                .HasComment("Thông số kỹ thuật dạng JSON (động cơ, công suất, kích thước...)")
                .HasColumnType("json")
                .HasColumnName("thong_so_ky_thuat");
            entity.Property(e => e.TrangThaiHoatDong)
                .HasDefaultValueSql("'1'")
                .HasComment("1 = hiển thị, 0 = ẩn")
                .HasColumnName("trang_thai_hoat_dong");

            entity.HasOne(d => d.DongXe).WithMany(p => p.Models)
                .HasForeignKey(d => d.DongXeId)
                .HasConstraintName("models_ibfk_1");
        });

        modelBuilder.Entity<ModelOption>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("model_options");

            entity.HasIndex(e => e.MauXeId, "mau_xe_id");

            entity.HasIndex(e => e.TuyChonId, "tuy_chon_id");

            entity.Property(e => e.Id)
                .HasComment("ID liên kết")
                .HasColumnName("id");
            entity.Property(e => e.BatBuoc)
                .HasDefaultValueSql("'0'")
                .HasComment("1 = bắt buộc phải chọn")
                .HasColumnName("bat_buoc");
            entity.Property(e => e.DuocChonMacDinh)
                .HasDefaultValueSql("'0'")
                .HasComment("1 = được chọn mặc định")
                .HasColumnName("duoc_chon_mac_dinh");
            entity.Property(e => e.GhiChuTuongThich)
                .HasComment("Ghi chú về tương thích (ví dụ: Yêu cầu gói M Sport)")
                .HasColumnType("text")
                .HasColumnName("ghi_chu_tuong_thich");
            entity.Property(e => e.MauXeId)
                .HasComment("ID mẫu xe")
                .HasColumnName("mau_xe_id");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TuyChonId)
                .HasComment("ID tùy chọn")
                .HasColumnName("tuy_chon_id");

            entity.HasOne(d => d.MauXe).WithMany(p => p.ModelOptions)
                .HasForeignKey(d => d.MauXeId)
                .HasConstraintName("model_options_ibfk_1");

            entity.HasOne(d => d.TuyChon).WithMany(p => p.ModelOptions)
                .HasForeignKey(d => d.TuyChonId)
                .HasConstraintName("model_options_ibfk_2");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("options");

            entity.HasIndex(e => e.NhomTuyChonId, "nhom_tuy_chon_id");

            entity.Property(e => e.Id)
                .HasComment("ID tùy chọn tự tăng")
                .HasColumnName("id");
            entity.Property(e => e.AnhMoTa)
                .HasMaxLength(500)
                .HasComment("Link ảnh minh họa tùy chọn")
                .HasColumnName("anh_mo_ta");
            entity.Property(e => e.DuongDanSlug)
                .HasMaxLength(255)
                .HasComment("Slug duy nhất trong nhóm")
                .HasColumnName("duong_dan_slug");
            entity.Property(e => e.GiaThem)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("'0.00'")
                .HasComment("Giá tăng thêm khi chọn (0 = miễn phí)")
                .HasColumnName("gia_them");
            entity.Property(e => e.LaMacDinh)
                .HasDefaultValueSql("'0'")
                .HasComment("1 = được chọn mặc định")
                .HasColumnName("la_mac_dinh");
            entity.Property(e => e.MaMau)
                .HasMaxLength(20)
                .HasComment("Mã màu sơn (hex hoặc mã BMW)")
                .HasColumnName("ma_mau");
            entity.Property(e => e.MoTa)
                .HasComment("Mô tả chi tiết tùy chọn")
                .HasColumnType("text")
                .HasColumnName("mo_ta");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NhomTuyChonId)
                .HasComment("ID nhóm mà tùy chọn này thuộc về")
                .HasColumnName("nhom_tuy_chon_id");
            entity.Property(e => e.TenTuyChon)
                .HasMaxLength(255)
                .HasComment("Tên tùy chọn (Alpine White, 19\" M Double-spoke...)")
                .HasColumnName("ten_tuy_chon");
            entity.Property(e => e.TonKho)
                .HasDefaultValueSql("'999'")
                .HasComment("Số lượng tồn kho (nếu áp dụng)")
                .HasColumnName("ton_kho");
            entity.Property(e => e.TrangThaiHoatDong)
                .HasDefaultValueSql("'1'")
                .HasComment("1 = hiển thị, 0 = ẩn")
                .HasColumnName("trang_thai_hoat_dong");

            entity.HasOne(d => d.NhomTuyChon).WithMany(p => p.Options)
                .HasForeignKey(d => d.NhomTuyChonId)
                .HasConstraintName("options_ibfk_1");
        });

        modelBuilder.Entity<OptionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("option_categories");

            entity.HasIndex(e => e.DuongDanSlug, "duong_dan_slug").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("ID nhóm tùy chọn")
                .HasColumnName("id");
            entity.Property(e => e.BieuTuong)
                .HasMaxLength(100)
                .HasComment("Icon (paint, wheels, seat...)")
                .HasColumnName("bieu_tuong");
            entity.Property(e => e.DuongDanSlug)
                .HasMaxLength(100)
                .HasComment("Slug cho URL hoặc tab")
                .HasColumnName("duong_dan_slug");
            entity.Property(e => e.MoTa)
                .HasComment("Mô tả nhóm")
                .HasColumnType("text")
                .HasColumnName("mo_ta");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenNhom)
                .HasMaxLength(100)
                .HasComment("Tên nhóm (Màu ngoại thất, Mâm xe, Nội thất...)")
                .HasColumnName("ten_nhom");
            entity.Property(e => e.ThuTuSapXep)
                .HasDefaultValueSql("'0'")
                .HasComment("Thứ tự hiển thị")
                .HasColumnName("thu_tu_sap_xep");
        });

        modelBuilder.Entity<OptionDependency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("option_dependencies");

            entity.HasIndex(e => e.TuyChonBatBuocId, "tuy_chon_bat_buoc_id");

            entity.HasIndex(e => e.TuyChonChinhId, "tuy_chon_chinh_id");

            entity.HasIndex(e => e.TuyChonXungDotId, "tuy_chon_xung_dot_id");

            entity.Property(e => e.Id)
                .HasComment("ID quy tắc")
                .HasColumnName("id");
            entity.Property(e => e.GhiChu)
                .HasComment("Giải thích quy tắc")
                .HasColumnType("text")
                .HasColumnName("ghi_chu");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TuyChonBatBuocId)
                .HasComment("Tùy chọn BẮT BUỘC phải chọn kèm")
                .HasColumnName("tuy_chon_bat_buoc_id");
            entity.Property(e => e.TuyChonChinhId)
                .HasComment("Tùy chọn chính đang được chọn")
                .HasColumnName("tuy_chon_chinh_id");
            entity.Property(e => e.TuyChonXungDotId)
                .HasComment("Tùy chọn KHÔNG ĐƯỢC chọn cùng")
                .HasColumnName("tuy_chon_xung_dot_id");

            entity.HasOne(d => d.TuyChonBatBuoc).WithMany(p => p.OptionDependencyTuyChonBatBuocs)
                .HasForeignKey(d => d.TuyChonBatBuocId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("option_dependencies_ibfk_2");

            entity.HasOne(d => d.TuyChonChinh).WithMany(p => p.OptionDependencyTuyChonChinhs)
                .HasForeignKey(d => d.TuyChonChinhId)
                .HasConstraintName("option_dependencies_ibfk_1");

            entity.HasOne(d => d.TuyChonXungDot).WithMany(p => p.OptionDependencyTuyChonXungDots)
                .HasForeignKey(d => d.TuyChonXungDotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("option_dependencies_ibfk_3");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("series");

            entity.HasIndex(e => e.DuongDanSlug, "duong_dan_slug").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("ID dòng xe tự tăng")
                .HasColumnName("id");
            entity.Property(e => e.AnhDaiDien)
                .HasMaxLength(500)
                .HasComment("Link ảnh đại diện dòng xe")
                .HasColumnName("anh_dai_dien");
            entity.Property(e => e.DuongDanSlug)
                .HasMaxLength(100)
                .HasComment("Slug dùng cho URL (3-series, x-series...)")
                .HasColumnName("duong_dan_slug");
            entity.Property(e => e.MoTa)
                .HasComment("Mô tả ngắn về dòng xe")
                .HasColumnType("text")
                .HasColumnName("mo_ta");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenDongXe)
                .HasMaxLength(100)
                .HasComment("Tên dòng xe (ví dụ: 3 Series, X Series)")
                .HasColumnName("ten_dong_xe");
            entity.Property(e => e.ThuTuSapXep)
                .HasDefaultValueSql("'0'")
                .HasComment("Thứ tự hiển thị trên website")
                .HasColumnName("thu_tu_sap_xep");
            entity.Property(e => e.TrangThaiHoatDong)
                .HasDefaultValueSql("'1'")
                .HasComment("1 = hiển thị, 0 = ẩn")
                .HasColumnName("trang_thai_hoat_dong");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "ten_dang_nhap").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("ID người dùng tự tăng")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasComment("Địa chỉ email")
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasComment("Họ và tên đầy đủ")
                .HasColumnName("ho_ten");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .HasComment("Mật khẩu đã mã hóa (bcrypt)")
                .HasColumnName("mat_khau_hash");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian cập nhật cuối")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_cap_nhat");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasComment("Thời gian tạo tài khoản")
                .HasColumnType("timestamp")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .HasComment("Số điện thoại")
                .HasColumnName("so_dien_thoai");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .HasComment("Tên đăng nhập (username)")
                .HasColumnName("ten_dang_nhap");
            entity.Property(e => e.TrangThaiHoatDong)
                .HasDefaultValueSql("'1'")
                .HasComment("1 = đang hoạt động, 0 = bị khóa")
                .HasColumnName("trang_thai_hoat_dong");
            entity.Property(e => e.VaiTro)
                .HasDefaultValueSql("'nguoi_dung'")
                .HasComment("Quyền: nguoi_dung hoặc quan_tri (admin)")
                .HasColumnType("enum('nguoi_dung','quan_tri')")
                .HasColumnName("vai_tro");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
