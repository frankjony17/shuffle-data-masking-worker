using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Messages;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Services
{
    public interface IReductionService
    {
        Task<bool> IsActionToReductionAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig);
    }
}
