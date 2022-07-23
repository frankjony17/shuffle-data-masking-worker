
using ShuffleDataMasking.Domain.Masking.Enums;

namespace ShuffleDataMasking.Domain.Masking.Entities
{
    public class IntrospectionColumn
    {
        public IntrospectionColumn()
        {

        }

        public IntrospectionColumn(long _id, string _columnName, TypeOfMask _typeOfMask)
        {
            Id = _id;
            ColumnName = _columnName;
            TypeOfMask = _typeOfMask;
        }

        public long Id { get; set; }
        public string ColumnName { get; set; }
        public TypeOfMask TypeOfMask { get; set; }

        ///* EF Relations */
        //public IntrospectionTable IntrospectionTable { get; private set; }
    }
}
