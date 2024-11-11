using System.Collections.Generic;

namespace SakuraTranslate.Helpers
{
    public static class DictHelper
    {
        public static List<string> GetDictStringList(IEnumerable<KeyValuePair<string, List<string>>> kvPairs)
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

            return dictList;
        }
    }
}