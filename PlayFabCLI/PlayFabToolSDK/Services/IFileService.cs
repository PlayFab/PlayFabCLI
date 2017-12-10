using System.Threading.Tasks;

namespace PlayFabToolSDK.Services
{
    /// <summary>
    /// Simple service to operate on files based on key abstraction
    /// </summary>
    public interface IFileService
    {
        Task Save(string text, string key);
        Task<string> Read(string key);
    }
}