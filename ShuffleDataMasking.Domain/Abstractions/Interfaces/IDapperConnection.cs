using ShuffleDataMasking.Domain.Masking.Enums;
using Microsoft.Data.SqlClient;

namespace ShuffleDataMasking.Domain.Abstractions.Interfaces
{
    public interface IDapperConnection
    {
        SqlConnection GetSqlConnection(DatabaseConfig database);
    }
}
