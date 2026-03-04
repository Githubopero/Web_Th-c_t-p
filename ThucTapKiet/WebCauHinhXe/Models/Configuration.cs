using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class Configuration
{
    /// <summary>
    /// ID cấu hình
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID người dùng (NULL nếu khách vãng lai)
    /// </summary>
    public int? NguoiDungId { get; set; }

    /// <summary>
    /// ID mẫu xe được chọn
    /// </summary>
    public int MauXeId { get; set; }

    /// <summary>
    /// Tên người dùng đặt cho cấu hình
    /// </summary>
    public string? TenCauHinh { get; set; }

    /// <summary>
    /// Mảng ID tùy chọn đã chọn [3,15,27,...]
    /// </summary>
    public string TuyChonDaChon { get; set; } = null!;

    /// <summary>
    /// Tổng giá sau khi tùy chỉnh
    /// </summary>
    public decimal TongGia { get; set; }

    /// <summary>
    /// Đơn vị tiền tệ
    /// </summary>
    public string? DonViTienTe { get; set; }

    /// <summary>
    /// Mã để chia sẻ link cấu hình
    /// </summary>
    public string MaChiaSe { get; set; } = null!;

    /// <summary>
    /// 1 = công khai, ai cũng xem được
    /// </summary>
    public sbyte? CongKhai { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    /// <summary>
    /// Thời gian cập nhật
    /// </summary>
    public DateTime? NgayCapNhat { get; set; }

    public virtual Model MauXe { get; set; } = null!;

    public virtual User? NguoiDung { get; set; }
}
