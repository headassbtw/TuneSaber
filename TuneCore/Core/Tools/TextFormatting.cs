using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TuneSaber.Core.Tools
{
    class TextFormatting
    {
        public static string NumberCommas(string input)
        {
            int chars = input.Length;
            int offset = 3;
            while (offset < chars)
            {
                input.Insert(chars - offset, ",");
                offset += 4;
            }
            return input;
        }
    }
}
