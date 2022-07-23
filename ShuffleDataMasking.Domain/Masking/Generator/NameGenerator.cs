
using System;
using System.Linq;
using System.Text;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class NameGenerator
    {
        public static string Get()
        {
            StringBuilder name = new(CommonGenerator.StringGenerator(4));
            name.Append(' ');
            name.Append(CommonGenerator.StringGenerator(5));
            name.Append(' ');
            name.Append(CommonGenerator.StringGenerator(5));

            var rest = int.Parse(CommonGenerator.IntegerGenerator(1, 100));

            if ((rest % 2) > 0)
            {
                name.Append(' ');
                name.Append(CommonGenerator.StringGenerator(5));
            }

            return name.ToString().ToUpper();
        }


    }
}
