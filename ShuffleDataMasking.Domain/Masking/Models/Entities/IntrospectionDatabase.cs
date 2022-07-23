
using ShuffleDataMasking.Domain.Abstractions.Entities;
using System;
using System.Collections.Generic;

namespace ShuffleDataMasking.Domain.Masking.Entities
{
    public class IntrospectionDatabase
    {
        public IntrospectionDatabase(long _id, string _databaseName)
        {
            Id = _id;
            DatabaseName = _databaseName;
        }

        public long Id { get; set; }
        public string DatabaseName { get; set; }
        public bool ConstraintDisabled { get; set; }
        public bool ProcessStarted { get; set; }
    }

}
