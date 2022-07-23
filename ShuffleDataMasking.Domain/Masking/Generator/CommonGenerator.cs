using System;
using System.Collections.Generic;
using System.Linq;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static partial class CommonGenerator
    {
        internal static readonly Random random = new();

        internal static int CalculaDigitoVerificador(int n) => n < 2 ? 0 : 11 - n;

        internal static int CalculaSoma(IEnumerable<int> xs, IEnumerable<int> ys) =>
            xs.Zip(ys, (x, y) => x * y).Sum();

        internal static int CriaDigitoVerificador(IEnumerable<int> xs, IEnumerable<int> ys) =>
            CalculaDigitoVerificador(CalculaSoma(xs, ys) % 11);

        internal static IEnumerable<int> NumerosRandom(int count, int min, int max) =>
            Enumerable.Range(0, count).Select((_, index) => random.Next(index == 0 ? 1 : min, max - 1)).ToList();

        internal static IEnumerable<int> CalculateCheckDigits(IEnumerable<int> xs, IEnumerable<IEnumerable<int>> zipper) =>
            zipper.Aggregate(xs, (a, b) => a.Concat(new[] { CriaDigitoVerificador(b, a) })).Skip(xs.Count());

        internal static string StringGenerator(int length)
        {
            const string chars = "qwertyuiopasdfghjklzxcvbnm";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal static string IntegerGenerator(int min, int max)
        {
            return random.Next(min, max).ToString();
        }
    }
}
