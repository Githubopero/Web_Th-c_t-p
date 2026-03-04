
using Microsoft.EntityFrameworkCore;

namespace WebCauHinhXe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            //
            // ĐĂNG KÝ DbContext (phần quan trọng nhất)
            builder.Services.AddDbContext<WebCauHinhXe.Models.TtContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));

            // Nếu bạn chưa có dòng này, hãy thêm
            builder.Services.AddControllers();
            //


            //
            // Thêm CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()           // Cho phép tất cả origin (dùng tạm để dev)
                        .AllowAnyMethod()           // GET, POST, PUT, DELETE...
                        .AllowAnyHeader();          // Headers cần thiết
                });
            });
            //

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Sử dụng CORS trước khi dùng các middleware khác
            app.UseCors("AllowAll");


            app.Run();
        }
    }
}
