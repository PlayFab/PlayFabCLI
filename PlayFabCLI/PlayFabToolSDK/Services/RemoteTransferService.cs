using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Awareness;
using PlayFab;

namespace PlayFabToolSDK.Services
{
    public class RemoteTransferService : IRemoteTransferService
    {

        private ILogger _logger;

        public RemoteTransferService(ILogger logger)
        {
            _logger = logger;
        }

        public Task UploadDataAsync(string uri, byte[] data)
        {
            using (WebClient client = new WebClient())
            {
                _logger.Verbose($"Uploading data to {uri}", this);
                return client.UploadDataTaskAsync(uri, "PUT", data);
            }
        }

        public Task<byte[]> DownloadDataAsync(string uri)
        {
            using (WebClient client = new WebClient())
            {
                _logger.Verbose($"Downloading data from {uri}",this);
                return client.DownloadDataTaskAsync(uri);
            }
        }

        public Task UploadFileAsync(string uri, string filename)
        {
            using (WebClient client = new WebClient())
            {
                _logger.Verbose($"Uploading file from {filename} to {uri}", this);
                //request.ContentType = "application/x-gzip";
                var realPath = Path.Combine("temp", filename);
                return client.UploadFileTaskAsync(uri, "PUT", realPath);
            }
        }

        public Task DownloadFileAsync(string uri, string filename)
        {
            using (WebClient client = new WebClient())
            {
                _logger.Verbose($"Downloading file from {uri} into {filename}",this);
                var realPath = Path.Combine("temp", filename);
                var realDirectoryPath = Path.Combine("temp", Path.GetDirectoryName(filename));
                Directory.CreateDirectory(realDirectoryPath);
                return client.DownloadFileTaskAsync(uri, realPath);
            }
        }


    }
}
