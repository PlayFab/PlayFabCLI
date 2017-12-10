using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFab;

namespace PlayFabToolSDK.Utils
{
    /// <summary>
    /// Allows encapsulate PlayFab title authentication into Using statement
    /// </summary>
    public class PlayFabTitleAuth : IDisposable
    {
        private readonly string _titleIdCache;
        private readonly string _developerKeyCache;

        public PlayFabTitleAuth(string titleId, string developerKey)
        {
            _titleIdCache = PlayFabSettings.TitleId;
            _developerKeyCache = PlayFabSettings.DeveloperSecretKey;
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey= developerKey;
        }

        public void Dispose()
        {
            PlayFabSettings.TitleId = _titleIdCache;
            PlayFabSettings.DeveloperSecretKey = _developerKeyCache;
        }
    }
}
