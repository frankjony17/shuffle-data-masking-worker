using ShuffleDataMasking.Domain.Abstractions.Exceptions;
using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Services
{
    public class ForeignKeysService : IForeignKeysService
    {
        private readonly ILogger<ForeignKeysService> _logger;
        private readonly IIntrospectionDapperRepository _introspectionTabletDapperRepository;
        private readonly IShuffleDataMaskingDapperRepository _shuffleDataMaskingDapperRepository;

        private const string DISABLEALLFOREIGNKEYS = "DisableAllForeignKeys";
        private const string ENABLEALLFOREIGNKEYS = "EnableAllForeignKeys";

        public ForeignKeysService(
            ILogger<ForeignKeysService> logger,
            IIntrospectionDapperRepository introspectionTabletDapperRepository,
            IShuffleDataMaskingDapperRepository shuffleDataMaskingDapperRepository)
        {
            _logger = logger;
            _introspectionTabletDapperRepository = introspectionTabletDapperRepository;
            _shuffleDataMaskingDapperRepository = shuffleDataMaskingDapperRepository;
        }

        public async Task<bool> IsActionToForeignKeysAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig)
        {
            switch (message.TableQuery)
            {
                case DISABLEALLFOREIGNKEYS:
                    await DisableAllForeignKeysAsync(message.Database, databaseConfig);
                    return true;
                case ENABLEALLFOREIGNKEYS:
                    await EnableAllForeignKeysAsync(message.Database, databaseConfig);
                    return true;
                default:
                    return false;
            }
        }

        private async Task DisableAllForeignKeysAsync(string databaseName, DatabaseConfig databaseConfig)
        {
            try
            {
                var introspectionTables = await GetIntrospectionTable(databaseConfig);

                _introspectionTabletDapperRepository.UpdateProcessStarted(1, databaseName);

                _logger.LogInformation($"Run 'ALTER TABLE @tableName NOCHECK CONSTRAINT ALL'. [Database = {databaseName}]");
                var disableError = await _shuffleDataMaskingDapperRepository.DisableForeignKeysAsync(introspectionTables, databaseConfig);

                if (disableError.Count > 0)
                {
                    _logger.LogInformation($"Execute => insert error in database.");
                    await _introspectionTabletDapperRepository.SaveProcessErrorAsync(disableError, ErrorType.ALTER_TABLE_NOCHECK_CONSTRAINT);
                }

                _logger.LogInformation("Disabled constraint in database");
                _introspectionTabletDapperRepository.UpdateConstraintDisabled(1, databaseName);

                _logger.LogInformation("Satart process in database");
                _introspectionTabletDapperRepository.UpdateProcessStarted(0, databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Disable all foreign keys. [Database = {databaseName}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Disable all foreign keys.", ex);
            }
        }

        private async Task EnableAllForeignKeysAsync(string databaseName, DatabaseConfig databaseConfig)
        {
            try
            {
                var introspectionTables = await GetIntrospectionTable(databaseConfig);

                _logger.LogInformation($"Run 'ALTER TABLE @tableName WITH CHECK CHECK CONSTRAINT ALL'. [Database = {databaseName}]");
                var enableError = await _shuffleDataMaskingDapperRepository.EnableForeignKeysAsync(introspectionTables, databaseConfig);

                if (enableError.Count > 0)
                {
                    _logger.LogInformation($"Execute => insert error in database.");
                    await _introspectionTabletDapperRepository.SaveProcessErrorAsync(enableError, ErrorType.ALTER_TABLE_WITH_CHECK_CONSTRAINT);
                }

                _logger.LogInformation("Enable constraint in database");
                _introspectionTabletDapperRepository.UpdateConstraintDisabled(0, databaseName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Enable all foreign keys. [Database = {databaseName}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Enable all foreign keys.", ex);
            }
        }

        private async Task<IEnumerable<IntrospectionTable>> GetIntrospectionTable(DatabaseConfig _database)
        {
            try
            {
                _logger.LogInformation($"Getting introspection table. [Database = {_database}]");
                var tables = await _introspectionTabletDapperRepository.FindAllIntrospectionTable(_database);

                return tables;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Select introspection table. [Database = {_database}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Select introspection table.", ex);
            }
        }

    }
}
