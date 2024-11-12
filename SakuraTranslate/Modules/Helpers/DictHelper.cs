using SakuraTranslate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        public static string GetDictStr(IEnumerable<KeyValuePair<string, List<string>>> kvPairs)
        {
            List<string> dictList = new List<string>();
            foreach (var entry in kvPairs)
            {
                var src = entry.Key;
                var dst = entry.Value[0];
                var info = entry.Value[1];
                if (string.IsNullOrEmpty(info))
                {
                    dictList.Add($"{src}->{dst}");
                }
                else
                {
                    dictList.Add($"{src}->{dst} #{info}");
                }
            }
            return string.Join("\n", dictList.ToArray());
        }

        public string GetDictStr(string originalText)
        {
            string dictStr;
            if (_dictMode == DictMode.None)
            {
                dictStr = null;
            }
            if (_dictMode == DictMode.Full)
            {
                dictStr = _fullDictStr;
            }
            else if (_dictMode == DictMode.MatchOriginal)
            {
                var usedDict = _dict.Where(x => originalText.Contains(x.Key));
                dictStr = usedDict.Any() ? GetDictStr(usedDict) : null;
            }
            else
            {
                throw new Exception("Invalid dict mode.");
            }

            return dictStr;
        }
    }
}