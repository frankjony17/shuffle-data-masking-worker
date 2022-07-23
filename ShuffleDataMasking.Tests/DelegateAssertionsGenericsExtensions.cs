using FluentAssertions;
using FluentAssertions.Specialized;
using System;

namespace ShuffleDataMasking.Tests
{
    public static class DelegateAssertionsGenericsExtensions
    {
        public static ExceptionAssertions<ArgumentNullException> ThrowArgumentNullException<TDelegate>(
            this DelegateAssertions<TDelegate> delegateAssertions)
            where TDelegate : Delegate =>
            delegateAssertions.Throw<ArgumentNullException>();

        public static ExceptionAssertions<ArgumentNullException> ThrowArgumentNullException<TDelegate>(
            this DelegateAssertions<TDelegate> delegateAssertions,
            string paramName)
            where TDelegate : Delegate
        {
            var exceptionAssertions = delegateAssertions.ThrowArgumentNullException();

            exceptionAssertions.And.ParamName.Should().Be(paramName);

            return exceptionAssertions;
        }
    }
}

