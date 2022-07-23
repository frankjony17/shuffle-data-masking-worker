
namespace ShuffleDataMasking.Domain.Masking.Enums
{
    public enum ErrorType
    {
        ALTER_TABLE_NOCHECK_CONSTRAINT = 1,
        UPDATE_MASKED_DATA_IN_TABLA = 2,
        ALTER_TABLE_WITH_CHECK_CONSTRAINT = 3,
        DATABASE_REDUCTION_PROCESS = 4
    }
}
