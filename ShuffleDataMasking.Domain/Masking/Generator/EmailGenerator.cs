
using System;
using System.Linq;
using System.Text;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class EmailGenerator
    {
        public static string Get()
        {   
            var name = CommonGenerator.StringGenerator(15);

            StringBuilder email = new(name);
            email.Append("@fksolutions.teste.com.br");

            return email.ToString();
        }
    }
}
