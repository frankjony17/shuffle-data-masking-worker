using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Messages;
using FKSolutionsSource.Infra.Abstractions.MessagingBroker;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Services
{
    public interface IMaskGeneratorService
    {
        Task<string> GetMaskingForColumn(string columnValue, TypeOfMask typeOfMask);
    }
}
