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
using PlayFabPowerTools.Utils;

namespace PlayFabPowerTools.Services
{
    public class PlayFabService
    {
        public static PlayFabServiceSettings Settings;

        private static string ErrorPrefix = "PlayFabErrorMessage: ";

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

        public static void Login(string user, string pass, Action<bool, string> callback)
        {
            var loginTask = PlayFabExtensions.Login(new LoginRequest()
            {
                Email = user,
                Password = pass,
                DeveloperToolProductName = "PlayFabPowerTools CLI",
                DeveloperToolProductVersion = "1.01"
            }).ContinueWith((result) =>
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

        public static void GetStudios(string token, Action<bool> callback)
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

        async public static Task<GetTitleDataResult> GetTitleData(string titleID)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleID);

            PlayFabSettings.TitleId = titleID;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabServerAPI.GetTitleDataAsync(new GetTitleDataRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;

            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null) {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            return result.Result;
        }

        async public static Task<bool> UpdateTitleData(string titleId, KeyValuePair<string, string> key)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabServerAPI.SetTitleDataAsync(new SetTitleDataRequest() {
                Key = key.Key,
                Value = key.Value
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null) {
                Console.WriteLine(ErrorPrefix + PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }

        async public static Task<GetTitleDataResult> GetTitleInternalData(string titleID)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleID);

            PlayFabSettings.TitleId = titleID;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabServerAPI.GetTitleInternalDataAsync(new GetTitleDataRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null) {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }

            return result.Result;
        }

        async public static Task<bool> UpdateTitleInternalData(string titleId, KeyValuePair<string, string> key)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabServerAPI.SetTitleInternalDataAsync(new SetTitleDataRequest() {
                Key = key.Key,
                Value = key.Value
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null) {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }

        async public static Task<ListVirtualCurrencyTypesResult> GetCurrencyData(string titleId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            // I want to return a task that holds the 
            var result = await PlayFabAdminAPI.ListVirtualCurrencyTypesAsync(new ListVirtualCurrencyTypesRequest());
            //.ContinueWith(
            //    (result) => {
            //        PlayFabSettings.TitleId = currentPlayFabTitleId;
            //        PlayFabSettings.DeveloperSecretKey = currentDevKey;
            //        if (result.Result.Error != null)
            //        {
            //            Console.WriteLine(PlayFabUtil.GetErrorReport(result.Result.Error));
            //            callback(false, null);
            //            return;
            //        }
            //        if (result.IsCompleted)
            //        {
            //            callback(true, result.Result.Result.VirtualCurrencies);
            //        }
            //    });

            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }

            Console.WriteLine("The data found in get currency " + result.Result.VirtualCurrencies.Count);

            return result.Result;
        }

        async public static Task<BlankResult> UpdateCurrencyData(string titleId, List<VirtualCurrencyData> currencyData)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabAdminAPI.AddVirtualCurrencyTypesAsync(new AddVirtualCurrencyTypesRequest()
            {
                VirtualCurrencies = currencyData
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            return result.Result;
        }

        async public static Task<BlankResult> DeleteCurrencyData(string titleId, List<VirtualCurrencyData> currencyData)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabAdminAPI.RemoveVirtualCurrencyTypesAsync(new RemoveVirtualCurrencyTypesRequest()
            {
                VirtualCurrencies = currencyData
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            return result.Result;
        }

        async public static Task<List<CloudScriptFile>> GetCloudScript(string titleId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.GetCloudScriptRevisionAsync(new GetCloudScriptRevisionRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            return result.Result.Files;
        }

        async public static Task<bool> UpdateCloudScript(string titleId, List<CloudScriptFile> cloudScriptData)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabAdminAPI.UpdateCloudScriptAsync(new UpdateCloudScriptRequest()
            {
                Files = cloudScriptData,
                Publish = true
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }

        async public static Task<List<ContentInfo>> GetContentList(string titleId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.GetContentListAsync(new GetContentListRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }

            return result.Result.Contents;
        }

        // Currently onle fetches the default catalog
        async public static Task<List<PlayFab.AdminModels.CatalogItem>> GetCatalogData(string titleId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.GetCatalogItemsAsync(new PlayFab.AdminModels.GetCatalogItemsRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            //var version = result.Result.Catalog[0].CatalogVersion;
            return result.Result.Catalog;
        }

        // NOTE: If there is an existing catalog with the version number in question, it will be deleted and replaced with only the items specified in this call.
        async public static Task<bool> UpdateCatalogData(string titleId, string catalogVersion, bool isDefault, List<PlayFab.AdminModels.CatalogItem> catalog)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            var result = await PlayFabAdminAPI.SetCatalogItemsAsync(new PlayFab.AdminModels.UpdateCatalogItemsRequest()
            {
                Catalog = catalog,
                CatalogVersion = catalogVersion,
                SetAsDefaultCatalog = isDefault
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }

        async public static Task<Dictionary<string, PlayFab.AdminModels.RandomResultTableListing>> GetDropTableData(string titleId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.GetRandomResultTablesAsync(new PlayFab.AdminModels.GetRandomResultTablesRequest());
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }

            return result.Result.Tables;
        }

        async public static Task<bool> UpdateDropTableData(string titleId, List<RandomResultTable> dropTables)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.UpdateRandomResultTablesAsync(new PlayFab.AdminModels.UpdateRandomResultTablesRequest()
            {
                Tables = dropTables
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }

        async public static Task<GetStoreItemsResult> GetStoreData(string titleId, string storeId)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabAdminAPI.GetStoreItemsAsync(new PlayFab.AdminModels.GetStoreItemsRequest()
            {
                StoreId = storeId
            });
              
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine("Get Store Error: " + PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }
            //var version = result.Result.CatalogVersion;
            //var marketingModel = result.Result.MarketingData;
            //var currentStoreId = result.Result.StoreId;

            return result.Result;
        }

        async public static Task<bool> UpdateStoreData(string titleId, string storeId, string catalogVersion, StoreMarketingModel marketingModel, List<PlayFab.AdminModels.StoreItem> store)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;
            //Console.WriteLine("Updating Store: " + storeId + " on title" + titleId);
            var result = await PlayFabAdminAPI.SetStoreItemsAsync(new PlayFab.AdminModels.UpdateStoreItemsRequest()
            {
                CatalogVersion = catalogVersion,
                StoreId = storeId,
                MarketingData = marketingModel,
                Store = store
            });
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (result.Error != null)
            {
                Console.WriteLine("Update Store Error: " + PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            return true;
        }


        public class DownloadedFile
        {
            public ContentInfo Data;
            public string FilePath;
        }
        async public static Task<DownloadedFile> DownloadFile(string titleId, string path, ContentInfo content)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var result = await PlayFabServerAPI.GetContentDownloadUrlAsync(new GetContentDownloadUrlRequest()
            {
                Key = content.Key,
                HttpMethod = "GET"
            });
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return null;
            }

            var folderPathArray = content.Key.Split('/');
            var fileName = folderPathArray.ToList().Last();

            var filePath = string.Format("{0}\\{1}", path, fileName);
            var success = await PlayFabExtensions.DownloadFile(result.Result.URL, filePath);
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;
            if (!success)
            {
                return null;
            }
            return new DownloadedFile() { Data = content, FilePath = filePath };
        }


        async public static Task<bool> UploadFile(string titleId, DownloadedFile fileInfo)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var key = fileInfo.FilePath.Split('/').ToList()[fileInfo.FilePath.Split('/').ToList().Count - 1];
            var type = MimeMapping.GetMimeMapping(key);
            var result = await PlayFabAdminAPI.GetContentUploadUrlAsync(
                new GetContentUploadUrlRequest()
                {
                    Key = fileInfo.Data.Key,
                    ContentType = type
                }
            );
            if (result.Error != null)
            {
                Console.WriteLine(PlayFabUtil.GetErrorReport(result.Error));
                return false;
            }
            PlayFabSettings.TitleId = currentPlayFabTitleId;
            PlayFabSettings.DeveloperSecretKey = currentDevKey;

            bool success = await PlayFabExtensions.UploadFile(result.Result.URL, fileInfo.FilePath);
            if (!success)
            {
                return false;
            }
            return true;
        }

        async public void DeleteFile(string titleId, DownloadedFile fileInfo)
        {
            var currentPlayFabTitleId = PlayFabSettings.TitleId;
            var currentDevKey = PlayFabSettings.DeveloperSecretKey;

            var title = FindTitle(titleId);
            PlayFabSettings.TitleId = titleId;
            PlayFabSettings.DeveloperSecretKey = title.SecretKey;

            var key = fileInfo.FilePath.Split('/').ToList()[fileInfo.FilePath.Split('/').ToList().Count - 1];
            var type = MimeMapping.GetMimeMapping(key);
            var file = new DeleteContentRequest();
            var result = await PlayFabAdminAPI.DeleteContentAsync(file);
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
                } catch
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
                System.IO.File.WriteAllText(path + file, settings);
            } catch
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
