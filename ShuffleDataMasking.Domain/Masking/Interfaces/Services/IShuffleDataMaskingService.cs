using ShuffleDataMasking.Domain.Masking.Messages;
using FKSolutionsSource.Infra.Abstractions.MessagingBroker;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Services
{
    public interface IShuffleDataMaskingService
    {
        Task Process(IMessage<ShuffleDataMaskingMessage> message);
    }
}
