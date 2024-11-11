using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeSakura32bPromptV0_10(string line)
        {
            string messagesStr = string.Empty;
            var messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "system",
                        Content = "你是一个轻小说翻译模型，可以流畅通顺地使用给定的术语表以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，注意不要混淆使役态和被动态的主语和宾语，不要擅自添加原文中没有的代词，也不要擅自增加或减少换行。"
                    }
                };
            string dictStr;
            if (_useDict == false)
            {
                dictStr = string.Empty;
            }
            else if (_dictMode == "Full")
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
            messages.Add(new PromptMessage
            {
                Role = "user",
                Content = $"根据以下术语表（可以为空）：\n{dictStr}\n\n" +
                          $"将下面的日文文本根据上述术语表的对应关系和备注翻译成中文：{line}"
            });
            messagesStr = PromptHelper.SerializePromptMessages(messages);

            return $"{{" +
                   $"\"model\": \"sukinishiro\"," +
                   $"\"messages\": " +
                   messagesStr +
                   $"," +
                   $"\"temperature\": 0.1," +
                   $"\"top_p\": 0.3," +
                   $"\"max_tokens\": 512," +
                   $"\"frequency_penalty\": 0.2," +
                   $"\"do_sample\": true," +
                   $"\"num_beams\": 1," +
                   $"\"repetition_penalty\": 1.0" +
                   $"}}";
        }
    }
}