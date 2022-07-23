
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using ShuffleDataMasking.Domain.Masking.Validations;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;

namespace ShuffleDataMasking.Domain.Masking.Messages
{
    public class ShuffleDataMaskingMessage
    {
        private readonly IValidator<ShuffleDataMaskingMessage> _validator;
        private ValidationResult _validationResult;

        public ShuffleDataMaskingMessage()
        {
            _validator = new ShuffleDataMaskingMessageValidator();
        }

        public ShuffleDataMaskingMessage(string _database, string _tableQuery)
        {
            Database = _database;
            TableQuery = _tableQuery;
        }

        public string Database { get; set; }
        public string TableQuery { get; set; }
        public string TableQueryId { get; set; }
        public int StartQuery { get; set; }
        public int EndQuery { get; set; }
        public long QueryProcessId { get; set; }
        public string ErrorProcessQuery { get; set; }
        public long ErrorProcessId { get; set; }
        public List<PrimaryKeyDto> ReductionIds { get; set; }

        public DateTime MessageDate { get; set; }

        public ValidationResult ValidationResult
        {
            get
            {
                if (_validationResult is null)
                {
                    _validationResult = _validator.Validate(this);
                }
                return _validationResult;
            }
        }
        public bool IsValid => ValidationResult.IsValid;

        public static ShuffleDataMaskingMessage Create(string _database, string _tableQuery)
        {
            return new ShuffleDataMaskingMessage(_database, _tableQuery);
        }
    }
}
