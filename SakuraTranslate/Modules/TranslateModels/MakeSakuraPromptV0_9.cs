using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakuraPromptV0_9(string line)
        {
            var messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "system",
                        Content = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。"
                    }
                };

            // 如果术语表为空，直接构建翻译指令
            messages.Add(new PromptMessage
            {
                Role = "user",
                Content = $"将下面的日文文本翻译成中文：{line}"
            });

            var messagesStr = MakeRequestStr(messages);
            //Console.WriteLine($"收到的line: {line}");
            //Console.WriteLine($"提交的prompt: {messagesStr}");

            return messagesStr;
        }
    }
}