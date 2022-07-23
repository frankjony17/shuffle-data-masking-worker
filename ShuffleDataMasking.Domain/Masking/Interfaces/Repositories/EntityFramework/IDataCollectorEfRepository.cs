using ShuffleDataMasking.Domain.Masking.Entities;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.EntityFramework
{
    public interface IDataCollectorEfRepository
    {
        void Save(DataCollector entity);
    }
}
