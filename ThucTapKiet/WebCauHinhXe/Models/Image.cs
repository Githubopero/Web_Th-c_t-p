using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class Image
{
    /// <summary>
    /// ID ảnh
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Loại: dòng xe, mẫu xe hay tùy chọn
    /// </summary>
    public string LoaiThucThe { get; set; } = null!;

    /// <summary>
    /// ID của dòng/mẫu/tùy chọn
    /// </summary>
    public int IdThucThe { get; set; }

    /// <summary>
    /// Link ảnh
    /// </summary>
    public string DuongDanAnh { get; set; } = null!;

    /// <summary>
    /// Mô tả ảnh (alt text)
    /// </summary>
    public string? MoTaAnh { get; set; }

    /// <summary>
    /// Góc chụp: front, rear, side, interior...
    /// </summary>
    public string? GocChup { get; set; }

    /// <summary>
    /// Mã màu liên quan (dùng filter theo màu sơn)
    /// </summary>
    public string? MaMau { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    public int? ThuTu { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }
}
