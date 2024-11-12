using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakuraPromptV0_10(string line)
        {
            string messagesStr = string.Empty;
            var messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "system",
                        Content = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，注意不要擅自添加原文中没有的代词，也不要擅自增加或减少换行。"
                    }
                };
            string dictStr;
            if (_dictMode == "Full")
            {
                dictStr = _fullDictStr;
            }
            else
            {
                var usedDict = _dict.Where(x => line.Contains(x.Key));
                if (usedDict.Count() > 0)
                {
                    var dictStrings = DictHelper.GetDictStringList(usedDict);
                    dictStr = string.Join("\n", dictStrings.ToArray());
                }
                else
                {
                    dictStr = string.Empty;
                }
            }
            if (string.IsNullOrEmpty(dictStr))
            {
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
                    Content = $"根据以下术语表：\n{dictStr}\n将下面的日文文本根据上述术语表的对应关系和注释翻译成中文：{line}"
                });
            }
            messagesStr = MakeRequestStr(messages, 0.1, 0.3, frequencyPenalty);
            return messagesStr;
        }
    }
}