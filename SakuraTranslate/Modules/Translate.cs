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

            var jsonResponse = JSON.Parse(responseText);
            string translatedText;
            translatedText = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString().Trim('\"');

            translatedText = JsonHelper.Unescape(translatedText);
            translatedText = TranslationHelper.FixTranslationEnd(untranslatedText, translatedText);

            // 提交翻译文本
            context.Complete(translatedText);
        }
    }
}