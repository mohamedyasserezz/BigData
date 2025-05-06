using BigData.Domain.Contract.Persistance;
using BigData.Persistance.Data;
namespace BigData.Persistance.GenericRepositry
{
    public class GenericRepositry<T> : IGenericRepositry<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public GenericRepositry(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Add(T entity)
        {
             _dbContext.Set<T>().Add(entity);

             _dbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);

            _dbContext.SaveChanges();
        }

        public void Update(T entity)
        {
            _dbContext.Set<T>().Update(entity);

            _dbContext.SaveChanges();
        }
    }
}
