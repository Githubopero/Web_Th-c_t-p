using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class OptionDependency
{
    /// <summary>
    /// ID quy tắc
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tùy chọn chính đang được chọn
    /// </summary>
    public int TuyChonChinhId { get; set; }

    /// <summary>
    /// Tùy chọn BẮT BUỘC phải chọn kèm
    /// </summary>
    public int? TuyChonBatBuocId { get; set; }

    /// <summary>
    /// Tùy chọn KHÔNG ĐƯỢC chọn cùng
    /// </summary>
    public int? TuyChonXungDotId { get; set; }

    /// <summary>
    /// Giải thích quy tắc
    /// </summary>
    public string? GhiChu { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual Option? TuyChonBatBuoc { get; set; }

    public virtual Option TuyChonChinh { get; set; } = null!;

    public virtual Option? TuyChonXungDot { get; set; }
}
