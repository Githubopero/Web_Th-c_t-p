using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/model-options")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminModelOptionController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminModelOptionController(TtContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tùy chọn khả dụng và trạng thái gán cho một mẫu xe
        /// GET: api/admin/model-options/{modelId}
        /// </summary>
        [HttpGet("{modelId}")]
        public async Task<IActionResult> GetOptionsForModel(int modelId)
        {
            if (!await _context.Models.AnyAsync(m => m.Id == modelId))
                return NotFound("Mẫu xe không tồn tại.");

            var categories = await _context.OptionCategories
                .OrderBy(c => c.ThuTuSapXep)
                .Select(c => new
                {
                    CategoryId = c.Id,
                    c.TenNhom,
                    Options = _context.Options
                        .Where(o => o.NhomTuyChonId == c.Id && o.TrangThaiHoatDong == 1)
                        .Select(o => new
                        {
                            o.Id,
                            o.TenTuyChon,
                            o.GiaThem,
                            o.MaMau,
                            o.AnhMoTa,
                            Assigned = _context.ModelOptions.Any(mo => mo.MauXeId == modelId && mo.TuyChonId == o.Id),
                            BatBuoc = _context.ModelOptions
                                .Where(mo => mo.MauXeId == modelId && mo.TuyChonId == o.Id)
                                .Select(mo => mo.BatBuoc)
                                .FirstOrDefault() ?? 0,
                            DuocChonMacDinh = _context.ModelOptions
                                .Where(mo => mo.MauXeId == modelId && mo.TuyChonId == o.Id)
                                .Select(mo => mo.DuocChonMacDinh)
                                .FirstOrDefault() ?? 0,
                            GhiChu = _context.ModelOptions
                                .Where(mo => mo.MauXeId == modelId && mo.TuyChonId == o.Id)
                                .Select(mo => mo.GhiChuTuongThich)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>
        /// Lưu thay đổi gán tùy chọn cho mẫu xe
        /// POST: api/admin/model-options/save/{modelId}
        /// Body: { "assignments": [{ "optionId": 5, "batBuoc": 1, "duocChonMacDinh": 0, "ghiChu": "Yêu cầu gói M Sport" }, ...] }
        /// </summary>
        [HttpPost("save/{modelId}")]
        public async Task<IActionResult> SaveModelOptions(int modelId, [FromBody] SaveModelOptionsRequest request)
        {
            if (!await _context.Models.AnyAsync(m => m.Id == modelId))
                return NotFound("Mẫu xe không tồn tại.");

            if (request.Assignments == null || !request.Assignments.Any())
                return BadRequest("Không có tùy chọn nào được gán.");

            // Xóa toàn bộ gán cũ của mẫu xe này
            var oldAssignments = await _context.ModelOptions
                .Where(mo => mo.MauXeId == modelId)
                .ToListAsync();

            _context.ModelOptions.RemoveRange(oldAssignments);

            // Thêm mới
            var newAssignments = request.Assignments.Select(a => new ModelOption
            {
                MauXeId = modelId,
                TuyChonId = a.OptionId,
                BatBuoc = a.BatBuoc,
                DuocChonMacDinh = a.DuocChonMacDinh,
                GhiChuTuongThich = a.GhiChu,
                NgayTao = DateTime.UtcNow
            }).ToList();

            _context.ModelOptions.AddRange(newAssignments);
            await _context.SaveChangesAsync();

            // Log
            await LogAction("cap_nhat_gan_tuy_chon", "mau_xe", modelId, new { TotalAssigned = newAssignments.Count });

            return Ok(new { Message = "Đã lưu tùy chọn cho mẫu xe thành công." });
        }

        private async Task LogAction(string hanhDong, string loai, int idThucThe, object chiTiet)
        {
            // Tạm thời bỏ qua userId nếu chưa có auth đầy đủ
            // Thêm tương tự như các controller trước
            var log = new ActivityLog
            {
                NguoiDungId = 1, // thay bằng userId thật khi có auth
                HanhDong = hanhDong,
                LoaiThucThe = loai,
                IdThucThe = idThucThe,
                ChiTiet = System.Text.Json.JsonSerializer.Serialize(chiTiet),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    public class AssignmentDto
    {
        public int OptionId { get; set; }
        public sbyte BatBuoc { get; set; } = 0;
        public sbyte DuocChonMacDinh { get; set; } = 0;
        public string? GhiChu { get; set; }
    }

    public class SaveModelOptionsRequest
    {
        public List<AssignmentDto> Assignments { get; set; } = new();
    }
}