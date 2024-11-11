using SakuraTranslate.Models;
using System.Collections.Generic;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core.Utilities;

namespace SakuraTranslate.Helpers
{
    public static class PromptHelper
    {
        public static string SerializePromptMessages(List<PromptMessage> messages)
        {
            string result = "[";
            result += string.Join(",", messages.Select(x => $"{{\"role\":\"{x.Role}\"," +
                $"\"content\":\"{JsonHelper.Escape(x.Content)}\"}}").ToArray());
            result += "]";
            return result;
        }
    }
}
