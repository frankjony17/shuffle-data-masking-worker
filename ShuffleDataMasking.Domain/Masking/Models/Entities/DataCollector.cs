
using ShuffleDataMasking.Domain.Abstractions.Entities;

namespace ShuffleDataMasking.Domain.Masking.Entities
{
    public class DataCollector : Entity
    {
        public DataCollector()
        {
            
        }

        public DataCollector(string _originalData, string _maskedData, int _maskedType)
        {
            OriginalData = _originalData;
            MaskedData = _maskedData;
            MaskedTypeId = _maskedType;
            SetCreatedAtAndCreatedBy();
        }

        public long DataCollectorId { get; set; }
        public string OriginalData { get; set; }
        public string MaskedData { get; set; }
        public int MaskedTypeId { get; set; }

        public static DataCollector Create(string _originalData, string _maskedData, int _maskedType)
        {
            return new DataCollector(_originalData, _maskedData, _maskedType);
        }
    }
}
