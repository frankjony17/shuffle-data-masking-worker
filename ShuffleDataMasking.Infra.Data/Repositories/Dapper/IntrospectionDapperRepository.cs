 using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using ShuffleDataMasking.Infra.Data.Sql;
using Dapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Infra.Data.Repositories.Dapper
{
    public class IntrospectionDapperRepository : IIntrospectionDapperRepository
    {
        private readonly IDapperConnection _dapperConnection;
        private readonly ILogger<IntrospectionDapperRepository> _logger;

        public IntrospectionDapperRepository(
            ILogger<IntrospectionDapperRepository> logger,
            IDapperConnection dapperConnection)
        {
            _logger = logger;
            _dapperConnection = dapperConnection;
        }

        public async Task<IEnumerable<IntrospectionTable>> FindAllIntrospectionTable(DatabaseConfig _database)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.QueryAsync<IntrospectionTable>(SqlStatements.SelectIntrospectionTable, new { database = _database.ToString() });

            return result;
        }

        public async Task<IEnumerable<IntrospectionColumn>> FindAllIntrospectionColumns(long _tableId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.QueryAsync<IntrospectionColumn>(SqlStatements.SelectIntrospectionColumn, new { tableId = _tableId });

            return result;
        }

        public async Task SaveProcessErrorAsync(List<ProcessErrorDto> _tableError, ErrorType errorTypeId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            foreach (var error in _tableError)
            {
                try
                {
                    _ = await sqlConnection.ExecuteAsync(SqlStatements.InsertProcessError, new { 
                        tableId=error.TableId,
                        errorTypeId,
                        errorDescription=error.ErrorDescription,
                        originalQuery=error.OriginalQuery,
                        queueProcessId=error.QueryProcessId 
                    });

                    _logger.LogInformation($"Execute => insert process error. [TableId = {error.TableId}] - [ErrorId = {errorTypeId}]");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception => [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                }
            }
        }

        public async Task<DataCollector> GetMaskedData(string originalData, int typeOfMaskId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.QueryFirstOrDefaultAsync<DataCollector>(SqlStatements.SelectOriginalData, new { originalData, typeOfMaskId });

            return result;
        }

        public async Task<int> GetCountMaskedData(string maskedData)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.ExecuteScalarAsync<int>(SqlStatements.SelectCountMaskedData, new { maskedData });

            return result;
        }

        public async void UpdateConstraintDisabled(int constraintDisabled, string database)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.UpdateConstraintDisabled, new { constraintDisabled, database });
        }

        public async void UpdateProcessStarted(int processStarted, string database)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.UpdateProcessStarted, new { processStarted, database });
        }

        public async void UpdateQueueStartProcess(DateTime startProcess, long queueId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.UpdateQueueStartProcess, new { startProcess, queueId });
        }

        public async void UpdateQueueEndedProcess(DateTime endedProcess, long queueId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.UpdateQueueEndedProcess, new { endedProcess, queueId });
        }

        public async Task<int> SelectProcessedMasking(long queueId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.ExecuteScalarAsync<int>(SqlStatements.SelectProcessedMasking, new { queueId });

            return result;
        }

        public async void RemoveErrorFromDatabase(long errorId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.RemoveErrorFromDatabase, new { processErrorId=errorId });
        }

        public async void UpdateProcessedMasking(int rowCounter, long queryProcessId)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            await sqlConnection.ExecuteAsync(SqlStatements.UpdateProcessedMasking, new { processedMasking = rowCounter, queueId=queryProcessId });
        }

        public async Task<IEnumerable<RelationDto>> FindAllSecondaryTable(string tableName, string fatherTableName)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(DatabaseConfig.SHUFFLE_DATA_MASKING);

            var result = await sqlConnection.QueryAsync<RelationDto>(SqlStatements.SelectRelativeTable, new { tableName, fatherTableName });

            return result;
        }
    }
}
