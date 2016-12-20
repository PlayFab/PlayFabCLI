using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PlayFab.AdminModels;
using PlayFabPowerTools.Services;

namespace PlayFabPowerTools.Packages
{
    public class PlayFabMigrateTitlePackage : iStatePackage
    {
        private enum States
        {
            Idle,
            TitleData,
            CloudScript,
            Files,
            Currency,
            Catalogs,
            DropTables,
            Stores,
            Complete
        }

        private States _state = States.Idle;

        private struct commandArgs
        {
            public string FromTitleId;
            public string ToTitleId;
        }

        private commandArgs _commandArgs;

        private CancellationTokenSource _cts = new CancellationTokenSource();
        private TitleDataMigration _titleData;
        private CurrencyDataMigration _currencyData;
        private CloudScriptDataMigration _cloudScriptData;
        private CdnFileDataMigration _cdnData;
        private CatalogDataMigration _catalogData;
        private StoreDataMigration _storeData;
        

        public void RegisterMainPackageStates(iStatePackage package)
        {
            List<MainPackageStates> states = new List<MainPackageStates>()
            {
                MainPackageStates.Migrate
            };
            PackageManagerService.RegisterMainPackageStates(states, package);
        }

        public bool SetState(string line)
        {
            //Parse Command Line args
            _commandArgs = new commandArgs();
            // migrate T381 T390
            var argsList = line.Split(' ');
            var lineSplit = argsList.ToList().FindAll(s => !s.ToLower().Contains("setstores") && !string.IsNullOrEmpty(s)).ToList();
            if (lineSplit.Count != 3)
            {
                Console.WriteLine("usage migrate [From TitleId] [To TitleId]");
                _state = States.Complete;
                return false;
            }
            _commandArgs.FromTitleId = lineSplit[1];
            _commandArgs.ToTitleId = lineSplit[2];

            //SetUp Data Objects
            _titleData = new TitleDataMigration();
            _currencyData = new CurrencyDataMigration();
            _cloudScriptData = new CloudScriptDataMigration();
            _cdnData = new CdnFileDataMigration();
            _catalogData = new CatalogDataMigration();
            _storeData = new StoreDataMigration
            {
                StoreList = PlayFabService.Settings.StoreList
            };

            SetNextState();
            return false;
        }

        public bool Loop()
        {
            _cts = new CancellationTokenSource();
            var worker = new Task(() =>
            {
                while (!_cts.Token.WaitHandle.WaitOne(2000))
                {
                    Execute();
                }
                //_cts.Token.ThrowIfCancellationRequested();
            },_cts.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning);

            //_worker.ContinueWith(x =>
            //{
            //    Trace.TraceError(x.Exception.InnerException.Message);
            //}, TaskContinuationOptions.OnlyOnFaulted);

            worker.Start();

            do
            {
                //block until _worker is completed.

            } while (!_cts.IsCancellationRequested);
            //Console.WriteLine("exit migrate");
            return false;
        }

        private void SetNextState()
        {
            switch (_state)
            {
                case States.Complete:
                case States.Idle:
                    _state = States.TitleData;
                    break;
                case States.TitleData:
                    _state = States.Currency;
                    break;
                case States.Currency:
                    _state = States.CloudScript;
                    break;
                case States.CloudScript:
                    _state = States.Files;
                    break;
                case States.Files:
                    _state = States.Catalogs;
                    break;
                case States.Catalogs:
                    _state = States.Stores;
                    break;
                default:
                    _state = States.Complete;
                    break;
            }
        }

