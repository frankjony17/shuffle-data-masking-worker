
namespace ShuffleDataMasking.Domain.Masking.Models.Dtos
{
    public class RelationDto
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string FatherColumnName { get; set; }

        public RelationDto()
        {

        }

        public RelationDto(string _tableName, string _columnName, string _fatherColumnName)
        {
            TableName = _tableName;
            ColumnName = _columnName;
            FatherColumnName = _fatherColumnName;
        }


        public static RelationDto Create(string _tableName, string _columnName, string _fatherColumnName)
        {
            return new RelationDto(_tableName, _columnName, _fatherColumnName);
        }
    }
}
