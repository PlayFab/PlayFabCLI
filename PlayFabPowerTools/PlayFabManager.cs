using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AdminModels;

namespace PlayFabPowerTools
{
    public class PlayFabManager
    {
        public static PlayFabToolsSettings ToolSettings = new PlayFabToolsSettings();
        private List<CloudScriptFile> _cloudScriptFiles = new List<CloudScriptFile>();

        public PlayFabManager()
        {
            Setup();
            if (ToolSettings.DeveloperSecretKey != null)
            {
                GetCloudScriptRevision();
            }
        }

        public void Setup()
        {
            var filePath = @"data\settings.json";
            if (!File.Exists(filePath))
            {
                var pfts = new PlayFabToolsSettings()
                {
                    CloudScriptRevision = 1,
                    CloudScriptVersion = 1,
                    DeveloperSecretKey = "[Add your developer secret key from playfab]",
                    DevelopmentMode = true,
                    ModulesPath = @"\modules",
                    RootPath = @"data\files"
                };

                var settings = JsonConvert.SerializeObject(pfts);
                using (StreamWriter sw = File.CreateText(filePath))
                {
                    sw.WriteLine(settings);
                }
                Console.WriteLine("Settings File was created at data/settings.json");
                CommandManager.Prompt();
                return;
            }
            else
            {
                using (StreamReader sr = File.OpenText(filePath))
                {
                    var json = sr.ReadToEnd();
                    var settings = JsonConvert.DeserializeObject<PlayFabToolsSettings>(json);
                    PlayFabSettings.DeveloperSecretKey = settings.DeveloperSecretKey;
                    PlayFabSettings.ProductionEnvironmentURL = settings.URI;
                    PlayFabSettings.DevelopmentEnvironmentURL = settings.URI;
                    PlayFabSettings.UseDevelopmentEnvironment = settings.DevelopmentMode;
                    ToolSettings = settings;
                }
            }
        }

        public void GetCloudScriptRevision(bool showOutput = true){
            var cloudRevsionRequest = new GetCloudScriptRevisionRequest();

            PlayFabAdminAPI.GetCloudScriptRevisionAsync(cloudRevsionRequest).ContinueWith((resultTask) =>
            {
                if (resultTask.IsCompleted)
                {
                    if (!resultTask.IsFaulted)
                    {
                        if (resultTask.Result.Result != null)
                        {
                            ToolSettings.CloudScriptRevision = resultTask.Result.Result.Revision;
                            ToolSettings.CloudScriptVersion = resultTask.Result.Result.Version;
                            _cloudScriptFiles = resultTask.Result.Result.Files;

                            var sdkVer = PlayFab.Internal.PlayFabVersion.getVersionString();
                            if (showOutput)
                            {
                                Console.WriteLine(string.Format("{2} - Cloud Script - Revision:{0} Version:{1}", ToolSettings.CloudScriptRevision, ToolSettings.CloudScriptVersion, sdkVer));
                                CommandManager.Prompt();
                            }
                        }
                        else
                        {
                            Console.WriteLine(resultTask.Result.Error.ErrorMessage);
                            CommandManager.Prompt();
                        }
                    }
                    else
                    {
                        try
                        {
                            Console.WriteLine(resultTask.Result.Error.ErrorMessage);
                            CommandManager.Prompt();
                        }
                        catch (Exception e)
                        {
                            if (e.InnerException != null)
                            {
                                Console.WriteLine(HandleErrorMessage(e.InnerException.Message));
                            }
                            else
                            {
                                Console.WriteLine(e.Message);
                            }
                            CommandManager.Prompt();
                        }
                            
                    }
                }
            });
        }

        public void Build()
        {
            var path = string.Format("{0}{1}", ToolSettings.RootPath, ToolSettings.ModulesPath);
            var cloudScriptBuilder = new StringBuilder();
            cloudScriptBuilder.Append("//This file is auto-generated, do not modify \n\r");
            //cloudScriptBuilder.Append("\n\r");

            if (Directory.Exists(path))
            {
                foreach (var filePath in Directory.GetFiles(path))
                {
                    var filename = Path.GetFileName(filePath);
                    var contents = string.Empty;
                    using (StreamReader sr = File.OpenText(filePath))
                    {
                        cloudScriptBuilder.Append(sr.ReadToEnd());
                    }
                    cloudScriptBuilder.Append("\r");
                }

                using (var sw = File.CreateText(string.Format(@"{0}/{1}", ToolSettings.RootPath, "CloudScript.js")))
                {
                    sw.WriteLine(cloudScriptBuilder.ToString());
                    sw.Flush();
                }

                Console.WriteLine("Build Complete!");
            }
            else
            {
                Console.WriteLine(string.Format("{0}{1}", "Directory not found:", path));
            }
        }

