using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using PlayFabCLI.Services;
using PlayFabCLI.Utils;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Services;

namespace PlayFabCLI.Commands
{
    /// <summary>
    /// This command migrates title to title, optionally allows to save
    /// </summary>
    public class MigrateViaArgumentsCommand : ConsoleCommandAsync
    {
        private readonly IMigrationService _migrationService;
        private readonly IMigrationConfigService _migrationConfigService;
        private string _sourceTitleId;
        private string _sourceTitleKey;
        private string _targetTitleId;
        private string _targetTitleKey;
        private string _saveName;
        private bool _copyNews;
        private List<string> _stores;
        private string _storesString;

        public MigrateViaArgumentsCommand(IMigrationService migrationService, IMigrationConfigService migrationConfigService)
        {
            _migrationService = migrationService;
            _migrationConfigService = migrationConfigService;

            IsCommand("migrate-title", "Runs migration based on given title information.");
            HasRequiredOption("o|original-title-id=", "Specify original title id", u => _sourceTitleId = u);
            HasRequiredOption("k|original-title-key=", "Specify original title key", u => _sourceTitleKey = u);
            HasRequiredOption("t|target-title-id=", "Specify target title id", u => _targetTitleId = u);
            HasRequiredOption("d|target-title-key=", "Specify target title key", u => _targetTitleKey= u);
            HasOption("s|save=", "Specify migration config name to save", u => _saveName = u);
            HasOption("n|upload-news:", "Should migration also copy news (true/false). Warning: news will be re-added regardless of possible duplicates. This option only makes sense when migrating for the first time", 
                t => _copyNews = t == null || Convert.ToBoolean(t));
            HasOption("e|upload-stores=", "Optional store ids to copy", u => _storesString = u);
        }

        public override async Task RunAsync(string[] remainingArguments)
        {
            if (IsPromtMode())
            {
                while (string.IsNullOrEmpty(_sourceTitleId))
                {
                    Console.Write("\nOriginal (source) title id: ");
                    _sourceTitleId = Console.ReadLine().Trim();
                }

                while (string.IsNullOrEmpty(_sourceTitleKey))
                {
                    Console.Write("\nOriginal (source) title key: ");
                    _sourceTitleKey = Console.ReadLine().Trim();
                }

                while (string.IsNullOrEmpty(_targetTitleId))
                {
                    Console.Write("\nTarget title id: ");
                    _targetTitleId = Console.ReadLine().Trim();
                }

                while (string.IsNullOrEmpty(_targetTitleKey))
                {
                    Console.Write("\nTarget title key: ");
                    _targetTitleKey = Console.ReadLine().Trim();
                }

                Console.Write("\nStores to migrate (separated with comma): ");
                _storesString = Console.ReadLine().Trim();

                Console.Write("\nDo you want to reupload title news (y/N)?: ");
                _copyNews = Console.ReadLine().Trim() == "y";
            }

            if (!string.IsNullOrEmpty(_storesString))
            {
                _stores = _storesString.Split(',').ToList();
            }

            var config = await _migrationConfigService.GenerateMigrationConfig( 
                new TitleReference()
                {
                    TitleId = _sourceTitleId,
                    DeveloperKey = _sourceTitleKey
                }, new TitleReference()
                {
                    TitleId = _targetTitleId,
                    DeveloperKey = _targetTitleKey
                }, _stores, _copyNews);

            if (!string.IsNullOrEmpty(_saveName))
            {
                await _migrationConfigService.SaveMigrationConfig(_saveName, config);
            }
            await _migrationService.MigrateAsync(config);
        }

        private bool IsPromtMode()
        {
            return string.IsNullOrEmpty(_sourceTitleId) || string.IsNullOrEmpty(_targetTitleId) ||
                   string.IsNullOrEmpty(_sourceTitleKey) || string.IsNullOrEmpty(_targetTitleKey);
        }
    }
}