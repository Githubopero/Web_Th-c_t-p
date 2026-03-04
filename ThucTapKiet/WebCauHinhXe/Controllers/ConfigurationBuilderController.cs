using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationBuilderController : ControllerBase
    {
        private readonly TtContext _context;

        public ConfigurationBuilderController(TtContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Load dữ liệu để bắt đầu tùy chỉnh cho một mẫu xe
        /// GET: api/ConfigurationBuilder/start/{modelId}
        /// </summary>
        [HttpGet("start/{modelId}")]
        public async Task<IActionResult> StartConfiguration(int modelId)
        {
            var model = await _context.Models
                .Include(m => m.ModelOptions).ThenInclude(mo => mo.TuyChon).ThenInclude(o => o.NhomTuyChon)
                .FirstOrDefaultAsync(m => m.Id == modelId && m.TrangThaiHoatDong == 1);

            if (model == null)
                return NotFound("Không tìm thấy mẫu xe.");

            // Cách sửa: Lấy danh sách ID nhóm trước
            var categoryIds = await _context.ModelOptions
                .Where(mo => mo.MauXeId == modelId)
                .Select(mo => mo.TuyChon.NhomTuyChonId)
                .Distinct()
                .ToListAsync();

            var categories = await _context.OptionCategories
                .Where(c => categoryIds.Contains(c.Id))
                .OrderBy(c => c.ThuTuSapXep)
                .Select(c => new
                {
                    c.Id,
                    c.TenNhom,
                    c.DuongDanSlug,
                    c.BieuTuong
                })
                .ToListAsync();

            var optionsByCategory = new Dictionary<int, List<object>>();

            foreach (var cat in categories)
            {
                var opts = await _context.ModelOptions
                    .Where(mo => mo.MauXeId == modelId && mo.TuyChon.NhomTuyChonId == cat.Id)
                    .Include(mo => mo.TuyChon)
                    .Select(mo => new
                    {
                        OptionId = mo.TuyChonId,
                        mo.TuyChon.TenTuyChon,
                        mo.TuyChon.DuongDanSlug,
                        mo.TuyChon.GiaThem,
                        mo.TuyChon.AnhMoTa,
                        mo.TuyChon.MaMau,
                        mo.TuyChon.TonKho,
                        mo.BatBuoc,
                        mo.DuocChonMacDinh,
                        mo.GhiChuTuongThich
                    })
                    .ToListAsync();

                optionsByCategory[cat.Id] = opts.Cast<object>().ToList();
            }

            // Tùy chọn mặc định tự động chọn
            var defaultSelected = optionsByCategory
                .SelectMany(kv => kv.Value)
                .Where(o => ((dynamic)o).DuocChonMacDinh == 1 || ((dynamic)o).BatBuoc == 1)
                .Select(o => ((dynamic)o).OptionId)
                .ToList();

            // Dòng ~80-99 trong action StartConfiguration
            var previewImages = await _context.Images
                .Where(img => img.LoaiThucThe == "mau_xe" && img.IdThucThe == modelId)
                .OrderBy(img => img.ThuTu ?? 0)
                .Select(img => new PreviewImageDto
                {
                    DuongDanAnh = img.DuongDanAnh,
                    GocChup = img.GocChup,
                    MaMau = img.MaMau,
                    MoTaAnh = img.MoTaAnh
                })
                .ToListAsync();

            // Không cần ??= nữa, vì ToListAsync() luôn trả List<T> (không null)
            // Nhưng vẫn xử lý trường hợp rỗng để frontend an toàn
            if (!previewImages.Any())
            {
                previewImages.Add(new PreviewImageDto
                {
                    DuongDanAnh = "/images/default-car-preview.jpg",
                    GocChup = "default",
                    MaMau = null,
                    MoTaAnh = "Ảnh mặc định"
                });
            }

            return Ok(new
            {
                Model = new { model.Id, model.TenMauXe, model.GiaCoBan },
                Categories = categories,
                OptionsByCategory = optionsByCategory,
                DefaultSelected = defaultSelected,
                PreviewImages = previewImages
            });
        }

        /// <summary>
        /// Kiểm tra dependencies realtime khi user thay đổi lựa chọn
        /// POST: api/ConfigurationBuilder/validate
        /// Body: { "modelId": 5, "selectedOptionIds": [3,15,27] }
        /// </summary>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateSelection([FromBody] ValidateSelectionRequest request)
        {
            if (request == null || !request.SelectedOptionIds.Any())
                return BadRequest("Danh sách tùy chọn không hợp lệ.");

            var selectedIds = request.SelectedOptionIds.ToHashSet();

            // Load tất cả dependencies liên quan đến các tùy chọn đang chọn
            var dependencies = await _context.OptionDependencies
                .Where(d => selectedIds.Contains(d.TuyChonChinhId))
                .Include(d => d.TuyChonBatBuoc)
                .Include(d => d.TuyChonXungDot)
                .ToListAsync();

            var requiredIds = new HashSet<int>();
            var conflictedIds = new HashSet<int>();

            foreach (var dep in dependencies)
            {
                // Bắt buộc phải chọn kèm
                if (dep.TuyChonBatBuocId.HasValue)
                {
                    requiredIds.Add(dep.TuyChonBatBuocId.Value);
                }

                // Xung đột - không được chọn cùng
                if (dep.TuyChonXungDotId.HasValue && selectedIds.Contains(dep.TuyChonXungDotId.Value))
                {
                    conflictedIds.Add(dep.TuyChonXungDotId.Value);
                }
            }

            // Tính tổng giá
            var totalAddOn = await _context.Options
                .Where(o => selectedIds.Contains(o.Id))
                .SumAsync(o => o.GiaThem ?? 0);

            var basePrice = (await _context.Models.FindAsync(request.ModelId))?.GiaCoBan ?? 0;
            var grandTotal = basePrice + totalAddOn;

            return Ok(new
            {
                RequiredToAdd = requiredIds,
                Conflicts = conflictedIds,
                TotalAddOnPrice = totalAddOn,
                GrandTotal = grandTotal,
                GrandTotalVnd = grandTotal.ToString("N0") + " VND"
            });
        }
    }

    public class ValidateSelectionRequest
    {
        public int ModelId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
    }
}