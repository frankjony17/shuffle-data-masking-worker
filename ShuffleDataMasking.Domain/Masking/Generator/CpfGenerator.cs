
using System;

namespace ShuffleDataMasking.Domain.Masking.Generator
{
    public static class CpfGenerator
    {
        public static string Get()
        {
            int sum = 0;
            int[] multiplierOne = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplierTwo = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            Random rnd = new();
            string seed = rnd.Next(100000000, 999999999).ToString();

            for (int i = 0; i < 9; i++)
            {
                sum += int.Parse(seed[i].ToString()) * multiplierOne[i];
            }

            int rest = sum % 11;

            if (rest < 2)
            {
                rest = 0;
            }
            else
            {
                rest = 11 - rest;
            }

            seed += rest;
            sum = 0;

            for (int i = 0; i < 10; i++)
            {
                sum += int.Parse(seed[i].ToString()) * multiplierTwo[i];
            }   

            rest = sum % 11;

            if (rest < 2)
            {
                rest = 0;
            }   
            else
            {
                rest = 11 - rest;
            }

            seed += rest;

            return seed;
        }
    }
}
