using ShuffleDataMasking.Domain.Abstractions.Entities;
using ShuffleDataMasking.Domain.Abstractions.Helpers;
using System;

namespace ShuffleDataMasking.Domain.Abstractions.Extensions
{
    public static class EntityExtensions
    {
        public static void SetCreatedAtAndCreatedByAsNew(this Entity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.CreatedAt = DateTime.Now;
            entity.CreatedBy = CreatorHelper.GetEntityCreatorIdentity();
        }
    }
}

