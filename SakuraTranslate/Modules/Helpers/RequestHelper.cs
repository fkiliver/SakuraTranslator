﻿using SakuraTranslate.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeRequestStr(List<PromptMessage> prompts, double frequencyPenalty = 0)
        {
            var sb = new StringBuilder();
            prompts.ForEach(p => { sb.Append($"{{\"role\":\"{JsonHelper.Escape(p.Role)}\",\"content\":\"{JsonHelper.Escape(p.Content)}\"}},"); });
            sb.Remove(sb.Length - 1, 1);
            int maxTokens;
            if (_maxTokensMode == MaxTokensMode.Static) { maxTokens = _staticMaxTokens; }
            else if (_maxTokensMode == MaxTokensMode.Dynamic) { maxTokens = (int)Math.Ceiling(prompts.Last().Content.Length * _dynamicMaxTokensMultiplier); }
            else { throw new Exception("Invalid max tokens mode."); }
            var retStr =
                $"{{\"model\":\"sukinishiro\"," +
                $"\"messages\":[{sb}]," +
                $"\"temperature\":{_temperature}," +
                $"\"top_p\":0.3" +
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

        private string MakeRequestJson(string line)
        {
            switch (_modelType)
            {
                case TranslationModel.SakuraV0_9:
                    return MakeSakuraPromptV0_9(line);
                case TranslationModel.SakuraV0_10:
                    return MakeSakuraPromptV0_10(line);
                case TranslationModel.SakuraV1_0:
                    return MakeSakuraPromptV1_0(line);
                case TranslationModel.GalTranslV2_6:
                    return MakeGalTranslPromptV2_6(line);
                default:
                    throw new Exception("Invalid model type.");
            }
        }
    }
}
