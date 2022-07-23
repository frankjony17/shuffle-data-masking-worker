using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.EntityFramework;
using ShuffleDataMasking.Infra.Data.Contexts;

namespace ShuffleDataMasking.Infra.Data.Repositories.EntityFramework
{
    public class DataCollectorEfRepository : IDataCollectorEfRepository
    {
        protected readonly DataContext _context;

        public DataCollectorEfRepository(DataContext context)
        {
            _context = context;
        }

        public void Save(DataCollector entity)
        {
            _context.Add(entity);
            _ = _context.SaveChangesAsync();
        }
    }
}

