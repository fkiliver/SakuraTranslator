using SakuraTranslate.Models;
using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakuraPromptV0_9(string originalText, double frequencyPenalty = 0)
        {
            return MakeRequestStr(new List<PromptMessage>
            {
                new PromptMessage
                {
                    Role = "system",
                    Content = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。"
                },
                new PromptMessage
                {
                    Role = "user",
                    Content = $"将下面的日文文本翻译成中文：{originalText}"
                }
            }, GetMaxTokens(originalText), 0.1, 0.3, frequencyPenalty);
        }
    }
}