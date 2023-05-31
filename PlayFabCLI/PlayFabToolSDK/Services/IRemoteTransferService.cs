using System.Threading.Tasks;

namespace PlayFabToolSDK.Services
{

    /// <summary>
    /// Simple service to execute remote file and data management. File management is based on key abstraction
    /// </summary>
    public interface IRemoteTransferService
    {
        Task UploadDataAsync(string uri, byte[] data);
        Task<byte[]> DownloadDataAsync(string uri);
        Task UploadFileAsync(string uri, string filename);
        Task DownloadFileAsync(string uri, string filename);
    }
}