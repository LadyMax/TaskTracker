
using Microsoft.OpenApi.Models;
using System.Reflection;
using TaskTracker.Api.Endpoints;
using TaskTracker.Core.Services;

namespace TaskTracker.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "TaskTracker API",
                    Description = "An API for handling Task items.",
                });
                options.CustomSchemaIds(type => type.FullName?.Replace('+', '.'));
                options.InferSecuritySchemes();
            });

            // Add services to the container.
            builder.Services.AddSingleton<ITaskService, TaskService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    // Disable swagger schemas at bottom
                    c.DefaultModelsExpandDepth(-1);
                });
            }

            app.MapEndpoints<Program>();

            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.Run();
        }
    }
}
