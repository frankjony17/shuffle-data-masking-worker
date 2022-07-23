using ShuffleDataMasking.Domain.Abstractions.Exceptions;
using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Messages;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using Microsoft.Extensions.Logging;
using FKSolutionsSource.Infra.Abstractions.Logging;
using FKSolutionsSource.Infra.Abstractions.MessagingBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Services
{
    public class ShuffleDataMaskingService : IShuffleDataMaskingService
    {
        private readonly ILogger<ShuffleDataMaskingService> _logger;
        private readonly IReductionService _reductionService;
        private readonly IForeignKeysService _foreignKeysService;
        private readonly IMaskGeneratorService _maskGeneratorService;
        private readonly IProcessErrorService _processErrorService;
        private readonly IDapperConnection _dapperConnection;
        private readonly IIntrospectionDapperRepository _introspectionTabletDapperRepository;
        private readonly IShuffleDataMaskingDapperRepository _shuffleDataMaskingDapperRepository;

        private DatabaseConfig databaseConfig;

        public ShuffleDataMaskingService(
            ILogger<ShuffleDataMaskingService> logger,
            IReductionService reductionService,
            IForeignKeysService foreignKeysService,
            IMaskGeneratorService maskGeneratorService,
            IProcessErrorService processErrorService,
            IDapperConnection dapperConnection,
            IIntrospectionDapperRepository introspectionTabletDapperRepository,
            IShuffleDataMaskingDapperRepository shuffleDataMaskingDapperRepository)
        {
            _logger = logger;
            _reductionService = reductionService;
            _foreignKeysService = foreignKeysService;
            _maskGeneratorService = maskGeneratorService;
            _processErrorService = processErrorService;
            _dapperConnection = dapperConnection;
            _introspectionTabletDapperRepository = introspectionTabletDapperRepository;
            _shuffleDataMaskingDapperRepository = shuffleDataMaskingDapperRepository;
        }

        public async Task Process(IMessage<ShuffleDataMaskingMessage> message)
        {
            var startTime = DateTimeOffset.Now;
            _logger.BeginCorrelationIdScope(message.CorrelationId);
            _logger.LogInformation($"Starting shuffle data masking process. [Database = {message.Data.Database}] - [StartAt = |{startTime}|]");

            if (!message.Data.IsValid)
            {
                _logger.LogInformation($"Message is not valid. [Database = {message.Data.Database}] - [Table = {message.Data.TableQuery}]");
                throw new MessageNotValidException();
            }

            databaseConfig = GetDatabase(message.Data.Database);
            var isActionToReductionAsync = await _reductionService.IsActionToReductionAsync(message.Data, databaseConfig);
            var isActionToForeignKeys = await _foreignKeysService.IsActionToForeignKeysAsync(message.Data, databaseConfig);
            var isErrorProcessAsync = await _processErrorService.IsErrorProcessAsync(message.Data, databaseConfig);

            if (!isActionToReductionAsync && !isActionToForeignKeys && !isErrorProcessAsync)
            {
                await StartProcessForAnyDatabase(message.Data);
            }
            _logger.LogInformation($"Finished part of data masking process. [Table = {message.Data.TableQuery}] - [EndsAt = |{DateTimeOffset.Now - startTime}|]");
        }

        private async Task StartProcessForAnyDatabase(ShuffleDataMaskingMessage maskingMessage)
        {
            _logger.LogInformation($"Update queue start process. [QueryProcessId = {maskingMessage.QueryProcessId}]");
            _introspectionTabletDapperRepository.UpdateQueueStartProcess(DateTime.Now, maskingMessage.QueryProcessId);

            _logger.LogInformation($"Get processed masking. [QueryProcessId = {maskingMessage.QueryProcessId}]");
            var processedMasking = await _introspectionTabletDapperRepository.SelectProcessedMasking(maskingMessage.QueryProcessId);

            var introspectionColumns = await GetIntrospectionColumn(long.Parse(maskingMessage.TableQueryId));

            await StartMaskingProcessForTable(introspectionColumns, maskingMessage, databaseConfig, processedMasking);
        }

        private async Task StartMaskingProcessForTable(IEnumerable<IntrospectionColumn> introspectionColumns, ShuffleDataMaskingMessage maskingMessage, DatabaseConfig database, int processedMasking)
        {
            var selectQuery = BuildQueryToSelectRecords(maskingMessage, introspectionColumns, processedMasking);

            var recordsList = await GetRecordsToMask(database, selectQuery, maskingMessage.TableQuery);

            var updateError = await BuildAndRunQueryToMaskingColumns(recordsList, introspectionColumns, maskingMessage, processedMasking);

            if (updateError.Count > 0)
            {
                _logger.LogInformation($"Execute => insert error in database. [Table = {maskingMessage.TableQuery}]");
                await _introspectionTabletDapperRepository.SaveProcessErrorAsync(updateError, ErrorType.UPDATE_MASKED_DATA_IN_TABLA);
            }
            _introspectionTabletDapperRepository.UpdateQueueEndedProcess(DateTime.Now, maskingMessage.QueryProcessId);
        }

        private async Task<IEnumerable<Object>> GetRecordsToMask(DatabaseConfig database, string selectQuery, string table)
        {
            try
            {
                _logger.LogInformation($"Getting records to mask. [Database = {database}] - [Table = {table}]");
                var records = await _shuffleDataMaskingDapperRepository.FindRecordByQuery(database, selectQuery);

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Select records to mask. [Database = {database}] - [Table = {table}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Select records to mask. [Database = {database}] - [Table = {table}]", ex);
            }
        }

        private async Task<IEnumerable<IntrospectionColumn>> GetIntrospectionColumn(long tableId)
        {
            try
            {
                _logger.LogInformation($"Getting introspection column. [TableId = {tableId}]");
                var columns = await _introspectionTabletDapperRepository.FindAllIntrospectionColumns(tableId);

                return columns;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Select introspection column. [TableId = {tableId}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Select introspection table.", ex);
            }
        }

        private static string BuildQueryToSelectRecords(ShuffleDataMaskingMessage maskingMessage, IEnumerable<IntrospectionColumn> introspectionColumns, int processedMasking)
        {
            StringBuilder selectQuery = new("SELECT");
            var firstColumn = introspectionColumns.First();
            var startSelect = processedMasking + maskingMessage.StartQuery;

            foreach (var column in introspectionColumns)
            {
                selectQuery.Append($" {column.ColumnName},");
            }

            selectQuery.Remove(selectQuery.Length - 1, 1);
            selectQuery.Append($" FROM(SELECT ROW_NUMBER() OVER (ORDER BY {firstColumn.ColumnName}) AS RowNum, * FROM {maskingMessage.TableQuery}) AS RowConstrainedResult");
            selectQuery.Append($" WHERE RowNum >= {startSelect} AND RowNum < {maskingMessage.EndQuery}");
            selectQuery.Append($" ORDER BY RowNum");

            return selectQuery.ToString();
        }

        private async Task<List<ProcessErrorDto>> BuildAndRunQueryToMaskingColumns(IEnumerable<Object> recordsList, IEnumerable<IntrospectionColumn> introspectionColumns, ShuffleDataMaskingMessage maskingMessage, int processedMasking)
        {
            int countQuery = 1;
            List<ProcessErrorDto> errors = new();

            using var connDatabase = _dapperConnection.GetSqlConnection(databaseConfig);
            using var connMaskings = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            foreach (var record in recordsList)
            {
                var recordDetails = ((IDictionary<string, object>)record);
                ProcessErrorDto error = null;

                string queryWhitMask = await GetQueryWhitMask(introspectionColumns, recordDetails, maskingMessage.TableQuery);

                if (queryWhitMask != string.Empty)
                {
                    error = await _shuffleDataMaskingDapperRepository.UpdateColumnsAsync(queryWhitMask, long.Parse(maskingMessage.TableQueryId), maskingMessage.QueryProcessId, processedMasking, connDatabase, connMaskings);
                    processedMasking++;

                    _logger.LogInformation($"Run masking query. [{countQuery} -> {recordsList.Count()}] - [Query = {queryWhitMask}]");
                    countQuery++;
                }

                if (error != null)
                {
                    errors.Add(error);
                }
            }
            return errors;
        }

        private async Task<string> GetQueryWhitMask(IEnumerable<IntrospectionColumn> introspectionColumns, IDictionary<string, object> recordDetails, string table)
        {
            StringBuilder updateQuery = new($"UPDATE {table} SET");
            StringBuilder whereQuery = new($" WHERE");

            var queryLength = updateQuery.Length;

            foreach (var column in introspectionColumns)
            {
                var originalValue = recordDetails[column.ColumnName];

                if (originalValue != null)
                {
                    var columnMask = await _maskGeneratorService.GetMaskingForColumn(originalValue.ToString(), column.TypeOfMask);

                    updateQuery.Append($" {column.ColumnName}='{columnMask}',");
                    whereQuery.Append($" {column.ColumnName}='{originalValue.ToString().Replace("'", "''")}' AND");
                }
            }

            if (updateQuery.Length == queryLength)
            {
                return string.Empty;
            }

            updateQuery.Remove(updateQuery.Length - 1, 1);
            whereQuery.Remove(whereQuery.Length - 4, 4);
            updateQuery.Append(whereQuery);

            return updateQuery.ToString();
        }

        private static DatabaseConfig GetDatabase(string databaseName)
        {
            return databaseName switch
            {
                "FKS" => DatabaseConfig.FKS,
                _ => DatabaseConfig.FKSOLUTIONS,
            };
        }
    }
}
