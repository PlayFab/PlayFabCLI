using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayFabCLI.Utils
{
    /// <summary>
    /// String extensions to quickly convert them to file/conventional names
    /// </summary>
    public static class Conventions
    {
        public static string ToPlayFabMigrationConfigFileName(this string key)
        {
            return key + ".playfab-migration.json";
        }
    }
}
