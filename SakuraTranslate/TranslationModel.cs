using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private enum TranslationModel
        {
            SakuraV0_8,
            SakuraV0_9,
            SakuraV0_10,
            SakuraV1_0,
            Sakura32bV0_10,
            GalTranslV2_6
        }

        private static TranslationModel GetTranslationModel(string modelName, string modelVersion)
        {
            switch (modelName.ToLower())
            {
                case "sakura":
                    switch (modelVersion)
                    {
                        case "0.8": return TranslationModel.SakuraV0_8;
                        case "0.9": return TranslationModel.SakuraV0_9;
                        case "0.10": return TranslationModel.SakuraV0_10;
                        case "1.0": return TranslationModel.SakuraV1_0;
                        default: return TranslationModel.SakuraV1_0;
                    }
                case "sakura32b":
                    switch (modelVersion)
                    {
                        case "0.10": return TranslationModel.Sakura32bV0_10;
                        default: return TranslationModel.Sakura32bV0_10;
                    }
                case "galtransl":
                    switch (modelVersion)
                    {
                        case "2.6": return TranslationModel.GalTranslV2_6;
                        default: return TranslationModel.GalTranslV2_6;
                    }
                default:
                    return TranslationModel.SakuraV1_0;
            }
        }

        private static bool IsOpenAIEndpoint(TranslationModel model)
        {
            return !(model == TranslationModel.SakuraV0_8 || model == TranslationModel.SakuraV0_9);
        }
    }
}
