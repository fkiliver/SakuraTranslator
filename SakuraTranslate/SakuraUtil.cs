namespace SakuraTranslate
{
    public static class SakuraUtil
    {
        public static string FixTranslationEnd(string original, string translation)
        {
            if (translation.EndsWith("。") && !original.EndsWith("。"))
            {
                translation = translation.Substring(0, translation.Length - "。".Length);
            }
            if (translation.EndsWith("。」") && !original.EndsWith("。」"))
            {
                translation = translation.Substring(0, translation.Length - "。」".Length) + "」";
            }

            return translation;
        }

        public static string UnescapeTranslation(string original, string translation)
        {
            if (!original.Contains("\\r"))
            {
                translation = translation.Replace("\\r", "\r");
            }
            if (!original.Contains("\\n"))
            {
                translation = translation.Replace("\\n", "\n");
            }
            if (!original.Contains("\\t"))
            {
                translation = translation.Replace("\\t", "\t");
            }

            return translation;
        }
    }
}
