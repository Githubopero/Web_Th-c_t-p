using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class OptionCategory
{
    /// <summary>
    /// ID nhóm tùy chọn
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Tên nhóm (Màu ngoại thất, Mâm xe, Nội thất...)
    /// </summary>
    public string TenNhom { get; set; } = null!;

    /// <summary>
    /// Slug cho URL hoặc tab
    /// </summary>
    public string DuongDanSlug { get; set; } = null!;

    /// <summary>
    /// Mô tả nhóm
    /// </summary>
    public string? MoTa { get; set; }

    /// <summary>
    /// Thứ tự hiển thị
    /// </summary>
    public int? ThuTuSapXep { get; set; }

    /// <summary>
    /// Icon (paint, wheels, seat...)
    /// </summary>
    public string? BieuTuong { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual ICollection<Option> Options { get; set; } = new List<Option>();
}
