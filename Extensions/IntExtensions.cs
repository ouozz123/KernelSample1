using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KernelSample.Extensions
{
    public static class IntExtensions
    {
        public static int ToInt(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            if (int.TryParse(str, out int result))
            {
                return result;
            }

            return 0;
        }
    }
}