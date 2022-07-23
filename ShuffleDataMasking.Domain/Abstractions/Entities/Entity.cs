using ShuffleDataMasking.Domain.Abstractions.Helpers;
using System;

namespace ShuffleDataMasking.Domain.Abstractions.Entities
{
    public abstract class Entity
    {
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        protected void SetCreatedAtAndCreatedBy()
        {
            CreatedAt = DateTime.Now;
            CreatedBy = CreatorHelper.GetEntityCreatorIdentity();
        }
    }
}

