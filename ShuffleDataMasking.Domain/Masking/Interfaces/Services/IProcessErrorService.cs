using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Messages;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Services
{
    public interface IProcessErrorService
    {
        Task<bool> IsErrorProcessAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig);
    }
}
