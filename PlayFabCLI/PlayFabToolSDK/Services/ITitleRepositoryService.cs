using System.Threading.Tasks;
using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{
    /// <summary>
    /// Low-level service to execute title fetching and uploading
    /// </summary>
    public interface ITitleRepositoryService
    {
        Task<TitleComposition> Fetch(TitleReference source, FetchConfiguration configuration);
        Task Upload(TitleReference title, UploadConfiguration configuration);
    }
}