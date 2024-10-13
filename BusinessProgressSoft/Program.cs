using BusinessProgressSoft.Models;
using BusinessProgressSoft.Models.Services;
using Microsoft.EntityFrameworkCore;

namespace BusinessProgressSoft
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
            builder.Services.AddDbContext<BusinessProgressSoftContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            //register the CSV service
            builder.Services.AddScoped<ICSVService, CSVService>();
            builder.Services.AddScoped<ICards, Cards>();
            // to connection front-end with back-end then app.UseCors("policy");
            builder.Services.AddCors(corsOptions =>
            {
                corsOptions.AddPolicy("policy",
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("policy");

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}