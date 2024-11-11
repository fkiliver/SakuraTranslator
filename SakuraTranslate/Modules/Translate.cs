using SimpleJSON;
using System.Collections.Generic;
using System.Net;
using System;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using System.Collections;
using System.IO;
using System.Text;
using SakuraTranslate.Models;
using SakuraTranslate.Helpers;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
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
            if (TranslateModelHelper.IsOpenAIEndpoint(_modelType))
            {
                translatedText = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString().Trim('\"');
            }
            else
            {
                translatedText = jsonResponse["content"]?.ToString().Trim('\"');
            }

            translatedText = JsonHelper.Unescape(translatedText);
            translatedText = TranslationHelper.FixTranslationEnd(untranslatedText, translatedText);

            // 提交翻译文本
            context.Complete(translatedText);
        }
    }
}