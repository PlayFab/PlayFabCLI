using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab.Json;
using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly ITitleRepositoryService _titleRepositoryService;
        private readonly IFileService _fileService;

        public MigrationService(ITitleRepositoryService titleRepositoryService, IFileService fileService)
        {
            _titleRepositoryService = titleRepositoryService;
            _fileService = fileService;
        }

        public async Task MigrateAsync(MigrationConfig config)
        {
            var titleComposition = await _titleRepositoryService.Fetch(config.Source, new FetchConfiguration()
            {
                TemplateComposition = new TitleComposition(),
                Stores = config.Stores ?? new List<string>()
            });

            await _fileService.Save(JsonConvert.SerializeObject(titleComposition, Formatting.Indented), "composition.old.json");

            await _titleRepositoryService.Upload(config.Target, new UploadConfiguration(titleComposition)
            {
                UploadNews = config.UploadNews
            });
        }
    }
}