using BusinessLogic;
using BusinessLogic.Reflection;
using Domain;
using IBussinesLogic;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using System.Diagnostics.CodeAnalysis;


namespace ServiceFactory;

[ExcludeFromCodeCoverage]
public static class ServiceFactory
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration, string webRootPath)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new Exception("Missing DefaultConnection connection string");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(3)));

        services.AddScoped<DbContext, AppDbContext>();

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Services
        services.AddScoped<IShippingTypeService, ShippingTypeService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IImageStorageService>(_ => new ImageStorageService(webRootPath));
        services.AddScoped<IAuditLoggerService, AuditLoggerService>();

        // Importers (Reflection)
        var pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        var imagesPath = Path.Combine(webRootPath, "images", "products");
        services.AddScoped<IAssemblyLoader>(_ => new AssemblyLoader(pluginsPath));
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IImporterService>(sp => new ImporterService(
            sp.GetRequiredService<IAssemblyLoader>(),
            sp.GetRequiredService<IProductRepository>(),
            sp.GetRequiredService<IGenericRepository<Domain.CommercialLine>>(),
            sp.GetRequiredService<IGenericRepository<Domain.Category>>(),
            sp.GetRequiredService<IFileService>(),
            imagesPath
        ));
    }
}
