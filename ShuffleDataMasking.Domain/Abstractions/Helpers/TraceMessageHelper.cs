using System;

namespace ShuffleDataMasking.Domain.Abstractions.Helpers
{
    public static class TraceMessageHelper
    {
        public static string GetCorrelationIdChangeMessage(Guid correlationId, Guid newCorrelationId)
            => $"New correlation id generated: {newCorrelationId}. Derived from: {correlationId}.";

        public static string GetGeneratedByCorrelationIdMessage(Guid correlationId)
            => $"Generated by correlation id: {correlationId}.";
    }
}

