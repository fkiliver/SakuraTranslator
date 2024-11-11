using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        private string MakeGalTranslPromptV2_6(string line)
        {
            var messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "system",
                        Content = "你是一个视觉小说翻译模型，可以通顺地使用给定的术语表以指定的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，注意不要混淆使役态和被动态的主语和宾语，不要擅自添加原文中没有的特殊符号，也不要擅自增加或减少换行。"
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
                Content = $"参考以下术语表（可为空，格式为src->dst #备注）：{(string.IsNullOrEmpty(dictStr) ? string.Empty : "\n" + dictStr)}\n\n" +
                            $"根据上述术语表的对应关系和备注，结合历史剧情和上下文，以流畅的风格将下面的文本从日文翻译成简体中文：{line}"
            });

            var messagesStr = PromptHelper.SerializePromptMessages(messages);
            //Console.WriteLine($"提交的prompt: {messagesStr}");

            return $"{{" +
                   $"\"model\": \"sukinishiro\"," +
                   $"\"messages\": " +
                   messagesStr +
                   $"," +
                   $"\"temperature\": 0.2," +
                   $"\"top_p\": 0.8," +
                   $"\"max_tokens\": 512," +
                   $"\"frequency_penalty\": 0.2," +
                   $"\"do_sample\": true," +
                   $"\"num_beams\": 1," +
                   $"\"repetition_penalty\": 1.0" +
                   $"}}";
        }
    }
}