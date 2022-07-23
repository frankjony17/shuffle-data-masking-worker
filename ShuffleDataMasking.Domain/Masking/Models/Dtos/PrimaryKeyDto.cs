
using System;
using System.Collections.Generic;
using System.Text;

namespace ShuffleDataMasking.Domain.Masking.Models.Dtos
{
    public class PrimaryKeyDto
    {
        public string ColumnName { get; set; }
        public List<string> PrimaryKeys { get; set; }

        public PrimaryKeyDto(string _columnName, List<string> _primaryKeys)
        {
            ColumnName = _columnName;
            PrimaryKeys = _primaryKeys;
        }


        public static PrimaryKeyDto Create(string _columnName, List<string> _primaryKeys)
        {
            return new PrimaryKeyDto(_columnName, _primaryKeys);
        }

        public string StringPrimaryKeys()
        {
            var keys = String.Join(",", PrimaryKeys.ToArray());
            
            if (keys == string.Empty)
            {
                keys = "0";
            }
            return keys;
        }

        public static string GetVaulesInsert(string column_name, List<string> primaryKeys, string fatherTableId)
        {
            StringBuilder sqlQuery = new();

            foreach (string key in primaryKeys)
            {
                sqlQuery.Append($"({column_name}, {key}, {fatherTableId})");
            }

            return sqlQuery.ToString();
        }
    }
}
