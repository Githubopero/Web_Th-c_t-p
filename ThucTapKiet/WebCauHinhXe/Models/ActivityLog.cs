using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class ActivityLog
{
    /// <summary>
    /// ID log
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID người thực hiện
    /// </summary>
    public int NguoiDungId { get; set; }

    /// <summary>
    /// Tên hành động (tao_cau_hinh, cap_nhat_tuy_chon...)
    /// </summary>
    public string HanhDong { get; set; } = null!;

    /// <summary>
    /// Loại đối tượng (mau_xe, tuy_chon...)
    /// </summary>
    public string? LoaiThucThe { get; set; }

    /// <summary>
    /// ID đối tượng
    /// </summary>
    public int? IdThucThe { get; set; }

    /// <summary>
    /// Chi tiết hành động dạng JSON
    /// </summary>
    public string? ChiTiet { get; set; }

    /// <summary>
    /// IP người dùng
    /// </summary>
    public string? DiaChiIp { get; set; }

    /// <summary>
    /// User agent
    /// </summary>
    public string? TrinhDuyet { get; set; }

    /// <summary>
    /// Thời gian thực hiện
    /// </summary>
    public DateTime? NgayThucHien { get; set; }
}
