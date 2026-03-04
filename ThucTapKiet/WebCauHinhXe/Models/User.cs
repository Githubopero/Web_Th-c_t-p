using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class User
{
    /// <summary>
    /// ID người dùng tự tăng
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên đăng nhập (username)
    /// </summary>
    public string TenDangNhap { get; set; } = null!;

    /// <summary>
    /// Địa chỉ email
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Mật khẩu đã mã hóa (bcrypt)
    /// </summary>
    public string MatKhauHash { get; set; } = null!;

    /// <summary>
    /// Họ và tên đầy đủ
    /// </summary>
    public string? HoTen { get; set; }

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string? SoDienThoai { get; set; }

    /// <summary>
    /// Quyền: nguoi_dung hoặc quan_tri (admin)
    /// </summary>
    public string? VaiTro { get; set; }

    /// <summary>
    /// 1 = đang hoạt động, 0 = bị khóa
    /// </summary>
    public sbyte? TrangThaiHoatDong { get; set; }

    /// <summary>
    /// Thời gian tạo tài khoản
    /// </summary>
    public DateTime? NgayTao { get; set; }

    /// <summary>
    /// Thời gian cập nhật cuối
    /// </summary>
    public DateTime? NgayCapNhat { get; set; }

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();
}
