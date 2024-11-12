using SakuraTranslate.Models;
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

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        // params
        private string _endpoint;
        private string _modelName;
        private string _modelVersion;
        private MaxTokensMode _maxTokensMode;
        private int _staticMaxTokens;
        private double _dynamicMaxTokensMultiplier;
        private DictMode _dictMode;
        private Dictionary<string, List<string>> _dict;
        private int _maxConcurrency;
        private bool _debug;

        // local var
        private TranslationModel _modelType;
        private string _fullDictStr;

        public string Id => "SakuraTranslate";

        public string FriendlyName => "Sakura Translator";

        public int MaxConcurrency => _maxConcurrency;

        public int MaxTranslationsPerRequest => 1;
    }
}
