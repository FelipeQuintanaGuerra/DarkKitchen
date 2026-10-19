using ServiceFactory;
using System.Diagnostics.CodeAnalysis;
using WebApi.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    options.Filters.Add<ExceptionFilter>());

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:4200", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

ServiceFactory.ServiceFactory.RegisterServices(builder.Services, builder.Configuration, builder.Environment.WebRootPath);

var app = builder.Build();

//app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseStaticFiles();
app.MapControllers();

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }