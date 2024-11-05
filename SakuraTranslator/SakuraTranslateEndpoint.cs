using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

[assembly: AssemblyVersion("0.3.6")]
[assembly: AssemblyFileVersion("0.3.6")]

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        public string Id => "SakuraTranslate";

        public string FriendlyName => "Sakura Translator";

        public int MaxConcurrency => _maxConcurrency;

        public int MaxTranslationsPerRequest => 1;

        // params
        private string _endpoint;
        private string _modelName;
        private string _modelVersion;
        private TranslationModel _modelType;
        private int _maxConcurrency;
        private bool _useDict;
        private string _dictMode;
        private Dictionary<string, List<string>> _dict;

        // local var
        private string _fullDictStr;

        public void Initialize(IInitializationContext context)
        {
            _endpoint = context.GetOrCreateSetting<string>("Sakura", "Endpoint", "http://127.0.0.1:8080/v1/chat/completions");
            _modelName = context.GetOrCreateSetting<string>("Sakura", "ModelName", "Sakura");
            _modelVersion = context.GetOrCreateSetting<string>("Sakura", "ModelVersion", "1.0");
            _modelType = GetTranslationModel(_modelName, _modelVersion);
            if (!int.TryParse(context.GetOrCreateSetting<string>("Sakura", "MaxConcurrency", "1"), out _maxConcurrency))
            {
                _maxConcurrency = 1;
            }
            if (_maxConcurrency > ServicePointManager.DefaultConnectionLimit)
            {
                ServicePointManager.DefaultConnectionLimit = _maxConcurrency;
            }
            if (!bool.TryParse(context.GetOrCreateSetting<string>("Sakura", "UseDict", "False"), out _useDict))
            {
                _useDict = false;
            }
            _dictMode = context.GetOrCreateSetting<string>("Sakura", "DictMode", "Partial");
            var dictStr = context.GetOrCreateSetting<string>("Sakura", "Dict", string.Empty);
            if (string.IsNullOrEmpty(dictStr))
            {
                _useDict = false;
                _fullDictStr = string.Empty;
            }
            else
            {
                try
                {
                    _dict = new Dictionary<string, List<string>>();
                    var dictJObj = JSON.Parse(dictStr);
                    foreach (var item in dictJObj)
                    {
                        try
                        {
                            var vArr = JSON.Parse(item.Value.ToString()).AsArray;
                            List<string> vList;
                            if (vArr.Count <= 0)
                            {
                                throw new Exception();
                            }
                            else if (vArr.Count == 1)
                            {
                                vList = new List<string> { vArr[0].ToString().Trim('\"'), string.Empty };
                            }
                            else
                            {
                                vList = new List<string> { vArr[0].ToString().Trim('\"'), vArr[1].ToString().Trim('\"') };
                            }
                            _dict.Add(item.Key.Trim('\"'), vList);
                        }
                        catch
                        {
                            _dict.Add(item.Key.Trim('\"'), new List<string> { item.Value.ToString().Trim('\"'), string.Empty });
                        }
                    }
                    if (_dict.Count == 0)
                    {
                        _useDict = false;
                        _fullDictStr = string.Empty;
                    }
                    else
                    {
                        var dictStrings = GetDictStringList(_dict);
                        _fullDictStr = string.Join("\n", dictStrings.ToArray());
                    }
                }
                catch
                {
                    _useDict = false;
                    _fullDictStr = string.Empty;
                }
            }
        }

        private List<string> GetDictStringList(IEnumerable<KeyValuePair<string, List<string>>> kvPairs)
        {
            List<string> dictList = new List<string>();
            foreach (var entry in kvPairs)
            {
                var src = entry.Key;
                var dst = entry.Value[0];
                var info = entry.Value[1];
                if (string.IsNullOrEmpty(info))
                {
                    dictList.Add($"{src}->{dst}");
                }
                else
                {
                    dictList.Add($"{src}->{dst} #{info}");
                }
            }

            return dictList;
        }

        public IEnumerator Translate(ITranslationContext context)
        {
            // 抽取未翻译文本
            var untranslatedText = context.UntranslatedText;

            //Console.WriteLine($"提交的翻译文本: {untranslatedText}");

            // 构建请求JSON
            string json = MakeRequestJson(untranslatedText);
            var dataBytes = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_endpoint);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = dataBytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            var asyncResult = request.BeginGetResponse(null, null);

            // 等待异步操作完成
            while (!asyncResult.IsCompleted)
            {
                yield return null;
            }

            string responseText;
            using (WebResponse response = request.EndGetResponse(asyncResult))
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
            }

            // 手动解析JSON响应
            //var startIndex = responseText.IndexOf("\"content\":") + 10;
            //var endIndex = responseText.IndexOf(",", startIndex);
            //var translatedText = responseText.Substring(startIndex, endIndex - startIndex);

            var jsonResponse = JSON.Parse(responseText);
            string translatedText;
            if (IsOpenAIEndpoint(_modelType))
            {
                translatedText = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString().Trim('\"');
            }
            else
            {
                translatedText = jsonResponse["content"]?.ToString().Trim('\"');
            }

            translatedText = SakuraUtil.FixTranslationEnd(untranslatedText, translatedText);

            // 提交翻译文本
            context.Complete(translatedText);
        }

        private string MakeRequestJson(string line)
        {
            string json;
            if (_modelType == TranslationModel.SakuraV0_8)
            {
                json = $"{{\"frequency_penalty\": 0.2, \"n_predict\": 1000, \"prompt\": \"<reserved_106>将下面的日文文本翻译成中文：{JsonHelper.Escape(line)}<reserved_107>\", \"repeat_penalty\": 1, \"temperature\": 0.1, \"top_k\": 40, \"top_p\": 0.3}}";
            }
            else if (_modelType == TranslationModel.SakuraV0_9)
            {
                json = $"{{\"prompt\":\"<|im_start|>system\\n你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，" +
                $"并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。<|im_end|>\\n<|im_start|>user\\n将下面的日文文本翻译成中文：" +
                $"{JsonHelper.Escape(line)}<|im_end|>\\n<|im_start|>assistant\\n\",\"n_predict\":512,\"temperature\":0.1,\"top_p\":0.3,\"repeat_penalty\":1," +
                $"\"frequency_penalty\":0.2,\"seed\":-1}}";
            }
            else if (_modelType == TranslationModel.SakuraV0_10)
            {
                json = MakeSakuraPromptV0_10(line);
            }
            else if (_modelType == TranslationModel.Sakura32bV0_10)
            {
                json = MakeSakura32bPromptV0_10(line);
            }
            else if (_modelType == TranslationModel.SakuraV1_0)
            {
                json = MakeSakuraPromptV1_0(line);
            }
            else if (_modelType == TranslationModel.GalTranslV2_6)
            {
                json = MakeGalTranslPromptV2_6(line);
            }
            else
            {
                json = MakeSakuraPromptV1_0(line);
            }

            return json;
        }

        private string MakeSakuraPromptV0_10(string line)
        {
            string messagesStr = string.Empty;
            if (_useDict)
            {
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
                        var dictStrings = GetDictStringList(usedDict);
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
                messagesStr = SerializePromptMessages(messages);
            }
            else
            {
                messagesStr = "[" +
                       $"{{" +
                       $"\"role\": \"system\"," +
                       $"\"content\": \"你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。\"" +
                       $"}}," +
                                $"{{" +
                                $"\"role\": \"user\"," +
                       $"\"content\": \"将下面的日文文本翻译成中文：{JsonHelper.Escape(line)}\"" +
                       $"}}" +
                       $"]";
            }
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
                    var dictStrings = GetDictStringList(usedDict);
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
            messagesStr = SerializePromptMessages(messages);

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

        private string MakeSakuraPromptV1_0(string line)
        {
            var messages = new List<PromptMessage>
                {
                    new PromptMessage
                    {
                        Role = "system",
                        Content = "你是一个轻小说翻译模型，可以流畅通顺地以日本轻小说的风格将日文翻译成简体中文，并联系上下文正确使用人称代词，不擅自添加原文中没有的代词。"
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
                    var dictStrings = GetDictStringList(usedDict);
                    dictStr = string.Join("\n", dictStrings.ToArray());
                }
                else
                {
                    dictStr = string.Empty;
                }
            }

            if (_useDict == false || string.IsNullOrEmpty(dictStr))
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

            var messagesStr = SerializePromptMessages(messages);

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
                    var dictStrings = GetDictStringList(usedDict);
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

            var messagesStr = SerializePromptMessages(messages);
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

        private string SerializePromptMessages(List<PromptMessage> messages)
        {
            string result = "[";
            result += string.Join(",", messages.Select(x => $"{{\"role\":\"{x.Role}\"," +
                $"\"content\":\"{JsonHelper.Escape(x.Content)}\"}}").ToArray());
            result += "]";
            return result;
        }

        class PromptMessage
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
