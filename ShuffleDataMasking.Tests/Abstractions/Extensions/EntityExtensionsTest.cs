using ShuffleDataMasking.Domain.Abstractions.Entities;
using ShuffleDataMasking.Domain.Abstractions.Extensions;
using ShuffleDataMasking.Domain.Abstractions.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace ShuffleDataMasking.Tests.Abstractions.Extensions
{
    public class EntityExtensionsTest
    {
        [Test]
        public void SetCreatedAtAndCreatedByAsNew_ShouldBeSuccessful()
        {
            var entityInstance = new ConcreteTestEntity();

            Action act = () => EntityExtensions.SetCreatedAtAndCreatedByAsNew(entityInstance);

            act.Should().NotThrow();
            entityInstance.CreatedAt.Should().BeCloseTo(DateTime.Now);
            entityInstance.CreatedBy.Should().Be(CreatorHelper.GetEntityCreatorIdentity());
        }

        [Test]
        public void SetCreatedAtAndCreatedByAsNew_WhenNullEntity_ShouldFail()
        {
            Action act = () => EntityExtensions.SetCreatedAtAndCreatedByAsNew(null);

            act.Should().ThrowArgumentNullException("entity");
        }

        public class ConcreteTestEntity : Entity { }
    }
}

