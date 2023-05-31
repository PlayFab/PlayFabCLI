using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFabCLI.Services;
using PlayFabCLI.Utils;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Services;

namespace PlayFabCLI.Commands
{
    /// <summary>
    /// This command executes migration based on specified config
    /// </summary>
    public class MigrateViaConfigAsyncConsoleCommand : ConsoleCommandAsync
    {
        private IFileService _fileService;
        private IMigrationService _migrationService;
        private IMigrationConfigService _migrationConfigService;

        public MigrateViaConfigAsyncConsoleCommand(IFileService fileService, IMigrationService migrationService, IMigrationConfigService migrationConfigService)
        {
            _fileService = fileService;
            _migrationService = migrationService;
            _migrationConfigService = migrationConfigService;
            IsCommand("migrate", "Runs migration based on a specified playfab migrate configuration.");
            HasAdditionalArguments(1, " <migration name>");
        }

        public override async Task RunAsync(string[] remainingArguments)
        {
            var migrationName = remainingArguments[0];
            var configuration = await _migrationConfigService.LoadConfiguration(migrationName.Trim());
            await _migrationService.MigrateAsync(configuration);
        }
    }
}
