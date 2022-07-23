using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace ShuffleDataMasking.Domain.Abstractions.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public sealed class GenericDomainException : DomainException
    {
        public GenericDomainException(string message) : base(message) { }

        public GenericDomainException(string message, Exception innerException) : base(message, innerException) { }

        private GenericDomainException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}

