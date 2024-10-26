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
    }
}