        private void Execute()
        {
            switch (_state)
            {
                case States.TitleData:
                    #region Update Title Data Keys
                    if (!_titleData.FromProcessed)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Getting Title Data from: " + _commandArgs.FromTitleId);
                        PlayFabService.GetTitleData(_commandArgs.FromTitleId,(success,result) =>
                        {
                            if (!success || result.Data.Count == 0)
                            {
                                Console.WriteLine("No Title Data found, skipping");
                                SetNextState();
                            }
                            else
                            {
                                Console.WriteLine("Title Data Keys Found: " + result.Data.Count.ToString());
                                _titleData.Data = result.Data;
                                _titleData.FromProcessed = true;
                            }
                        });
                    }

                    if (!_titleData.ToProcessed && _titleData.FromProcessed)
                    {
                        if (_titleData.Data.Count == 0)
                        {
                            _titleData.ToProcessed = true;
                            SetNextState();
                            break;
                        }
                        var kvp = _titleData.Pop();
                        Console.WriteLine("Saving Title Data from: " + _commandArgs.FromTitleId + " To: " + _commandArgs.ToTitleId);
                        PlayFabService.UpdateTitleData(_commandArgs.ToTitleId, kvp , (success) =>
                        {
                            if (!success) { 
                                Console.WriteLine("Save Title Data Failed, skipping");
                                SetNextState();
                            }
                        });
                    }
                    #endregion
                    break;
                case States.Currency:
                    #region Update Currency Types
                    if (!_currencyData.FromProcessed)
                    {
                        Console.WriteLine("Getting Currency Types Data from: " + _commandArgs.FromTitleId);
                        PlayFabService.GetCurrencyData(_commandArgs.FromTitleId, (success, vcData) =>
                        {
                            if (!success || vcData.Count == 0)
                            {
                                Console.WriteLine("Error Fetching Currency Type Data, skipping");
                                SetNextState();
                                return;
                            }
                            _currencyData.Data = vcData;
                            _currencyData.FromProcessed = true;
                        });
                    }

                    if (!_currencyData.ToProcessed && _currencyData.FromProcessed)
                    {

                        if (_currencyData.Data == null)
                        {
                            _currencyData.ToProcessed = true;
                            SetNextState();
                            break;
                        }

                        PlayFabService.UpdateCurrencyData(_commandArgs.ToTitleId, _currencyData.Data,
                            (success) =>
                            {
                                //TODO: Add this back in once the API is fixed.
                                //if (!success)
                                //{
                                //    Console.WriteLine("Save Title Data Failed.");
                                //    _cts.Cancel();
                                //}
                                _currencyData.Data = null;
                            });
                    }
                    #endregion
                    break;
                case States.CloudScript:
                    #region Update CloudScript File
                    if (!_cloudScriptData.FromProcessed)
                    {
                        Console.WriteLine("Getting CloudScript Data from: " + _commandArgs.FromTitleId);
                        PlayFabService.GetCloudScript(_commandArgs.FromTitleId, (success, data) =>
                        {
                            if (!success || data.Count == 0)
                            {
                                Console.WriteLine("Error Fetching CloudScript Data, skipping.");
                                SetNextState();
                                return;
                            }
                            _cloudScriptData.Data = data;
                            _cloudScriptData.FromProcessed = true;
                        });
                    }

                    if (!_cloudScriptData.ToProcessed && _cloudScriptData.FromProcessed)
                    {

                        if (_cloudScriptData.Data == null)
                        {
                            _cloudScriptData.ToProcessed = true;
                            SetNextState();
                            break;
                        }

                        PlayFabService.UpdateCloudScript(_commandArgs.ToTitleId, _cloudScriptData.Data,
                            (success) =>
                            {
                                //if (!success)
                                //{
                                //    Console.WriteLine("Save CloudScript Failed.");
                                //    _cts.Cancel();
                                //}
                                _cloudScriptData.Data = null;
                            });
                    }
                    #endregion
                    break;
                case States.Files:
                    #region Update Content Files
                    //Start by creating a temp directory
                    var path = AppDomain.CurrentDomain.BaseDirectory + "temp";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    if (!_cdnData.FromProcessed)
                    {
                       PlayFabService.GetContentList(_commandArgs.FromTitleId, (success, data) =>
                       {
                           if (!success)
                           {
                               Console.WriteLine("Error Fetching CloudScript Data, skipping");
                               SetNextState();
                               return;
                           }
                           _cdnData.Data = data;
                           _cdnData.FromProcessed = true;
                       });
                    }

                    if (!_cdnData.ToProcessed && _cdnData.FromProcessed)
                    {
                        if (_cdnData.Data.Count == 0 && _cdnData.FileList.Count == 0)
                        {
                            _cdnData.ToProcessed = true;
                            _cdnData.Data = null;
                            _cdnData.FileList = null;
                            Directory.Delete(path, true);
                            SetNextState();
                            break;
                        }

                        if (_cdnData.Data.Count > 0)
                        {
                            var isDone = false;
                            
                            PlayFabService.DownloadFile(_commandArgs.FromTitleId, path, _cdnData.popData(), (success, filePath, data) =>
                            {
                                if (success)
                                {
                                    _cdnData.FileList.Add(new CdnFileDataMigration.UploadFile()
                                    {
                                        Data = data,
                                        FilePath = filePath
                                    });
                                }
                                isDone = true;
                            });
                            do
                            {
                                //block until done.
                            } while (!isDone);
                            break;
                        }

                        if (_cdnData.Data.Count == 0 && _cdnData.FileList.Count > 0)
                        {
                            var isUploadDone = false;
                            PlayFabService.UploadFile(_commandArgs.ToTitleId, _cdnData.popFileList(), (success) =>
                            {
                                isUploadDone = true;
                            });
                            do
                            {
                                //Block until this file upload is done.
                            } while (!isUploadDone);
                        }

                    }
                    #endregion
                    break;
                case States.Catalogs:
                    #region Update Catalog
                    if (_catalogData.FromProcessed && _catalogData.ToProcessed)
                    {
                        SetNextState();
                        break;
                    }

                    if (!_catalogData.FromProcessed)
                    {
                        //TODO: Make this support multiple catalogs
                        Console.WriteLine("Getting Catalog Data for Main Catalog only");
                        PlayFabService.GetCatalogData(_commandArgs.FromTitleId,
                            (success, catalogVersion, catalog) =>
                            {
                                if (!success)
                                {
                                    Console.WriteLine("Error Fetching CloudScript Data, skipping");
                                    SetNextState();
                                }
                                _catalogData.Data = catalog;
                                _catalogData.CatalogVersion = catalogVersion;
                                _catalogData.FromProcessed = true;
                            });
                    }

                    if (!_catalogData.ToProcessed && _catalogData.FromProcessed)
                    {
                        Console.WriteLine("Updating Catalog on Title: " + _commandArgs.ToTitleId);
                        PlayFabService.UpdateCatalogData(_commandArgs.ToTitleId, _catalogData.CatalogVersion, true, _catalogData.Data,
                            (success) =>
                            {
                                if (!success)
                                {
                                    Console.WriteLine("Save Catalog Failed, skipping.");
                                    SetNextState();
                                }
                                _catalogData.Data = null;
                                _catalogData.CatalogVersion = null;
                                _catalogData.ToProcessed = true;
                            });
                    }
                    #endregion
                    break;
                case States.Stores:
                    #region Update Stores
                    if (_storeData.IsComplete)
                    {
                        _storeData.Data = null;
                        _storeData.MarketingModel = null;
                        _storeData.StoreId = null;
                        _storeData.CatalogVersion = null;
                        SetNextState();
                        break;
                    }

                    if (!_storeData.FromProcessed)
                    {
                        if (_storeData.StoreList.Count == 0)
                        {
                            SetNextState();
                            break;
                        }
                        var currentStoreId = _storeData.popStoreId();
                        Console.WriteLine("Getting Store Data for StoreID: " + currentStoreId);
                        PlayFabService.GetStoreData(_commandArgs.FromTitleId, currentStoreId, 
                            (success, catalogVersion, storeId, marketingModel, store) =>
                            {
                                if (!success)
                                {
                                    Console.WriteLine("Error Fetching Store Data, Skipping.");
                                    SetNextState();
                                }
                                _storeData.FromProcessed = true;
                                _storeData.Data = store;
                                _storeData.CatalogVersion = catalogVersion;
                                _storeData.StoreId = storeId;
                                _storeData.MarketingModel = marketingModel;
                            });
                    }

                    if (!_storeData.ToProcessed && _storeData.FromProcessed)
                    {
                        var currentStoreId = _storeData.StoreId;
                        PlayFabService.UpdateStoreData(_commandArgs.ToTitleId, _storeData.StoreId, _storeData.CatalogVersion, _storeData.MarketingModel, _storeData.Data,
                            (success) =>
                            {
                                if (!success)
                                {
                                    Console.WriteLine("Save Store Failed,  Skipping.");
                                    SetNextState();
                                }
                                _storeData.Data = null;
                                _storeData.CatalogVersion = null;

                                if (_storeData.StoreList.Count == 0)
                                {
                                    _storeData.ToProcessed = true;
                                    _storeData.IsComplete = true;
                                }
                                else
                                {
                                    _storeData.ToProcessed = false;
                                    _storeData.FromProcessed = false;
                                }
                            });
                    }
                    #endregion
                    break;
                case States.Complete:
                    Console.WriteLine("Migration Complete.");
                    Console.ForegroundColor = ConsoleColor.White;
                    PackageManagerService.SetState(MainPackageStates.Idle);
                    _cts.Cancel();
                    break;
            }
        }

    }


    public class DataMigration
    {
        public bool ToProcessed;
        public bool FromProcessed;
    }

    public class StoreDataMigration : DataMigration
    {
        public List<StoreItem> Data;
        public string CatalogVersion;
        public string StoreId;
        public StoreMarketingModel MarketingModel;
        public List<string> StoreList = new List<string>();
        public bool IsComplete;

        public string popStoreId()
        {
            var storeid = StoreList[0];
            StoreList.Remove(storeid);
            return storeid;
        }

    }

    public class CatalogDataMigration : DataMigration
    {
        public List<CatalogItem> Data;
        public string CatalogVersion;
    }

    public class CdnFileDataMigration : DataMigration
    {
        public List<ContentInfo> Data = new List<ContentInfo>();
        public List<UploadFile> FileList = new List<UploadFile>();
        public class UploadFile
        {
            public ContentInfo Data;
            public string FilePath;
        }

        public ContentInfo popData()
        {
            var data = Data[0];
            Data.Remove(data);
            return data;
        }

        public UploadFile popFileList()
        {
            var data = FileList[0];
            FileList.Remove(data);
            return data;
        }

    }

    public class CloudScriptDataMigration : DataMigration
    {
        public List<CloudScriptFile> Data = new List<CloudScriptFile>();
    }

    public class CurrencyDataMigration : DataMigration
    {
        public List<PlayFab.AdminModels.VirtualCurrencyData> Data = new List<VirtualCurrencyData>(); 
    }

    public class TitleDataMigration : DataMigration
    {
        public Dictionary<string,string> Data = new Dictionary<string, string>();

        public KeyValuePair<string, string> Pop()
        {
            Dictionary<string, string> _tempDictionary = Data;
            foreach (var kvp in _tempDictionary)
            {
                Data.Remove(kvp.Key);
                return kvp;
            }

            return new KeyValuePair<string, string>();
        }
    }

}
