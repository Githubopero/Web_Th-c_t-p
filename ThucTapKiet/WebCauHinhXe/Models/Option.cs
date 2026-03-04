using System;
using System.Collections.Generic;

namespace WebCauHinhXe.Models;

public partial class Option
{
    /// <summary>
    /// ID tùy chọn tự tăng
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// ID nhóm mà tùy chọn này thuộc về
    /// </summary>
    public int NhomTuyChonId { get; set; }

    /// <summary>
    /// Tên tùy chọn (Alpine White, 19&quot; M Double-spoke...)
    /// </summary>
    public string TenTuyChon { get; set; } = null!;

    /// <summary>
    /// Slug duy nhất trong nhóm
    /// </summary>
    public string DuongDanSlug { get; set; } = null!;

    /// <summary>
    /// Giá tăng thêm khi chọn (0 = miễn phí)
    /// </summary>
    public decimal? GiaThem { get; set; }

    /// <summary>
    /// Mô tả chi tiết tùy chọn
    /// </summary>
    public string? MoTa { get; set; }

    /// <summary>
    /// Link ảnh minh họa tùy chọn
    /// </summary>
    public string? AnhMoTa { get; set; }

    /// <summary>
    /// Mã màu sơn (hex hoặc mã BMW)
    /// </summary>
    public string? MaMau { get; set; }

    /// <summary>
    /// 1 = được chọn mặc định
    /// </summary>
    public sbyte? LaMacDinh { get; set; }

    /// <summary>
    /// Số lượng tồn kho (nếu áp dụng)
    /// </summary>
    public int? TonKho { get; set; }

    /// <summary>
    /// 1 = hiển thị, 0 = ẩn
    /// </summary>
    public sbyte? TrangThaiHoatDong { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime? NgayTao { get; set; }

    public virtual ICollection<ModelOption> ModelOptions { get; set; } = new List<ModelOption>();

    public virtual OptionCategory NhomTuyChon { get; set; } = null!;

    public virtual ICollection<OptionDependency> OptionDependencyTuyChonBatBuocs { get; set; } = new List<OptionDependency>();

    public virtual ICollection<OptionDependency> OptionDependencyTuyChonChinhs { get; set; } = new List<OptionDependency>();

    public virtual ICollection<OptionDependency> OptionDependencyTuyChonXungDots { get; set; } = new List<OptionDependency>();
}
