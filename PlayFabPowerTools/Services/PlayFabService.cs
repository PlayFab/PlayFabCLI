using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AdminModels;
using PlayFab.EditorModels;
using PlayFab.ServerModels;
using PlayFabPowerTools.Packages;
using GetTitleDataRequest = PlayFab.ServerModels.GetTitleDataRequest;
using GetTitleDataResult = PlayFab.ServerModels.GetTitleDataResult;
using SetTitleDataRequest = PlayFab.ServerModels.SetTitleDataRequest;
using SetTitleDataResult = PlayFab.ServerModels.SetTitleDataResult;

namespace PlayFabPowerTools.Services
{
    public class PlayFabService
    {
        public static PlayFabServiceSettings Settings;

        public static void Init()
        {
            if (Settings == null)
            {
                Settings = new PlayFabServiceSettings();
            }

            if (Settings.StoreList == null)
            {
                Settings.StoreList = new List<string>();
            }
            if (Settings.Studios == null)
            {
                Settings.Studios = new List<Studio>();
            }
        }

        public static List<Studio> Studios
        {
            get { return Settings.Studios; }
            set { Settings.Studios = value; }
        }

        public static List<string> StoreList
        {
            get { return Settings.StoreList; }
            set { Settings.StoreList = value; }
        }

        public static string DeveloperPlayFabId
        {
            get { return Settings.DeveloperPlayFabId; }   
            set { Settings.DeveloperPlayFabId = value; }
        }

        public static string DeveloperClientToken
        {
            get { return Settings.DeveloperClientToken; }
            set { Settings.DeveloperClientToken = value; }
        }

        public static void Login(string user, string pass, string twofa, Action<bool,string> callback)
        {
            Task<PlayFabResult<LoginResult>> loginTask;

            if (String.IsNullOrEmpty(twofa))
            {
                loginTask = PlayFabExtensions.Login(new LoginRequest()
                {
                    Email = user,
                    Password = pass,
                    DeveloperToolProductName = "PlayFabPowerTools CLI",
                    DeveloperToolProductVersion = "1.01"
                });
            }
            else
            {
                loginTask = PlayFabExtensions.Login(new LoginRequest()
                {
                    Email = user,
                    Password = pass,
                    TwoFactorAuth = twofa,
                    DeveloperToolProductName = "PlayFabPowerTools CLI",
                    DeveloperToolProductVersion = "1.01"
                });
            } 
            loginTask.ContinueWith((result) =>
            {
                if (result.Result.Error != null)
                {
                    Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                    callback(false, null);
                    return;
                }

                if (result.IsCompleted)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Login Successful");
                    DeveloperClientToken = result.Result.Result.DeveloperClientToken;
                    callback(true, DeveloperClientToken);
                }
            });
        }

        public static void GetStudios(string token,Action<bool> callback)
        {
            var getStudioTask = PlayFabExtensions.GetStudios(new GetStudiosRequest()
            {
                DeveloperClientToken = token
            }).
                ContinueWith((result) =>
                {
                    if (result.Result.Error != null)
                    {
                        Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                        callback(false);
                        return;
                    }

                    if (result.IsCompleted)
                    {
                        PlayFabService.Studios = result.Result.Result.Studios.ToList();
                        callback(true);
                    }
                });
        }

        public static void GetTitleData(string titleID, Action<bool,GetTitleDataResult> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleID);

