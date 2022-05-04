using System;

namespace Soma.Build
{
    public static class DefineSymbolsUtils
    {
        private const string JoinSeparator = ";";
        private static readonly char[] s_splitDivider = {';'};

        public static string[] SplitDefineSymbolString(string defineSymbols)
        {
            var split = defineSymbols.Split(s_splitDivider, StringSplitOptions.RemoveEmptyEntries);

            return split;
        }

        public static string MergeDefineSymbols(string[] defineSymbols)
        {
            var merge = string.Join(JoinSeparator, defineSymbols);

            return merge;
        }
    }
}
