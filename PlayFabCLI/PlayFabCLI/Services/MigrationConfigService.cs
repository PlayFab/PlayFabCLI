using System.Collections.Generic;
using System.Threading.Tasks;
using Awareness;
using Newtonsoft.Json;
using PlayFabCLI.Utils;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Services;

namespace PlayFabCLI.Services
{
    public class MigrationConfigService : IMigrationConfigService
    {
        private ILogger _logger;
        private readonly IFileService _fileService;

        public MigrationConfigService(ILogger logger, IFileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<MigrationConfig> GenerateMigrationConfig(TitleReference source, TitleReference target, List<string> stores, bool copyNews = false)
        {
            var config = new MigrationConfig()
            {
                Target = target,
                Source = source,
                Stores = stores,
                UploadNews = copyNews
            };
            return config;
        }

        public async Task SaveMigrationConfig(string name, MigrationConfig config)
        {
            var filename = name.ToPlayFabMigrationConfigFileName();
            await _fileService.Save(JsonConvert.SerializeObject(config, Formatting.Indented), filename);
            _logger.Log($"Saved {name} configuration as {filename}",this);
        }

        public async Task<MigrationConfig> LoadConfiguration(string name)
        {
            var file = await _fileService.Read(name.ToPlayFabMigrationConfigFileName());
            var migrationConfig = JsonConvert.DeserializeObject<MigrationConfig>(file);
            return migrationConfig;
        }
    }
}