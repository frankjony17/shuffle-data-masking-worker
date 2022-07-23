using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace ShuffleDataMasking.Domain.Abstractions.Exceptions
{
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class MessageNotValidException : DomainException
    {
        public MessageNotValidException()
            : base("Message not valid") { }

        protected MessageNotValidException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

    }
}
