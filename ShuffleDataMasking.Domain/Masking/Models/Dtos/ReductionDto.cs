
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShuffleDataMasking.Domain.Masking.Models.Dtos
{
    public class ReductionDto
    {
        public Dictionary<string, List<RelationDto>> Father { get; set; }
        public List<string> ColumnName { get; set; }

        public ReductionDto(Dictionary<string, List<RelationDto>> _father, List<string> _columnName)
        {
            Father = _father;
            ColumnName = _columnName;
        }

        public static ReductionDto Create(Dictionary<string, List<RelationDto>> _father)
        {
            return new ReductionDto(_father, null);
        }

        public static ReductionDto Create(List<string> _columnName)
        {
            return new ReductionDto(null, _columnName);
        }

        public static string StringPrimaryKeys(List<string> _primaryKeys)
        {
            return String.Join(",", _primaryKeys.ToArray());
        }
    }
}
