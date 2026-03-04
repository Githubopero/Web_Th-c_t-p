using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class Model
{
    /// <summary>
    /// ID mẫu xe tự tăng
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID dòng xe mà mẫu này thuộc về
    /// </summary>
    public int DongXeId { get; set; }

    /// <summary>
    /// Tên mẫu xe (ví dụ: 330i Sedan, X5 xDrive40i)
    /// </summary>
    public string TenMauXe { get; set; } = null!;

    /// <summary>
    /// Slug dùng cho URL
    /// </summary>
    public string DuongDanSlug { get; set; } = null!;

    /// <summary>
    /// Giá khởi điểm (MSRP)
    /// </summary>
    public decimal GiaCoBan { get; set; }

    /// <summary>
    /// Năm sản xuất/model year
    /// </summary>
    public short? NamSanXuat { get; set; }

    /// <summary>
    /// Mô tả chi tiết mẫu xe
    /// </summary>
    public string? MoTa { get; set; }

    /// <summary>
    /// Thông số kỹ thuật dạng JSON (động cơ, công suất, kích thước...)
    /// </summary>
    public string? ThongSoKyThuat { get; set; }

    /// <summary>
    /// Link ảnh đại diện mẫu xe
    /// </summary>
    public string? AnhDaiDien { get; set; }

    /// <summary>
    /// 1 = hiển thị, 0 = ẩn
    /// </summary>
    public sbyte? TrangThaiHoatDong { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual ICollection<Configuration> Configurations { get; set; } = new List<Configuration>();

    public virtual Series DongXe { get; set; } = null!;

    public virtual ICollection<ModelOption> ModelOptions { get; set; } = new List<ModelOption>();
}