            PlayFabSettings.TitleId = titleID;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var task = PlayFabServerAPI.GetTitleDataAsync(new GetTitleDataRequest()).ContinueWith(
                (result) =>
                {
                    PlayFabSettings.TitleId = currentPlayFabTitleId;
                    PlayFabSettings.DeveloperSecretKey = currentDevKey;
                    if (result.Result.Error != null)
                    {
                        Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                        callback(false, null);
                        return;
                    }
                    if (result.IsCompleted)
                    {
                        callback(true, result.Result.Result);
                    }
                });
        }

        public static void UpdateTitleData(string titleId, KeyValuePair<string,string> key, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabServerAPI.SetTitleDataAsync(new SetTitleDataRequest()
            {
                Key = key.Key,
                Value = key.Value
            }).ContinueWith(
                (result) =>
                {
                    PlayFabSettings.TitleId = currentPlayFabTitleId;
                    PlayFabSettings.DeveloperSecretKey = currentDevKey;
                    if (result.Result.Error != null)
                    {
                        Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                        callback(false);
                        return;
                    }
                    if (result.IsCompleted)
                    {
                        callback(true);
                    }
                });
        }

        public static void GetTitleInternalData(string titleID, Action<bool, GetTitleDataResult> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleID);

            PlayFabSettings.TitleId = titleID;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var task = PlayFabServerAPI.GetTitleInternalDataAsync(new GetTitleDataRequest()).ContinueWith(
                (result) =>
                {
                    PlayFabSettings.TitleId = currentPlayFabTitleId;
                    PlayFabSettings.DeveloperSecretKey = currentDevKey;
                    if (result.Result.Error != null)
                    {
                        Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                        callback(false, null);
                        return;
                    }
                    if (result.IsCompleted)
                    {
                        callback(true, result.Result.Result);
                    }
                });
        }

        public static void UpdateTitleInternalData(string titleId, KeyValuePair<string, string> key, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabServerAPI.SetTitleInternalDataAsync(new SetTitleDataRequest()
            {
                Key = key.Key,
                Value = key.Value
            }).ContinueWith(
                (result) =>
                {
                    PlayFabSettings.TitleId = currentPlayFabTitleId;
                    PlayFabSettings.DeveloperSecretKey = currentDevKey;
                    if (result.Result.Error != null)
                    {
                        Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                        callback(false);
                        return;
                    }
                    if (result.IsCompleted)
                    {
                        callback(true);
                    }
                });
        }

        public static void GetCurrencyData(string titleId, Action<bool, List<VirtualCurrencyData>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.ListVirtualCurrencyTypesAsync(new ListVirtualCurrencyTypesRequest())
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true, result.Result.Result.VirtualCurrencies);
                        }
                    });

        }

        public static void UpdateCurrencyData(string titleId,List<VirtualCurrencyData> currencyData, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var task = PlayFabAdminAPI.AddVirtualCurrencyTypesAsync(new AddVirtualCurrencyTypesRequest()
            {
                VirtualCurrencies = currencyData
            })
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            //Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true);
                        }
                    });
        }

        public static void GetCloudScript(string titleId, Action<bool, List<CloudScriptFile>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.GetCloudScriptRevisionAsync(new GetCloudScriptRevisionRequest())
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true, result.Result.Result.Files);
                        }
                    });
        }

        public static void UpdateCloudScript(string titleId,List<CloudScriptFile> cloudScriptData, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var task = PlayFabAdminAPI.UpdateCloudScriptAsync(new UpdateCloudScriptRequest()
            {
                Files = cloudScriptData,
                Publish = true
            })
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            
                            //Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true);
                        }
                    });
        }

        public static void GetContentList(string titleId, Action<bool, List<ContentInfo>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.GetContentListAsync(new GetContentListRequest())
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {

                            callback(true, result.Result.Result.Contents);
                        }
                    });
        }

        public static void GetCatalogData(string titleId, Action<bool, string, List<PlayFab.AdminModels.CatalogItem>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.GetCatalogItemsAsync(new PlayFab.AdminModels.GetCatalogItemsRequest())
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false,null, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            var version = result.Result.Result.Catalog[0].CatalogVersion;
                            callback(true, version, result.Result.Result.Catalog);
                        }
                    });

        }

        public static void UpdateCatalogData(string titleId, string catalogVersion, bool isDefault, List<PlayFab.AdminModels.CatalogItem> catalog, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var task = PlayFabAdminAPI.SetCatalogItemsAsync(new PlayFab.AdminModels.UpdateCatalogItemsRequest()
                {
                    Catalog = catalog,
                    CatalogVersion = catalogVersion,
                    SetAsDefaultCatalog = isDefault 
                })
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true);
                        }
                    });
        }

        public static void GetDropTableData(string titleId, Action<bool, Dictionary<string, PlayFab.AdminModels.RandomResultTableListing>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.GetRandomResultTablesAsync(new PlayFab.AdminModels.GetRandomResultTablesRequest())
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        Task<PlayFabResult<PlayFab.AdminModels.GetRandomResultTablesResult>> taskC = result as Task<PlayFabResult<PlayFab.AdminModels.GetRandomResultTablesResult>>;
                        if (taskC.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(taskC.Result.Error));
                            callback(false, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true, taskC.Result.Result.Tables);
                        }
                    });

        }

        public static void UpdateDropTableData(string titleId, List<RandomResultTable> dropTables, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.UpdateRandomResultTablesAsync(new PlayFab.AdminModels.UpdateRandomResultTablesRequest()
            {
                Tables = dropTables
            }
            )
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true);
                        }
                    });
        }

        public static void GetStoreData(string titleId, string storeId,  Action<bool, string, string, StoreMarketingModel, List<PlayFab.AdminModels.StoreItem>> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var task = PlayFabAdminAPI.GetStoreItemsAsync(new PlayFab.AdminModels.GetStoreItemsRequest()
            {
                StoreId = storeId
            })
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine("Get Store Error: " + PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false, null, null, null, null);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            var version = result.Result.Result.CatalogVersion;
                            var marketingModel = result.Result.Result.MarketingData;
                            var currentStoreId = result.Result.Result.StoreId;
                            callback(true, version, currentStoreId, marketingModel, result.Result.Result.Store);
                        }
                    });

        }

        public static void UpdateStoreData(string titleId,string storeId, string catalogVersion, StoreMarketingModel marketingModel, List<PlayFab.AdminModels.StoreItem> store, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            Console.WriteLine("Updating Store: " + storeId + " on title" + titleId);
            var task = PlayFabAdminAPI.SetStoreItemsAsync(new PlayFab.AdminModels.UpdateStoreItemsRequest()
                {
                    CatalogVersion = catalogVersion,
                    StoreId = storeId,
                    MarketingData = marketingModel,
                    Store = store
                })
                .ContinueWith(
                    (result) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (result.Result.Error != null)
                        {
                            Console.WriteLine("Update Store Error: " + PlayFabUtil.GetErrorReport(result.Result.Error));
                            callback(false);
                            return;
                        }
                        if (result.IsCompleted)
                        {
                            callback(true);
                        }
                    });
        }

        public static void DownloadFile(string titleId, string path, ContentInfo content, Action<bool, string, ContentInfo> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            PlayFabServerAPI.GetContentDownloadUrlAsync(new GetContentDownloadUrlRequest()
            {
                Key = content.Key,
                HttpMethod = "GET"
            }).ContinueWith((result) =>
            {
                if (result.Result.Error != null)
                {
                    Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
                    callback(false, null, null);
                    return;
                }

                if (result.IsCompleted)
                {
                    var folderPathArray = content.Key.Split('/');
                    var fileName = folderPathArray.ToList().Last();

                    var filePath = string.Format("{0}\\{1}", path, fileName);
                    PlayFabExtensions.DownloadFile(result.Result.Result.URL, filePath, (success) =>
                    {
                        PlayFabSettings.TitleId = currentPlayFabTitleId;
                        PlayFabSettings.DeveloperSecretKey = currentDevKey;
                        if (success)
                        {
                            callback(true, filePath, content);
                            return;
                        }
                        callback(false, null, null);
                    });
                }
            });
        }


        public static void UploadFile(string titleId, CdnFileDataMigration.UploadFile fileInfo, Action<bool> callback)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var key = fileInfo.FilePath.Split('/').ToList()[fileInfo.FilePath.Split('/').ToList().Count-1];
            var type = MimeMapping.GetMimeMapping(key);
            PlayFabAdminAPI.GetContentUploadUrlAsync(new GetContentUploadUrlRequest()
            {
                Key = fileInfo.Data.Key,
                ContentType = type
            }).ContinueWith((result) =>
            {
                PlayFabSettings.TitleId = currentPlayFabTitleId;
                PlayFabSettings.DeveloperSecretKey = currentDevKey;
                PlayFabExtensions.UploadFile(result.Result.Result.URL, fileInfo.FilePath, callback);
            });
        }


        public static void Load()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "/data/";
            var file = "playfabsettings.json";
            if (Directory.Exists(path))
            {
                try
                {
                    var settings = File.ReadAllText(path + file);
                    var obj = JsonConvert.DeserializeObject<PlayFabServiceSettings>(settings);
                    if (obj != null)
                    {
                        Settings = obj;
                    }
                }
                catch
                {
                    Console.WriteLine("Could not load playfab settings file.");
                }
            }

        }

        public static void Save()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "/data/";
            var file = "playfabsettings.json";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                var settings = JsonConvert.SerializeObject(Settings);
                System.IO.File.WriteAllText(path+file, settings);
            }
            catch
            {
                Console.WriteLine("Could not save playfab settings file.");
            }
            
        }

        private static Title FindTitle(string titleId)
        {
            foreach (var studio in Studios)
            {
                var title = studio.Titles.ToList().Find(t => t.Id == titleId);
                if (title != null)
                {
                    return title;
                }
            }
            return null;
        }

    }

    [Serializable]
    public class PlayFabServiceSettings
    {
        public List<Studio> Studios;
        public List<string> StoreList;

        public string DeveloperPlayFabId;
        public string DeveloperClientToken;
    }
}
