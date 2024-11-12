using SakuraTranslate.Helpers;
using SimpleJSON;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.Common.Logging;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        public IEnumerator Translate(ITranslationContext context)
        {
            // 抽取未翻译文本
            var untranslatedText = context.UntranslatedText;

            if (_debug) { XuaLogger.AutoTranslator.Debug($"Translate: untranslatedText: {untranslatedText}"); }

            double frequencyPenalty = 0;
            int retryCount = 0;
            string translatedText = string.Empty;
            while (true)
            {
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

                if (_debug) { XuaLogger.AutoTranslator.Debug($"Translate: responseText: {responseText}"); }

                var jsonResponse = JSON.Parse(responseText);
                int completionTokens = int.Parse(jsonResponse["usage"]["completion_tokens"]);
                translatedText = jsonResponse["choices"]?[0]?["message"]?["content"]?.ToString().Trim('\"');

                if (translatedText == null)
                {
                    XuaLogger.AutoTranslator.Error($"Failed to parse response, jsonResponse: {jsonResponse}");
                }

                if (_fixDegeneration && completionTokens == GetMaxTokens(untranslatedText))
                {
                    if (retryCount < DEGENERATION_MAX_RETRIES)
                    {
                        XuaLogger.AutoTranslator.Warn($"Detected degeneration, retry: {retryCount + 1}/{DEGENERATION_MAX_RETRIES}, " +
                            $"completionTokens: {completionTokens}, maxTokens: {GetMaxTokens(untranslatedText)}, untranslatedText: {untranslatedText}");
                        frequencyPenalty += 0.2;
                        retryCount++;
                        continue;
                    }
                    else
                    {
                        XuaLogger.AutoTranslator.Warn($"Failed to fix degeneration after {retryCount} retries, untranslatedText: {untranslatedText}");
                        break;
                    }
                }
                break;
            }

            translatedText = JsonHelper.Unescape(translatedText);
            translatedText = TranslationHelper.FixTranslationEnd(untranslatedText, translatedText);

            if (_debug) { XuaLogger.AutoTranslator.Debug($"Translate: translatedText: {translatedText}"); }

            // 提交翻译文本
            context.Complete(translatedText);
        }
    }
}