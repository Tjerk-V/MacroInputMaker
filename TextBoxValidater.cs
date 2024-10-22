using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AutoClicker
{
    static internal class TextBoxValidater
    {
        private static readonly Regex _regex = new("[^0-9.-]+");
        public static bool IsNumerical(string text)
        {
            return _regex.IsMatch(text);
        }
    }
}
