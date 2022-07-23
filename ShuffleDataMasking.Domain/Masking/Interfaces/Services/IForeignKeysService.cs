using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Messages;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Services
{
    public interface IForeignKeysService
    {
        Task<bool> IsActionToForeignKeysAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig);
    }
}
