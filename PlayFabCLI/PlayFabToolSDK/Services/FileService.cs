using System.IO;
using System.Threading.Tasks;

namespace PlayFabToolSDK.Services
{

    public class FileService : IFileService
    {
        public async Task Save(string text, string key)
        {
            File.WriteAllText(key,text);
        }

        public async Task<string> Read(string key)
        {
            return File.ReadAllText(key);
        }
    }
}