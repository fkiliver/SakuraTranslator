using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using System.Collections.Generic;

namespace SakuraTranslator
{
    public class SakuraTranslatorEndpoint : ITranslateEndpoint
    {
        public string Id => "SakuraTranslator";

        public string FriendlyName => "Sakura Translator";

        public int MaxConcurrency => 1;

        public int MaxTranslationsPerRequest => 1;

        public void Initialize(IInitializationContext context)
        {
            // 初始化代码（如果有的话）
        }

        public IEnumerator Translate(ITranslationContext context)
        {
            var untranslatedText = context.UntranslatedText;

            // 使用自定义方法转义文本
            var escapedUntranslatedText = EscapeJsonString(untranslatedText);

            // 构建请求JSON
            var json = $"{{\"frequency_penalty\": 0, \"n_predict\": 1000, \"prompt\": \"<reserved_106>将下面的日文文本翻译成中文：{escapedUntranslatedText}<reserved_107>\", \"repeat_penalty\": 1, \"temperature\": 0.1, \"top_k\": 40, \"top_p\": 0.3}}";
            var dataBytes = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8080/completion");
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = dataBytes.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(dataBytes, 0, dataBytes.Length);
            }

            var asyncResult = request.BeginGetResponse(null, null);

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
            var startIndex = responseText.IndexOf("\"content\":") + 10;
            var endIndex = responseText.IndexOf(",", startIndex);
            var translatedText = responseText.Substring(startIndex, endIndex - startIndex).Trim('\"', ' ', '\r', '\n');

            context.Complete(translatedText);
        }

        private string EscapeJsonString(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ')
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)c);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
