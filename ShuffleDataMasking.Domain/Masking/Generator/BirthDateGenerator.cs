
using System;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class BirthDateGenerator
    {
        public static string Get()
        {
            DateTime today = DateTime.Today;

            var start = new DateTime(1950, 1, 1);
            var ended = new DateTime((today.Year - 18), 1, 1);

            var gen = new Random();
            var range = (ended - start).Days;

            return start.AddDays(gen.Next(range)).AddHours(gen.Next(0, 24)).AddMinutes(gen.Next(0, 60)).AddSeconds(gen.Next(0, 60)).ToString();
        }
    }
}
