using SimpleJSON;
using System.Collections.Generic;
using System.Net;
using System;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using SakuraTranslate.Helpers;
using SakuraTranslate.Models;
using XUnity.Common.Logging;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        public void Initialize(IInitializationContext context)
        {
            _endpoint = context.GetOrCreateSetting<string>("Sakura", "Endpoint", "http://127.0.0.1:8080/v1/chat/completions");
            _modelName = context.GetOrCreateSetting<string>("Sakura", "ModelName", "Sakura");
            _modelVersion = context.GetOrCreateSetting<string>("Sakura", "ModelVersion", "1.0");
            _modelType = TranslateModelHelper.GetTranslationModel(_modelName, _modelVersion);
            #region maxTokens
            try
            {
                _maxTokensMode = (MaxTokensMode)Enum.Parse(typeof(MaxTokensMode), context.GetOrCreateSetting<string>("Sakura", "MaxTokensMode", "Static"), true);
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Warn(ex, $"Failed to parse max tokens mode: {context.GetOrCreateSetting<string>("Sakura", "MaxTokensMode", "Static")}, falling back to Static");
                _maxTokensMode = MaxTokensMode.Static;
            }
            if (!int.TryParse(context.GetOrCreateSetting<string>("Sakura", "StaticMaxTokens", "512"), out _staticMaxTokens) || _staticMaxTokens <= 0) { _staticMaxTokens = 512; }
            if (!double.TryParse(context.GetOrCreateSetting<string>("Sakura", "DynamicMaxTokensMultiplier", "1.5"), out _dynamicMaxTokensMultiplier) || _dynamicMaxTokensMultiplier <= 0) { _dynamicMaxTokensMultiplier = 1.0; }
            #endregion
            // init dict
            #region init dict
            try
            {
                _dictMode = (DictMode)Enum.Parse(typeof(DictMode), context.GetOrCreateSetting<string>("Sakura", "DictMode", "None"), true);
            }
            catch (Exception ex)
            {
                XuaLogger.AutoTranslator.Warn(ex, $"Failed to parse dict mode: {context.GetOrCreateSetting<string>("Sakura", "DictMode", "None")}, falling back to None");
                _dictMode = DictMode.None;
            }
            var dictStr = context.GetOrCreateSetting<string>("Sakura", "Dict", string.Empty);
            if (string.IsNullOrEmpty(dictStr))
            {
                XuaLogger.AutoTranslator.Warn("Dict is empty, setting DictMode to None");
                _dictMode = DictMode.None;
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
                                vList = new List<string> { JsonHelper.Unescape(vArr[0].ToString().Trim('\"')), string.Empty };
                            }
                            else
                            {
                                vList = new List<string> { JsonHelper.Unescape(vArr[0].ToString().Trim('\"')),
                                    JsonHelper.Unescape(vArr[1].ToString().Trim('\"')) };
                            }
                            _dict.Add(JsonHelper.Unescape(item.Key.Trim('\"')), vList);
                        }
                        catch
                        {
                            _dict.Add(JsonHelper.Unescape(item.Key.Trim('\"')),
                                new List<string> { JsonHelper.Unescape(item.Value.ToString().Trim('\"')), string.Empty });
                        }
                    }
                    if (_dict.Count == 0)
                    {
                        _dictMode = DictMode.None;
                        _fullDictStr = string.Empty;
                    }
                    else
                    {
                        _fullDictStr = GetDictStr(_dict);
                    }
                }
                catch (Exception ex)
                {
                    XuaLogger.AutoTranslator.Warn(ex, $"Failed to parse dict string: {dictStr}");
                    _dictMode = DictMode.None;
                    _fullDictStr = string.Empty;
                }
            }
            #endregion
            if (!int.TryParse(context.GetOrCreateSetting<string>("Sakura", "MaxConcurrency", "1"), out _maxConcurrency))
            {
                _maxConcurrency = 1;
            }
            if (_maxConcurrency > ServicePointManager.DefaultConnectionLimit)
            {
                XuaLogger.AutoTranslator.Info($"Setting ServicePointManager.DefaultConnectionLimit to {_maxConcurrency}");
                ServicePointManager.DefaultConnectionLimit = _maxConcurrency;
            }
            if (!bool.TryParse(context.GetOrCreateSetting<string>("Sakura", "Debug", "False"), out _debug)) { _debug = false; }
        }
    }
}