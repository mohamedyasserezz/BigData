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

            services.AddScoped<IIndexingService, IndexingService>();
            services.AddScoped<ISearchService, SearchService>();

            return services;
        }
    }
}
