
namespace ShuffleDataMasking.Domain.Masking.Models.Dtos
{
    public class ProcessErrorDto
    {
        public long TableId { get; set; }
        public string ErrorDescription { get; set; }
        public string OriginalQuery { get; set; }
        public long QueryProcessId { get; set; }

        public ProcessErrorDto(long _tableId, string _errorDescription, string _original_query, long _queueId)
        {
            TableId = _tableId;
            ErrorDescription = _errorDescription;
            OriginalQuery = _original_query;
            QueryProcessId = _queueId;
        }


        public static ProcessErrorDto Create(long _tableId, string _errorDescription, string _original_query, long _queueId=0)
        {
            return new ProcessErrorDto(_tableId, _errorDescription, _original_query, _queueId);
        }
    }
}
