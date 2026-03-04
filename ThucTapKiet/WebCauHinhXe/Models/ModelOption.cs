using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class ModelOption
{
    /// <summary>
    /// ID liên kết
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID mẫu xe
    /// </summary>
    public int MauXeId { get; set; }

    /// <summary>
    /// ID tùy chọn
    /// </summary>
    public int TuyChonId { get; set; }

    /// <summary>
    /// 1 = bắt buộc phải chọn
    /// </summary>
    public sbyte? BatBuoc { get; set; }

    /// <summary>
    /// 1 = được chọn mặc định
    /// </summary>
    public sbyte? DuocChonMacDinh { get; set; }

    /// <summary>
    /// Ghi chú về tương thích (ví dụ: Yêu cầu gói M Sport)
    /// </summary>
    public string? GhiChuTuongThich { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual Model MauXe { get; set; } = null!;

    public virtual Option TuyChon { get; set; } = null!;
}
