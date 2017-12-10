using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayFabToolSDK.Models;

namespace PlayFabToolSDK.Services
{
    /// <summary>
    /// This is highlevel service to execute migration
    /// </summary>
    public interface IMigrationService
    {
        Task MigrateAsync(MigrationConfig config);
    }
}
