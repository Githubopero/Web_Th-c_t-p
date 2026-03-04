using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class Series
{
    /// <summary>
    /// ID dòng xe tự tăng
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên dòng xe (ví dụ: 3 Series, X Series)
    /// </summary>
    public string TenDongXe { get; set; } = null!;

    /// <summary>
    /// Slug dùng cho URL (3-series, x-series...)
    /// </summary>
    public string DuongDanSlug { get; set; } = null!;

    /// <summary>
    /// Mô tả ngắn về dòng xe
    /// </summary>
    public string? MoTa { get; set; }

    /// <summary>
    /// Link ảnh đại diện dòng xe
    /// </summary>
    public string? AnhDaiDien { get; set; }

    /// <summary>
    /// Thứ tự hiển thị trên website
    /// </summary>
    public int? ThuTuSapXep { get; set; }

    /// <summary>
    /// 1 = hiển thị, 0 = ẩn
    /// </summary>
    public sbyte? TrangThaiHoatDong { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();
}
