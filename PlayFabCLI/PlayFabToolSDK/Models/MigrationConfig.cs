using System.Collections.Generic;

namespace PlayFabToolSDK.Models
{
    /// <summary>
    /// Migration config defines configuration for migration (c) Captain Obvious
    /// </summary>
    public struct MigrationConfig
    {
        public TitleReference Source { get; set; }
        public TitleReference Target { get; set; }
        public List<string> Stores { get; set; }
        public bool UploadNews { get; set; }
    }
}