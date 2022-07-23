
using System;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class RgGenerator
    {
        public static string Get()
        {
            return CommonGenerator.IntegerGenerator(1000000, 999999999).ToString();
        }
    }
}
