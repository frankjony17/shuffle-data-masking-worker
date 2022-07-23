using ShuffleDataMasking.Domain.Abstractions.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace ShuffleDataMasking.Tests.Abstractions.Helpers
{
    public class CreatorHelperTests
    {
        [Test]
        public void GetEntityCreatorIdentity_ShouldBeSuccessful()
        {
            string result = null;

            Action act = () => result = CreatorHelper.GetEntityCreatorIdentity();

            act.Should().NotThrow();
            result.Should().NotBeNullOrWhiteSpace();
        }
    }
}

