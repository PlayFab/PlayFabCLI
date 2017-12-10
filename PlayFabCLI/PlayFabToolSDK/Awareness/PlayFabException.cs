using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.Internal;

namespace PlayFabToolSDK.Awareness
{

    /// <summary>
    /// Exception that wraps PlayFab error and provides simple interface to Message and Code
    /// </summary>
    public class PlayFabException : Exception
    {
        private PlayFabError _error;

        public PlayFabException(PlayFabError error)
        {
            _error = error;
        }

        public override string Message
        {
            get
            {
                return _error.GenerateErrorReport();
            }
        }
        public PlayFabErrorCode Code => _error.Error;
    }

    /// <summary>
    /// Ensures the PlayFab response has no errors. Throws PlayFabException otherwise
    /// </summary>
    public static class PlayFabResultExtensions
    {
        public static T SafeResult<T>(this PlayFabResult<T> result) where T : PlayFabResultCommon
        {
            if(result.Error != null) throw new PlayFabException(result.Error);
            return result.Result;
        }
    }
}