        public void Publish(bool newVersion = false)
        {
            List<CloudScriptFile> files = new List<CloudScriptFile>();
            var dirPath = ToolSettings.RootPath;
            if (Directory.Exists(dirPath))
            {
                foreach (var filePath in Directory.GetFiles(dirPath))
                {
                    var filename = Path.GetFileName(filePath);
                    string contents = string.Empty;
                    using (StreamReader sr = File.OpenText(filePath))
                    {
                        contents = sr.ReadToEnd();
                    }
                    CloudScriptFile csfile = new CloudScriptFile()
                    {
                        Filename = filename,
                        FileContents = contents
                    };

                    //Get previous Revision of the CloudScript File if it exists.
                    var revCsFile = _cloudScriptFiles.Find((csf) => { return csf.Filename == filename; });
                    bool fileDirty = false;
                    if (revCsFile != null)
                    {
                        // File exists on playfab
                        if (revCsFile.FileContents != contents)
                        {
                            fileDirty = true;
                        }
                    }
                    else
                    {
                        fileDirty = true;
                    }

                    if (fileDirty)
                    {
                        var csfiles = new List<CloudScriptFile>();
                        csfiles.Add(csfile);
                        int? version = ToolSettings.CloudScriptVersion;
                        if(newVersion){
                            version = ToolSettings.CloudScriptVersion + 1;
                        }

                        PlayFabAdminAPI.UpdateCloudScriptAsync(new UpdateCloudScriptRequest()
                        {
                            Files = csfiles,
                            Version = version
                        }).ContinueWith((result) =>
                        {
                            Console.WriteLine("test");
                            if (!result.IsFaulted && result.IsCompleted)
                            {
                                if (result.Result.Error != null)
                                {
                                    CommandManager.WriteAsync(string.Format("Error:{0}",result.Result.Error.ErrorMessage),ConsoleColor.Red);
                                }
                                else
                                {
                                    GetCloudScriptRevision(false);
                                    CommandManager.WriteAsync(string.Format("Publishing - {0}", filePath), ConsoleColor.Yellow);
                                }
                            }
                            else
                            {
                                CommandManager.WriteAsync("Error: Not Published", ConsoleColor.Red);
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine("Already Up to Date");
                    }
                }
            }
        }

        public void Pull()
        {
            Console.WriteLine("Use git repository to manage versions until playfab supports multiple files.");
            /*
            var dirPath = @"data\files";
            if( !Directory.Exists( dirPath ) )
            {
                Directory.CreateDirectory( dirPath );
            }

            if( _cloudScriptFiles != null )
            {
                foreach( var cloudScript in _cloudScriptFiles )
                {
                    using (var sw = File.CreateText(string.Format(@"{0}\{1}",dirPath,cloudScript.Filename)))
                    {
                        sw.WriteLine(cloudScript.FileContents);
                        sw.Flush();
                        Console.WriteLine( string.Format( "Recieved File: {0}", cloudScript.Filename ) );
                    }
                }
            }*/

        }

        public void GetVersion()
        {
            Console.WriteLine("CloudScript Version: {0}", ToolSettings.CloudScriptVersion);
        }

        private string HandleErrorMessage(string msg)
        {
            if (msg.ToLower().Contains("secretkey"))
            {
                return "Secret Key file is missing";
            }
            
            return "Oops Something went wrong";
        }

    }

    [Serializable]
    public class PlayFabToolsSettings
    {
        public string DeveloperSecretKey { get; set; }
        public string URI { get; set; }
        public int? CloudScriptRevision { get; set; }
        public int? CloudScriptVersion { get; set; }
        public bool DevelopmentMode { get; set; }
        public string ModulesPath { get; set; }
        public string RootPath { get; set; }
    }
}
