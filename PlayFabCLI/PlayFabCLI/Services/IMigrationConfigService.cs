using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFab.Json;
using PlayFabToolSDK.Models;

namespace PlayFabCLI.Services
{
    /// <summary>
    /// This service manages instances of MigrationConfig
    /// </summary>
    public interface IMigrationConfigService
    {
        /// <summary>
        /// Create instance of MigrationConfig
        /// </summary>
        /// <param name="source">Source title reference</param>
        /// <param name="target">Target title reference</param>
        /// <param name="stores">Optional stores to fetch and upload</param>
        /// <param name="copyNews">Optionally copy news</param>
        /// <returns></returns>
        Task<MigrationConfig> GenerateMigrationConfig(TitleReference source, TitleReference target, List<string> stores, bool copyNews = false);
        Task SaveMigrationConfig(string name, MigrationConfig config);
        Task<MigrationConfig> LoadConfiguration(string name);
    }
}
