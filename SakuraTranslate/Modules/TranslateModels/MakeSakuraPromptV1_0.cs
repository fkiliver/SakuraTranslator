using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakuraPromptV1_0(string line, double frequencyPenalty = 0)
        {
            var messages = new List<PromptMessage>
            {
                new PromptMessage
                {
                    Role = "system",
                    Content = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。"
                }
            };

            string dictStr = GetDictStr(line);

            if (_dictMode == DictMode.None || string.IsNullOrEmpty(dictStr))
            {
                // 如果术语表为空，直接构建翻译指令
                messages.Add(new PromptMessage
                {
                    Role = "user",
                    Content = $"将下面的日文文本翻译成中文：{line}"
                });
            }
            else
            {
                messages.Add(new PromptMessage
                {
                    Role = "user",
                    Content = $"根据以下术语表（可以为空）：\n{dictStr}\n" +
                          $"将下面的日文文本根据对应关系和备注翻译成中文：{line}"
                });
            }

            return MakeRequestStr(messages, 0.1, 0.3, frequencyPenalty);
        }
    }
}