using ShuffleDataMasking.Domain.Masking.Entities;
using ShuffleDataMasking.Domain.Masking.Enums;
using ShuffleDataMasking.Domain.Masking.Generator;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.EntityFramework;
using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ShuffleDataMasking.Domain.Masking.Services
{
    public class MaskGeneratorService : IMaskGeneratorService
    {
        private readonly ILogger<ShuffleDataMaskingService> _logger;
        private readonly IDataCollectorEfRepository _dataCollectorEfRepository;
        private readonly IIntrospectionDapperRepository _introspectionDapperRepository;
        private const int UNIQUE_CONSTRAINT_ERROR = 2627;
        private const int DUPLICATED_KEY_ERROR = 2601;

        public MaskGeneratorService(
            ILogger<ShuffleDataMaskingService> logger,
            IDataCollectorEfRepository dataCollectorEfRepository,
            IIntrospectionDapperRepository introspectionDapperRepository)
        {
            _logger = logger;
            _dataCollectorEfRepository = dataCollectorEfRepository;
            _introspectionDapperRepository = introspectionDapperRepository;
        }

        public async Task<string> GetMaskingForColumn(string columnValue, TypeOfMask typeOfMask)
        {
            var maskedValue = await GetMaskedData(columnValue, (int)typeOfMask);

            if (maskedValue != string.Empty)
            {
                return maskedValue;
            }

            maskedValue = await GenerateValueMaskAsync(columnValue, typeOfMask);

            return maskedValue;
        }

        private static string GetCpfCnpjOrGenerateOne(string columnValue)
        {
            if (columnValue.Length > 11)
            {
                return CnpjGenerator.Get();
            }
            return CpfGenerator.Get();
        }

        private async Task<string> GenerateValueMaskAsync(string columnValue, TypeOfMask typeOfMask)
        {
            var columnMask = typeOfMask switch
            {
                TypeOfMask.NOME => NameGenerator.Get(),
                TypeOfMask.ADDRESS => CommonGenerator.StringGenerator(100).ToString(),
                TypeOfMask.SOCIAL_REASON => NameGenerator.Get(),
                TypeOfMask.BIRTH_DATE => BirthDateGenerator.Get(),
                TypeOfMask.REMUNERATION => CommonGenerator.IntegerGenerator(100, 999),
                _ => await GetUniqueMaskAsync(columnValue, typeOfMask),
            };

            return columnMask;
        }

        private async Task<string> GetUniqueMaskAsync(string columnValue, TypeOfMask typeOfMask)
        {
            int countMask = 1;
            string columnMask = columnValue;

            while (countMask > 0)
            {
                columnMask = typeOfMask switch
                {
                    TypeOfMask.RG => RgGenerator.Get(),
                    TypeOfMask.CPFCNPJ => GetCpfCnpjOrGenerateOne(columnValue),
                    TypeOfMask.EMAIL => EmailGenerator.Get(),
                    TypeOfMask.TELEPHONE => TelephoneGenerator.Get(),
                    _ => columnValue,
                };

                countMask = await _introspectionDapperRepository.GetCountMaskedData(columnMask);
            }

            return await SaveGenerateDataAsync(columnValue, columnMask, typeOfMask);
        }


        private async Task<string> SaveGenerateDataAsync(string columnValue, string generateValue, TypeOfMask typeOfMask)
        {
            try
            {
                _dataCollectorEfRepository.Save(DataCollector.Create(columnValue, generateValue, (int)typeOfMask));
                return generateValue;
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlException)
            {
                switch (sqlException.Errors[0].Number)
                {
                    case UNIQUE_CONSTRAINT_ERROR:
                    case DUPLICATED_KEY_ERROR:
                        return await GetMaskedData(columnValue, (int)typeOfMask);
                    default:
                        _logger.LogError($"Exception ==> Save generate data. [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                        throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception ==> Save generate data. [InnerException = {ex.InnerException?.Message}] - [ErrorMessage = {ex.Message}]");
                throw;
            }
        }

        private async Task<string> GetMaskedData(string columnValue, int typeOfMaskId)
        {
            var originalData = await _introspectionDapperRepository.GetMaskedData(columnValue, typeOfMaskId);

            if (originalData != null)
            {
                return originalData.MaskedData;
            }

            return string.Empty;
        }
    }
}
