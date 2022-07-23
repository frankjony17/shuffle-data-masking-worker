
using System;
using System.Text;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class TelephoneGenerator
    {
        public static string Get()
        {
            StringBuilder telephone = new(CommonGenerator.IntegerGenerator(100, 999));

            telephone.Append(CommonGenerator.IntegerGenerator(100000, 999999));

            return telephone.ToString();
        }
    }
}
