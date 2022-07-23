using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using ShuffleDataMasking.Infra.Data.Sql;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Infra.Data.Repositories.Dapper
{
    public class ShuffleDataMaskingDapperRepository : IShuffleDataMaskingDapperRepository
    {
        private readonly IDapperConnection _dapperConnection;
        private readonly ILogger<ShuffleDataMaskingDapperRepository> _logger;

        public ShuffleDataMaskingDapperRepository(
            ILogger<ShuffleDataMaskingDapperRepository> logger,
            IDapperConnection dapperConnection)
        {
            _logger = logger;
            _dapperConnection = dapperConnection;
        }

        public async Task<List<ProcessErrorDto>> DisableForeignKeysAsync(IEnumerable<IntrospectionTable> introspectionTables, DatabaseConfig database)
        {
            return await AlterTableConstraintAsync(introspectionTables, database, false);
        }

        public async Task<List<ProcessErrorDto>> EnableForeignKeysAsync(IEnumerable<IntrospectionTable> introspectionTables, DatabaseConfig database)
        {
            return await AlterTableConstraintAsync(introspectionTables, database, true);
        }

        public async Task<IEnumerable<Object>> FindRecordByQuery(DatabaseConfig database, string selectQuery)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(database);

            var result = await sqlConnection.QueryAsync<Object>(selectQuery);

            return result;
        }

        public async Task<IEnumerable<Object>> FindRecordByQuery(string selectQuery, SqlConnection connection)
        {
            return await connection.QueryAsync<Object>(selectQuery);
        }

        private async Task<List<ProcessErrorDto>> AlterTableConstraintAsync(IEnumerable<IntrospectionTable> introspectionTables, DatabaseConfig database, bool option)
        {
            List<ProcessErrorDto> errors = new();

            using var sqlConnection = _dapperConnection.GetSqlConnection(database);

            foreach (var table in introspectionTables)
            {
                var query = GetAlterTableQuery(table.TableName, option);
                try
                {
                    _ = await sqlConnection.ExecuteAsync(query);

                    _logger.LogInformation($"Execute => '{query}' <=> OK");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception => [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                    errors.Add(ProcessErrorDto.Create(table.Id, $"[InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]", query));
                }
            }

            return errors;
        }

        public async Task<ProcessErrorDto> UpdateColumnsAsync(string updateQuery, long tableId, long queueId, int processedMasking, SqlConnection connDatabase, SqlConnection connMaskings)
        {
            ProcessErrorDto error = null;
            try
            {
                _ = await connDatabase.ExecuteAsync(updateQuery);
                _ = await connMaskings.ExecuteAsync(SqlStatements.UpdateProcessedMasking, new { processedMasking, queueId });
            }
            catch (Exception ex)
            {
                error = ProcessErrorDto.Create(tableId, $"[InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]", updateQuery, queueId);
                _logger.LogError($"Exception => [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
            }
            return error;
        }

        public async Task RunQuery(DatabaseConfig database, string query)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(database);

            _ = await sqlConnection.ExecuteAsync(query);

            _logger.LogInformation($"Execute => '{query}' <=> OK");
        }

        public async Task<IEnumerable<string>> FindColumnNameOfPrimaryKey(string tableName, DatabaseConfig database)
        {
            using var sqlConnection = _dapperConnection.GetSqlConnection(database);

            var result = await sqlConnection.QueryAsync<string>(SqlStatements.SelectColumnNameOfPrimaryKey, new { tableName });

            return result;
        }

        public async Task CreateTemporalTable(DatabaseConfig databaseConfig)
        {
            await RunQuery(databaseConfig, SqlStatements.CreateTemporalTable);
        }

        public async Task RemoveTemporalTable(DatabaseConfig databaseConfig)
        {
            await RunQuery(databaseConfig, SqlStatements.DropTemporalTable);
        }

        public void SavetFirstFatherPrimaryKey(PrimaryKeyDto reduction, string tableName, DatabaseConfig database)
        {
            int i = 0;
            var parms = new List<object>();

            foreach (string primaryKey in reduction.PrimaryKeys)
            {
                parms.Add(new { columnName = reduction.ColumnName, primaryKey, tableName });
                i++;
            }

            using var sqlConnection = _dapperConnection.GetSqlConnection(database);
            sqlConnection.Execute(SqlStatements.InsertPrimaryKey, parms.ToArray());
        }

        public async Task InsertPrimaryKeyTotalRecordAsync(string query, DatabaseConfig database)
        {
            await RunQuery(database, query);
        }

        public async Task<List<ProcessErrorDto>> ExcecuteReductionQuery(List<string> deleteQueryList, int processedMasking, DatabaseConfig databaseConfig)
        {
            List<ProcessErrorDto> errors = new();

            using var sqlConnection = _dapperConnection.GetSqlConnection(databaseConfig);

            foreach (var query in deleteQueryList)
            {
                try
                {
                    _ = await sqlConnection.ExecuteAsync(query);

                    _logger.LogInformation("Execute => ReductionQuery");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception => [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                    errors.Add(ProcessErrorDto.Create(0, $"[InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]", query));
                }
            }

            return errors;
        }

        private static string GetAlterTableQuery(string tableName, bool option)
        {
            var query = SqlStatements.AlterTableNoCheckConstraint(tableName);

            if (option)
            {
                query = SqlStatements.AlterTableCheckConstraint(tableName);
            }
            return query;
        }
    }
}
