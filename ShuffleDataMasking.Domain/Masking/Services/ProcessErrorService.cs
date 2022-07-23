using ShuffleDataMasking.Domain.Abstractions.Exceptions;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Services
{
    public class ProcessErrorService : IProcessErrorService
    {
        private readonly ILogger<ProcessErrorService> _logger;
        private readonly IIntrospectionDapperRepository _introspectionTabletDapperRepository;
        private readonly IShuffleDataMaskingDapperRepository _shuffleDataMaskingDapperRepository;

        private const string RUNERRORQUERY = "RunErrorQuery";

        public ProcessErrorService(
            ILogger<ProcessErrorService> logger,
            IIntrospectionDapperRepository introspectionTabletDapperRepository,
            IShuffleDataMaskingDapperRepository shuffleDataMaskingDapperRepository)
        {
            _logger = logger;
            _introspectionTabletDapperRepository = introspectionTabletDapperRepository;
            _shuffleDataMaskingDapperRepository = shuffleDataMaskingDapperRepository;
        }

        public async Task<bool> IsErrorProcessAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig)
        {
            switch (message.TableQuery)
            {
                case RUNERRORQUERY:
                    await ProcessErrorQueryAsync(databaseConfig, message);
                    return true;
                default:
                    return false;
            }
        }

        private async Task ProcessErrorQueryAsync(DatabaseConfig databaseConfig, ShuffleDataMaskingMessage message)
        {
            try
            {
                _logger.LogInformation("Run query for recovery error.");
                await _shuffleDataMaskingDapperRepository.RunQuery(databaseConfig, message.ErrorProcessQuery);

                if (message.QueryProcessId > 0)
                {
                    var processedMasking = await _introspectionTabletDapperRepository.SelectProcessedMasking(message.QueryProcessId);
                    processedMasking++;

                    _introspectionTabletDapperRepository.UpdateProcessedMasking(processedMasking, message.QueryProcessId);
                }

                _logger.LogInformation("Remove error from database.");
                _introspectionTabletDapperRepository.RemoveErrorFromDatabase(message.ErrorProcessId);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Run query for recovery error. [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Run query for recovery error.", ex);
            }
        }

    }
}
