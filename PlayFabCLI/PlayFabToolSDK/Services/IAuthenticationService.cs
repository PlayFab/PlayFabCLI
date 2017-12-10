using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awareness;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Utils;

namespace PlayFabToolSDK.Services
{
    /// <summary>
    /// Combined authentication service that may authenticate both the Tool and the Title
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate tool with your personal credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task AuthenticateTool(string username, string password);

        /// <summary>
        /// Authenticate title with a title reference (id and secret key)
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        Task<IDisposable> AuthenticateTitleDeveloper(TitleReference title);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private ILogger _logger;
        private IPlayFabEditorService _editorService;

        public AuthenticationService(IPlayFabEditorService editorService, ILogger logger)
        {
            _editorService = editorService;
            _logger = logger;
        }


        public async Task AuthenticateTool(string username, string password)
        {
            await _editorService.LoginAsync(new LoginRequest()
            {
                DeveloperToolProductName = "PlayFab Tools SDK"
            });
        }

        public Task<IDisposable> AuthenticateTitleDeveloper(TitleReference title)
        {
            return Task.FromResult(new PlayFabTitleAuth(title.TitleId,title.DeveloperKey) as IDisposable);
        }
    }
}
