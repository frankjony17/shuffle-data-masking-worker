using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper
{
    public interface IIntrospectionDapperRepository
    {
        Task<IEnumerable<IntrospectionTable>> FindAllIntrospectionTable(DatabaseConfig _database);
        Task<IEnumerable<IntrospectionColumn>> FindAllIntrospectionColumns(long _tableId);
        Task SaveProcessErrorAsync(List<ProcessErrorDto> _tableError, ErrorType errorTypeId);
        Task<DataCollector> GetMaskedData(string originalData, int typeOfMaskId);
        Task<int> GetCountMaskedData(string maskedData);

        void UpdateConstraintDisabled(int constraintDisabled, string database);
        void UpdateProcessStarted(int processStarted, string database);
        void UpdateQueueStartProcess(DateTime startProcess, long queueId);
        void UpdateQueueEndedProcess(DateTime endedProcess, long queueId);
        Task<int> SelectProcessedMasking(long queueId);
        void RemoveErrorFromDatabase(long errorId);
        void UpdateProcessedMasking(int rowCounter, long queryProcessId);

        Task<IEnumerable<RelationDto>> FindAllSecondaryTable(string tableName, string fatherTableName);
    }
}

