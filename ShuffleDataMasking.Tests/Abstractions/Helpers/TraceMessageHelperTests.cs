using ShuffleDataMasking.Domain.Abstractions.Helpers;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace ShuffleDataMasking.Tests.Abstractions.Helpers
{
    public class TraceMessageHelperTests
    {
        [Test]
        public void GetCorrelationIdChangeMessage_ShouldBeSuccessful()
        {
            string result = null;

            Action act = () => result = TraceMessageHelper.GetCorrelationIdChangeMessage(Guid.NewGuid(), Guid.NewGuid());

            act.Should().NotThrow();
            result.Should().NotBeNullOrWhiteSpace();
        }

        [Test]
        public void GetGeneratedByCorrelationIdMessage_ShouldBeSuccessful()
        {
            string result = null;

            Action act = () => result = TraceMessageHelper.GetGeneratedByCorrelationIdMessage(Guid.NewGuid());

            act.Should().NotThrow();
            result.Should().NotBeNullOrWhiteSpace();
        }
    }
}

