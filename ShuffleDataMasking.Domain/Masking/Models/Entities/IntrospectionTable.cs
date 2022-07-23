
using System;

namespace ShuffleDataMasking.Domain.Masking.Entities
{
    public class IntrospectionTable
    {
        public IntrospectionTable()
        {

        }

        public IntrospectionTable(long _id, string _tableName, long _databaseId)
        {
            Id = _id;
            TableName = _tableName;
            DatabaseId = _databaseId;
        }

        public long Id { get; set; }
        public string TableName { get; set; }
        public long DatabaseId { get; set; }
    }
}
