using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFabCLI.Services;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Services;

namespace PlayFabCLI.Commands
{
    /// <summary>
    /// This command generates migration config either in prompt mode or based on cli arguments
    /// </summary>
    public class GenerateMigrationConfigAsyncConsoleCommand : ConsoleCommandAsync
    {
        private readonly IMigrationConfigService _migrationConfigService;
        private string _sourceTitleId;
        private string _sourceTitleKey;
        private string _targetTitleId;
        private string _targetTitleKey;
        private bool _copyNews;
        private List<string> _stores;
        private string _storesString;

        public GenerateMigrationConfigAsyncConsoleCommand(IMigrationConfigService migrationConfigService)
        {
            _migrationConfigService = migrationConfigService;

            IsCommand("generate-config", "Generates migration config file.");
            HasOption("o|original-title-id=", "Specify original title id", u => _sourceTitleId = u);
            HasOption("k|original-title-key=", "Specify original title key", u => _sourceTitleKey = u);
            HasOption("t|target-title-id=", "Specify target title id", u => _targetTitleId = u);
            HasOption("d|target-title-key=", "Specify target title key", u => _targetTitleKey= u);
            HasOption("n|upload-news", "Should migration also copy news (true/false). Warning: news will be re-added regardless of possible duplicates. This option only makes sense when migrating for the first time",
                t => _copyNews = t == null || Convert.ToBoolean(t));
            HasOption("e|upload-stores=", "Optional store ids to copy", u => _storesString = u);
            HasAdditionalArguments(1, " <migration name>");
        }

        public override async Task RunAsync(string[] remainingArguments)
        {
            var saveName = remainingArguments[0];

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

            await _migrationConfigService.SaveMigrationConfig(saveName, config);
        }

        private bool IsPromtMode()
        {
            return string.IsNullOrEmpty(_sourceTitleId) || string.IsNullOrEmpty(_targetTitleId) ||
                   string.IsNullOrEmpty(_sourceTitleKey) || string.IsNullOrEmpty(_targetTitleKey);
        }
    }
}