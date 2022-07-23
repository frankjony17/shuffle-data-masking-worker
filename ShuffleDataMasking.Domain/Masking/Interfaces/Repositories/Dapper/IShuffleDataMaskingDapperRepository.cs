using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper
{
    public interface IShuffleDataMaskingDapperRepository
    {
        Task<List<ProcessErrorDto>> DisableForeignKeysAsync(IEnumerable<IntrospectionTable> introspectionTables, DatabaseConfig database);
        Task<List<ProcessErrorDto>> EnableForeignKeysAsync(IEnumerable<IntrospectionTable> introspectionTables, DatabaseConfig database);
        Task<IEnumerable<Object>> FindRecordByQuery(string selectQuery, SqlConnection connection);
        Task<IEnumerable<Object>> FindRecordByQuery(DatabaseConfig database, string selectQuery);
        Task<ProcessErrorDto> UpdateColumnsAsync(string updateQuery, long tableId, long queueId, int processedMasking, SqlConnection connDatabase, SqlConnection connMaskings);
        Task RunQuery(DatabaseConfig database, string query);
        Task<List<ProcessErrorDto>> ExcecuteReductionQuery(List<string> deleteQueryList, int processedMasking, DatabaseConfig databaseConfig);
        Task<IEnumerable<string>> FindColumnNameOfPrimaryKey(string tableName, DatabaseConfig database);
        Task CreateTemporalTable(DatabaseConfig databaseConfig);
        Task RemoveTemporalTable(DatabaseConfig databaseConfig);
        void SavetFirstFatherPrimaryKey(PrimaryKeyDto reduction, string tableName, DatabaseConfig database);
        Task InsertPrimaryKeyTotalRecordAsync(string query, DatabaseConfig database);
    }
}

