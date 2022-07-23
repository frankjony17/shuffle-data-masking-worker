using ShuffleDataMasking.Domain.Abstractions.Exceptions;
using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Messages;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Services
{
    public class ReductionService : IReductionService
    {
        private readonly ILogger<ReductionService> _logger;
        private readonly IForeignKeysService _foreignKeysService;
        private readonly IIntrospectionDapperRepository _introspectionTabletDapperRepository;
        private readonly IShuffleDataMaskingDapperRepository _shuffleDataMaskingDapperRepository;

        private DatabaseConfig _databaseConfig;
        private readonly List<string> _deleteQueryList;
        private readonly Dictionary<string, ReductionDto> _reductionTableDictionary;
        private const string _temporalQuery = "SELECT PrimaryKey FROM TEMPORAL_PRIMARY_KEY WHERE ";
        private int _totalRows;
        private const string DISABLEALLFOREIGNKEYS = "DisableAllForeignKeys";

        public ReductionService(
            ILogger<ReductionService> logger,
            IForeignKeysService foreignKeysService,
            IIntrospectionDapperRepository introspectionTabletDapperRepository,
            IShuffleDataMaskingDapperRepository shuffleDataMaskingDapperRepository)
        {
            _logger = logger;
            _foreignKeysService = foreignKeysService;
            _introspectionTabletDapperRepository = introspectionTabletDapperRepository;
            _shuffleDataMaskingDapperRepository = shuffleDataMaskingDapperRepository;
            _deleteQueryList = new();
            _reductionTableDictionary = new();
        }

        public async Task<bool> IsActionToReductionAsync(ShuffleDataMaskingMessage message, DatabaseConfig databaseConfig)
        {
            if (message.ReductionIds.Count > 0)
            {
                _databaseConfig = databaseConfig;
                await StartReductionProcess(message);
                return true;
            }
            return false;
        }

        private async Task StartReductionProcess(ShuffleDataMaskingMessage message)
        {
            try
            {
                _logger.LogInformation($"Start queue process. [QueryProcessId = {message.QueryProcessId}]");
                _introspectionTabletDapperRepository.UpdateQueueStartProcess(DateTime.Now, message.QueryProcessId);

                _logger.LogInformation($"Get processed masking. [QueryProcessId = {message.QueryProcessId}]");
                var processedMasking = await _introspectionTabletDapperRepository.SelectProcessedMasking(message.QueryProcessId);

                _logger.LogInformation($"Getting reduction querys.");
                await GetReductionQueryListAsync(message);

                _logger.LogInformation($"Excecute reduction querys.");
                await ExcecuteReductionQuery(message, processedMasking);

                _introspectionTabletDapperRepository.UpdateQueueEndedProcess(DateTime.Now, message.QueryProcessId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Start reduction process. [Database = {message.Database}] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Start reduction process.", ex);
            }
        }

        private async Task ExcecuteReductionQuery(ShuffleDataMaskingMessage message, int processedMasking)
        {
            _ = await _foreignKeysService.IsActionToForeignKeysAsync(ShuffleDataMaskingMessage.Create(message.Database, DISABLEALLFOREIGNKEYS), _databaseConfig);

            _logger.LogInformation($"Excecute reduction query.");
            var reductionError = await _shuffleDataMaskingDapperRepository.ExcecuteReductionQuery(_deleteQueryList, processedMasking, _databaseConfig);

            if (reductionError.Count > 0)
            {
                _logger.LogInformation($"Execute => insert error in database. [Table = {message.TableQuery}]");
                await _introspectionTabletDapperRepository.SaveProcessErrorAsync(reductionError, ErrorType.DATABASE_REDUCTION_PROCESS);
            }
        }

        private async Task GetReductionQueryListAsync(ShuffleDataMaskingMessage message)
        {
            // Steep 0 -> Create Temporal Table.
            await _shuffleDataMaskingDapperRepository.CreateTemporalTable(_databaseConfig);

            // Steep 1 -> Get total father ids (Total of table rows).
            _totalRows = message.EndQuery;
            await SaveFirstFatherPrimaryKeyAsync(message.TableQuery, message.ReductionIds);

            // Steep 2 -> Update DeleteQueryList.
            UpdateDeleteQuery(_reductionTableDictionary[message.TableQuery], message.TableQuery);

            // Steep 3 -> Get all secondary table from PrinciplaTable.
            await GetSecondaryTable(message.TableQuery, string.Empty);

            await StartReductionTables();

            ProcessDeleteQuerys();

            _deleteQueryList.Reverse();
        }

        private async Task StartReductionTables()
        {
            int dictionaryCount = _reductionTableDictionary.Count;

            for (int index = 1; index < dictionaryCount; index++)
            {
                try
                {
                    var reductionTable = _reductionTableDictionary.ElementAt(index);

                    await CreatePrimaryKeyForTable(reductionTable);

                    await GetSecondaryTable(reductionTable.Key, string.Empty);

                    dictionaryCount = _reductionTableDictionary.Count;

                    _logger.LogInformation($"Process => Table = {reductionTable.Key}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception ==> Get primary key dtos async. [Method = GetPrimaryKeyDtosAsync()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                }
            }
        }

        private void ProcessDeleteQuerys()
        {
            for (int index = 1; index < _reductionTableDictionary.Count; index++)
            {
                var reductionTable = _reductionTableDictionary.ElementAt(index);

                UpdateDeleteQuery(reductionTable.Value, reductionTable.Key);
            }
        }

        private async Task CreatePrimaryKeyForTable(KeyValuePair<string, ReductionDto> reductionTable)
        {
            try
            {
                var primariKeyQuery = await BuildPrimariKeyQueryAsync(reductionTable);

                foreach (string query in primariKeyQuery)
                {
                    await _shuffleDataMaskingDapperRepository.RunQuery(_databaseConfig, query);
                }
                _logger.LogInformation($"Update => Primary keys Table={reductionTable.Key}.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Get primary key dtos async. [Method = GetPrimaryKeyDtosAsync()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
            }
        }

        private async Task<List<string>> BuildPrimariKeyQueryAsync(KeyValuePair<string, ReductionDto> reductionTable)
        {
            var primariKeyQueryList = new List<string>();
            try
            {
                var columnNameOfPrimaryKey = await FindColumnNameOfPrimaryKey(reductionTable.Key);

                if (columnNameOfPrimaryKey == null || !columnNameOfPrimaryKey.Any())
                {
                    return primariKeyQueryList;
                }

                StringBuilder whereQuery = new("WHERE ");

                foreach (KeyValuePair<string, List<RelationDto>> father in reductionTable.Value.Father)
                {
                    foreach (RelationDto relation in father.Value)
                    {
                        whereQuery.Append($"{relation.ColumnName} IN ({_temporalQuery}ColumnName = '{relation.FatherColumnName}' AND FatherTableName = '{father.Key}') AND ");
                    }
                }
                whereQuery.Remove(whereQuery.Length - 4, 4);

                foreach (string columnName in columnNameOfPrimaryKey)
                {
                    StringBuilder insertPrimaryKeyQuery = new($"INSERT INTO TEMPORAL_PRIMARY_KEY (ColumnName, PrimaryKey, FatherTableName) ");
                    insertPrimaryKeyQuery.Append($"SELECT TOP({_totalRows}) '{columnName}', {columnName}, '{reductionTable.Key}' FROM {reductionTable.Key} ");

                    insertPrimaryKeyQuery.Append(whereQuery);
                    insertPrimaryKeyQuery.Append("ORDER BY NEWID()");

                    primariKeyQueryList.Add(insertPrimaryKeyQuery.ToString());
                }

                _reductionTableDictionary[reductionTable.Key].ColumnName = columnNameOfPrimaryKey.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Get primari key query async. [Table = {reductionTable.Key}] - [Method = GetPrimariKeyQueryAsync()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
            }
            return primariKeyQueryList;
        }

        private async Task GetSecondaryTable(string tableName, string fatherTableName)
        {
            var relationTableList = await _introspectionTabletDapperRepository.FindAllSecondaryTable(tableName, fatherTableName);

            foreach (RelationDto relationTable in relationTableList)
            {
                try
                {
                    if (_reductionTableDictionary.ContainsKey(relationTable.TableName))
                    {
                        if (_reductionTableDictionary[relationTable.TableName].Father.ContainsKey(tableName))
                        {
                            _reductionTableDictionary[relationTable.TableName].Father[tableName].Add(relationTable);
                        }
                        else
                        {
                            _reductionTableDictionary[relationTable.TableName].Father.Add(tableName, new List<RelationDto>() { relationTable });
                        }
                    }
                    else
                    {
                        Dictionary<string, List<RelationDto>> father = new() { { tableName, new List<RelationDto>() { relationTable } } };
                        _reductionTableDictionary.Add(relationTable.TableName, ReductionDto.Create(father));
                    }
                    _logger.LogInformation($"Get => Secondary Table={relationTable.TableName}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Exception ==> Get secondary table. [Table = GetSecondaryTable()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                }
            }
        }

        private void UpdateDeleteQuery(ReductionDto reductionTable, string tableName)
        {
            try
            {
                StringBuilder deleteQuery = new($"DELETE FROM {tableName} ");
                StringBuilder whereQuery = new("WHERE ");

                if (reductionTable.ColumnName != null)
                {
                    foreach (string column in reductionTable.ColumnName)
                    {
                        whereQuery.Append($"{column} NOT IN ({_temporalQuery} ColumnName = '{column}' AND FatherTableName = '{tableName}') AND ");
                    }
                }
                else
                {
                    foreach (KeyValuePair<string, List<RelationDto>> father in reductionTable.Father)
                    {
                        foreach (RelationDto relation in father.Value)
                        {
                            whereQuery.Append($"{relation.ColumnName} NOT IN ({_temporalQuery}ColumnName = '{relation.FatherColumnName}' AND FatherTableName = '{father.Key}') AND ");
                        }
                    }
                }
                whereQuery.Remove(whereQuery.Length - 4, 4);
                deleteQuery.Append(whereQuery);

                _deleteQueryList.Add(deleteQuery.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Update delete query. [Method = UpdateDeleteQuery()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
            }
        }

        private async Task SaveFirstFatherPrimaryKeyAsync(string tableName, List<PrimaryKeyDto> reductionIds)
        {
            try
            {
                List<string> columnNameList = new();

                foreach (PrimaryKeyDto reduction in reductionIds)
                {
                    _shuffleDataMaskingDapperRepository.SavetFirstFatherPrimaryKey(reduction, tableName, _databaseConfig);
                    columnNameList.Add(reduction.ColumnName);
                }
                _reductionTableDictionary.Add(tableName, ReductionDto.Create(columnNameList));

                int totalValue = TotalCount(reductionIds, _totalRows);

                if (totalValue > 0)
                {
                    foreach (PrimaryKeyDto reduction in reductionIds)
                    {
                        StringBuilder query = new("INSERT INTO TEMPORAL_PRIMARY_KEY (ColumnName, PrimaryKey, FatherTableName) ");
                        query.Append($"SELECT TOP({totalValue}) '{reduction.ColumnName}', {reduction.ColumnName}, '{tableName}' FROM {tableName} ");
                        query.Append($"WHERE {reduction.ColumnName} NOT IN (SELECT PrimaryKey FROM TEMPORAL_PRIMARY_KEY WHERE ");
                        query.Append($"ColumnName = '{reduction.ColumnName}' AND FatherTableName = '{tableName}') ORDER BY NEWID()");

                        await _shuffleDataMaskingDapperRepository.InsertPrimaryKeyTotalRecordAsync(query.ToString(), _databaseConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Save first father primary key. [Method = SaveFirstFatherPrimaryKeyAsync()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw new GenericDomainException($"Exception ==> Get first father primary key.", ex);
            }
        }

        private async Task<IEnumerable<string>> FindColumnNameOfPrimaryKey(string tableName)
        {
            try
            {
                return await _shuffleDataMaskingDapperRepository.FindColumnNameOfPrimaryKey(tableName, _databaseConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Add primary keys. [Table = AddPrimaryKeys()] - [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
            }
            return null;
        }

        private static int TotalCount(List<PrimaryKeyDto> reductionIds, int totalIds)
        {
            int total = totalIds;

            foreach (PrimaryKeyDto keyDto in reductionIds)
            {
                total -= keyDto.PrimaryKeys.Count;
            }

            return total;
        }
    }
}
