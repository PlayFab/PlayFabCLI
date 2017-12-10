using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{
    public class UploadConfiguration
    {
        public TitleComposition Composition { get; }

        public UploadConfiguration(TitleComposition composition)
        {
            Composition = composition;
        }

        public bool UploadNews { get; set; }
    }
}