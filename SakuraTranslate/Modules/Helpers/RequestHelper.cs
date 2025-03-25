using SakuraTranslate.Models;
using System;
using System.Collections.Generic;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeRequestStr(List<PromptMessage> prompts, int maxTokens, double temperature, double topP, double frequencyPenalty = 0)
        {
            var sb = new StringBuilder();
            prompts.ForEach(p => { sb.Append($"{{\"role\":\"{JsonHelper.Escape(p.Role)}\",\"content\":\"{JsonHelper.Escape(p.Content)}\"}},"); });
            sb.Remove(sb.Length - 1, 1);
            var retStr =
                $"{{\"model\":\"sukinishiro\"," +
                $"\"messages\":[{sb}]," +
                $"\"temperature\":{temperature}," +
                $"\"top_p\":{topP}," +
                $"\"max_tokens\":{maxTokens}," +
                $"\"frequency_penalty\":{frequencyPenalty}," +
                $"\"seed\":-1," +
                $"\"do_sample\":true," +
                $"\"num_beams\":1," +
                $"\"repetition_penalty\":1.0," +
                $"\"stream\":false}}";
            if (_debug) { XuaLogger.AutoTranslator.Debug($"MakeRequestStr: retStr={{{retStr}}}"); }
            return retStr;
        }

        private int GetMaxTokens(string originalText)
        {
            if (_maxTokensMode == MaxTokensMode.Static)
            {
                return _staticMaxTokens;
            }
            else if (_maxTokensMode == MaxTokensMode.Dynamic)
            {
                return Math.Max((int)Math.Ceiling(originalText.Length * _dynamicMaxTokensMultiplier), MIN_MAX_TOKENS);
            }
            else
            {
                throw new Exception("Invalid max tokens mode.");
            }
        }

        private string MakeRequestJson(string line, double frequencyPenalty = 0)
        {
            switch (_modelType)
            {
                case TranslationModel.SakuraV0_9:
                    return MakeSakuraPromptV0_9(line, frequencyPenalty);
                case TranslationModel.SakuraV0_10:
                    return MakeSakuraPromptV0_10(line, frequencyPenalty);
                case TranslationModel.SakuraV1_0:
                    return MakeSakuraPromptV1_0(line, frequencyPenalty);
                case TranslationModel.GalTranslV2_6:
                    return MakeGalTranslPromptV2_6(line, frequencyPenalty);
                case TranslationModel.GalTranslV3:
                    return MakeGalTranslPromptV3(line, frequencyPenalty);
                default:
                    throw new Exception("Invalid model type.");
            }
        }
    }
}
