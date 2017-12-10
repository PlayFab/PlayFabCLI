using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.Internal;
using PlayFab.Json;
using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{
    public interface IPlayFabEditorService
    {
        Task<PlayFabResult<LoginResult>> LoginAsync(LoginRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null);
        Task<PlayFabResult<LogoutResult>> LogoutAsync(LogoutRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null);
        Task<PlayFabResult<GetStudiosResult>> GetStudiosAsync(GetStudiosRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null);
        Task<PlayFabResult<CreateTitleResult>> CreateTitleAsync(CreateTitleRequest request, string authType = "X-Authorization", string authKey = null, Dictionary<string, string> headers = null);
    }

    public class PlayFabEditorService : IPlayFabEditorService
    {

        private static async Task<PlayFabResult<TResult>> ExecuteEditorOperationAsync<TRequest, TResult>(string url, TRequest request, string authType, string authKey, Dictionary<string,string> headers) where TRequest : PlayFabRequestCommon where TResult : PlayFabResultCommon
        {
            var result = new PlayFabResult<TResult>();
            
            //Save titleId and set to editor
            var titleId = PlayFabSettings.TitleId;
            PlayFabSettings.TitleId = "editor";
            object httpResult = await PlayFabHttp.DoPost(url, request, authType, authKey, headers);
            PlayFabSettings.TitleId = titleId;

            if (httpResult is PlayFabError)
            {
                PlayFabError error = (PlayFabError)httpResult;
                PlayFabSettings.GlobalErrorHandler?.Invoke(error);
                result.Error = error;
                return result;
            }
            result.Result = JsonWrapper.DeserializeObject<PlayFabJsonSuccess<TResult>>((string)httpResult).data;
            return result;
        }


        public Task<PlayFabResult<LoginResult>> LoginAsync(LoginRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null)
        {
            return ExecuteEditorOperationAsync<LoginRequest, LoginResult>("/DeveloperTools/User/Login",request,authType, authKey, headers);
        }

        public Task<PlayFabResult<LogoutResult>> LogoutAsync(LogoutRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null)
        {
            return ExecuteEditorOperationAsync<LogoutRequest, LogoutResult>("/DeveloperTools/User/Logout", request, authType, authKey, headers);
        }

        public Task<PlayFabResult<GetStudiosResult>> GetStudiosAsync(GetStudiosRequest request, string authType = null, string authKey = null, Dictionary<string, string> headers = null)
        {
            return ExecuteEditorOperationAsync<GetStudiosRequest, GetStudiosResult>("/DeveloperTools/User/GetStudios", request, authType, authKey, headers);
        }

        public Task<PlayFabResult<CreateTitleResult>> CreateTitleAsync(CreateTitleRequest request, string authType = "X-Authorization", string authKey = null, Dictionary<string, string> headers = null)
        {
            return ExecuteEditorOperationAsync<CreateTitleRequest, CreateTitleResult>("/DeveloperTools/User/CreateTitle", request, authType, authKey, headers);
        }

    }
}
