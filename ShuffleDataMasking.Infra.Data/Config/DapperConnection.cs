using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Enums;
using Microsoft.Data.SqlClient;

namespace ShuffleDataMasking.Infra.Data.Config
{
    public class DapperConnection : IDapperConnection
    {
        private readonly string _connectionFksString;
        private readonly string _connectionFKSolutionsString;
        private readonly string _connectionShuffleDataMaskingString;

        public DapperConnection(string connectionFksString, string fksolutionsString, string shuffleDataMaskingString)
        {
            _connectionFksString = connectionFksString;
            _connectionFKSolutionsString = fksolutionsString;
            _connectionShuffleDataMaskingString = shuffleDataMaskingString;
        }

        public SqlConnection GetSqlConnection(DatabaseConfig database)
        {
            var connectioString = database switch
            {
                DatabaseConfig.FKS => _connectionFksString,
                DatabaseConfig.FKSOLUTIONS => _connectionFKSolutionsString,
                _ => _connectionShuffleDataMaskingString,
            };
            return new SqlConnection(connectioString);
        }
    }
}
