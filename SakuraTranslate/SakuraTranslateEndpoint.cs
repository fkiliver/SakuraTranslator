using SakuraTranslate.Models;
using System.Collections.Generic;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;

namespace SakuraTranslate
{
    public partial class SakuraTranslateEndpoint : ITranslateEndpoint
    {
        // constants
        private const int DEGENERATION_MAX_RETRIES = 1;

        // params
        private string _endpoint;
        private string _modelName;
        private string _modelVersion;
        private MaxTokensMode _maxTokensMode;
        private int _staticMaxTokens;
        private double _dynamicMaxTokensMultiplier;
        private DictMode _dictMode;
        private Dictionary<string, List<string>> _dict;
        private bool _fixDegeneration;
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
