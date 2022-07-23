
using System.Collections.Generic;
using System.Linq;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class CnpjGenerator
    {
        static readonly int[] cnpjZipper = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        static readonly IEnumerable<int>[] cnpjZipperList = new[] { cnpjZipper.Skip(1), cnpjZipper };

        public static string Get()
        {
            return ConcatCheckDigits(CommonGenerator.NumerosRandom(12, 0, 9)).Aggregate(0L, (acc, x) => acc * 10 + x).ToString();
        }

        private static IEnumerable<int> ConcatCheckDigits(IEnumerable<int> xs) =>
            xs.Concat(CalculateCheckDigits(xs));

        public static IEnumerable<int> CalculateCheckDigits(IEnumerable<int> xs) =>
            CommonGenerator.CalculateCheckDigits(xs, cnpjZipperList);
    }
}
