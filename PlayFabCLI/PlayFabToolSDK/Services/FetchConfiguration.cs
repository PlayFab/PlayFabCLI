using System.Collections.Generic;
using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{

    /// <summary>
    /// Defines how to fetch title
    /// </summary>
    public class FetchConfiguration
    {
        public List<string> Stores = new List<string>();
        public TitleComposition TemplateComposition = new TitleComposition();
    }
}