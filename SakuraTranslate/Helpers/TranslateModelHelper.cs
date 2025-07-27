using SakuraTranslate.Models;

namespace SakuraTranslate.Helpers
{
    public static class TranslateModelHelper
    {
        public static TranslationModel GetTranslationModel(string modelName, string modelVersion)
        {
            switch (modelName.ToLower())
            {
                case "sakura":
                    switch (modelVersion)
                    {
                        case "0.9": return TranslationModel.SakuraV0_9;
                        case "0.10": return TranslationModel.SakuraV0_10;
                        case "1.0": return TranslationModel.SakuraV1_0;
                        default: return TranslationModel.SakuraV1_0;
                    }
                case "galtransl":
                    switch (modelVersion)
                    {
                        case "2.6": return TranslationModel.GalTranslV2_6;
                        case "3": return TranslationModel.GalTranslV3;
                        default: return TranslationModel.GalTranslV3;
                    }
                default:
                    return TranslationModel.SakuraV1_0;
            }
        }
    }
}
