
using ShuffleDataMasking.Domain.Masking.Models.Dtos;
using System.Collections.Generic;

namespace ShuffleDataMasking.Domain.Masking.Entities
{
    public class IntrospectionRelativetable
    {
        public IntrospectionRelativetable()
        {

        }

        public IntrospectionRelativetable(string _principalTableName, string _secondaryTableName, string _principalColumnName, string _secondaryColumnName, string _secondaryColumPrimaryKey)
        {
            PrincipalTableName = _principalTableName;
            SecondaryTableName = _secondaryTableName;
            PrincipalColumnName = _principalColumnName;
            SecondaryColumnName = _secondaryColumnName;
            SecondaryColumnPrimaryKey = _secondaryColumPrimaryKey;
        }

        public string PrincipalTableName { get; set; }
        public string SecondaryTableName { get; set; }
        public string PrincipalColumnName { get; set; }        
        public string SecondaryColumnName { get; set; }
        public string SecondaryColumnPrimaryKey { get; set; }
    }
}
