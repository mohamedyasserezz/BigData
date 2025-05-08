using BigData.Apis.Contract;
using BigData.Apis.Services;
using BigData.Application.Services;
using BigData.Persistance.Data;
using Microsoft.EntityFrameworkCore;

namespace BigData.Apis
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(optionsAction =>
                optionsAction.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            #region CORS
            // var allowedOrgins = configuration.GetSection("AllowedOrgins").Get<string[]>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyOrigin()    // Allow requests from any origin
                        .AllowAnyHeader()    // Allow any headers
                        .AllowAnyMethod();   // Allow any HTTP methods (GET, POST, etc.)
                });
            });

            #endregion
            services.AddScoped<IIndexingService, IndexingService>();
            services.AddScoped<ISearchService, SearchService>();

            return services;
        }
    }
}
