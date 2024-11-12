using SakuraTranslate.Models;
using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakuraPromptV0_10(string line, double frequencyPenalty = 0)
        {
            return MakeRequestStr(new List<PromptMessage>
            {
                new PromptMessage
                {
                    Role = "system",
                    Content = "你是一个轻小说翻译模型，可以流畅通顺地使用给定的术语表以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，注意不要混淆使役态和被动态的主语和宾语，不要擅自添加原文中没有的代词，也不要擅自增加或减少换行。"
                },
                new PromptMessage
                {
                    Role = "user",
                    Content = $"根据以下术语表：\n{GetDictStr(line) ?? string.Empty}\n\n将下面的日文文本根据上述术语表的对应关系和备注翻译成中文：{line}"
                }
            }, 0.1, 0.3, frequencyPenalty);
        }
    }
}