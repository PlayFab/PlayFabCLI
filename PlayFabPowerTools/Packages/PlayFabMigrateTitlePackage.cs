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
using PlayFabPowerTools.Utils;

namespace PlayFabPowerTools.Packages
{
    public class PlayFabMigrateTitlePackage : iStatePackage
    {
        private enum States
        {
            Idle,
            TitleData,
            TitleInternalData,
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

        // TODO: Store a talbe with enums for different Migrators
        // Then based on the enums added by the config we runn the processess.

        private bool _overwriteEmptyTables = true;

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

            Console.WriteLine("Migration Started");

            Task.Run(async () => {
                await MigrateTitleData(_commandArgs.FromTitleId, _commandArgs.ToTitleId);
                await MigrateInternalTitleData(_commandArgs.FromTitleId, _commandArgs.ToTitleId);
                await MigrateCurrencyAsync(_commandArgs.FromTitleId, _commandArgs.ToTitleId);
                await MigrateCloudScriptAsync(_commandArgs.FromTitleId, _commandArgs.ToTitleId);
                await MigrateCatolgItems(_commandArgs.FromTitleId, _commandArgs.ToTitleId);
                await MigrateStores(_commandArgs.FromTitleId, _commandArgs.ToTitleId, PlayFabService.Settings.StoreList);
                await MigrateDropTables(_commandArgs.FromTitleId, _commandArgs.ToTitleId);

                await Task.Delay(10);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nMigration Completed");
            });

            return false;
        }

        public bool Loop()
        {
            return false;
        }

        async public Task MigrateTitleData(string sourceTitleID, string targetTitleID)
        {
            var console = new ConsoleTaskWriter("# Migrating TitlData");

            // - FETCH
            console.LogProcess("Fetching data for comparison");

            PlayFab.ServerModels.GetTitleDataResult[] results = await Task.WhenAll(
                PlayFabService.GetTitleData(sourceTitleID),
                PlayFabService.GetTitleData(targetTitleID)
            );
            Dictionary<string, string> sourceTitleData = results[0].Data ?? new Dictionary<string, string>();
            Dictionary<string, string> targetTitleData = results[1].Data ?? new Dictionary<string, string>();

            if (sourceTitleData == null && targetTitleData == null) {
                console.LogError("Failed to retrieve title data, continuing...");
            }

            // - UPDATE

            Dictionary<string, string> itemsNeedingUpdate = FilterTitleDataToUpdate(sourceTitleData, targetTitleData);
            if (itemsNeedingUpdate.Count == 0) 
            {
                console.LogSuccess("Found no title data items to update.");
                return;
            }

            int totalItems = itemsNeedingUpdate.Count;
            int updatItemCounter = 0;
            while (itemsNeedingUpdate.Count > 0)
            {
                updatItemCounter++;
                console.LogProcess("Updating " + updatItemCounter + " out of " + totalItems + " items.");

                var kvp = itemsNeedingUpdate.FirstOrDefault();
                itemsNeedingUpdate.Remove(kvp.Key);
                bool success = await PlayFabService.UpdateTitleData(targetTitleID, kvp);
                if (!success) {
                    console.LogError("Save Title Data Failed, skipping");
                }
            }

            console.LogSuccess("TitleData Migration completed, Updated " + totalItems + " items");
        }

        async public Task MigrateInternalTitleData(string sourceTitleID, string targetTitleID) {
            var console = new ConsoleTaskWriter("# Migrating InternalTitleData");

            PlayFab.ServerModels.GetTitleDataResult[] results = await Task.WhenAll(
                PlayFabService.GetTitleInternalData(sourceTitleID),
                PlayFabService.GetTitleInternalData(targetTitleID)
            );
            Dictionary<string, string> sourceTitleData = results[0].Data ?? new Dictionary<string, string>();
            Dictionary<string, string> targetTitleData = results[1].Data ?? new Dictionary<string, string>();

            if (sourceTitleData == null && targetTitleData == null) {
                console.LogError("Failed to retrieve title data, continuing...");
            }

            Dictionary<string, string> itemsNeedingUpdate = FilterTitleDataToUpdate(sourceTitleData, targetTitleData);
            if (itemsNeedingUpdate.Count == 0) {
                console.LogSuccess("Found no internal title data to update.");
                return;
            }

            int totalItems = itemsNeedingUpdate.Count;
            int updatItemCounter = 0;

            // Update all entities
            while (itemsNeedingUpdate.Count > 0) {
                updatItemCounter++;
                console.LogProcess("Updating " + updatItemCounter + " out of " + totalItems + " items.");

                var kvp = itemsNeedingUpdate.FirstOrDefault();
                itemsNeedingUpdate.Remove(kvp.Key);
                bool success = await PlayFabService.UpdateTitleInternalData(targetTitleID, kvp);
                if (!success) {
                    console.LogError("Save Internal Title Data Failed, skipping");
                }
            }

            console.LogSuccess("InternalTitleData Migration completed, Updated " + totalItems + " items");
        }

        async public Task MigrateCurrencyAsync(string sourceTitleID, string targetTitleID, bool forceOverWrite = true)
        {
            var console = new ConsoleTaskWriter("# Migrating currency data");

            // - FETCH

            // Get data from both titles for comparison
            ListVirtualCurrencyTypesResult[] results = await Task.WhenAll(
                PlayFabService.GetCurrencyData(sourceTitleID),
                PlayFabService.GetCurrencyData(targetTitleID)
            );
            List<VirtualCurrencyData> sourceData = results[0].VirtualCurrencies ?? new List<VirtualCurrencyData>();
            List<VirtualCurrencyData> targetData = results[1].VirtualCurrencies ?? new List<VirtualCurrencyData>();

            // - DELETE

            // Find all items in the target that don't exist in the source
            List<VirtualCurrencyData> dataToBeDeleted = targetData.FindAll((PlayFab.AdminModels.VirtualCurrencyData targetCurrency) => {
                var delete = true;
                foreach (VirtualCurrencyData sourceCurrency in sourceData)
                {
                    if (sourceCurrency.CurrencyCode == targetCurrency.CurrencyCode)
                    {
                        delete = false;
                    }
                }
                return delete;
            });

            // Delete data
            if (dataToBeDeleted.Count > 0)
            {
                console.LogProcess("Deleting " + dataToBeDeleted.Count + " items");

                var deletedResult = await PlayFabService.DeleteCurrencyData(targetTitleID, dataToBeDeleted);
                if (deletedResult == null)
                {
                    console.LogError("Deleting currency data failed.");
                    return;
                }
            }

            // - UPDATE

            // Find all items in the source data that don't match target or doesn't exist
            List<VirtualCurrencyData> dataThatNeedsUpdate = sourceData.FindAll((PlayFab.AdminModels.VirtualCurrencyData sourceCurrency) => {
                var needsUpdate = true;
                foreach (VirtualCurrencyData targetCurrency in targetData)
                {
                    if (targetCurrency.CurrencyCode == sourceCurrency.CurrencyCode && targetCurrency.Equals(sourceCurrency))
                    {
                        needsUpdate = false;
                    }
                }
                return needsUpdate;
            });

            if (dataThatNeedsUpdate.Count == 0)
            {
                console.LogSuccess("Found no data to be updated");
                return;
            }

            // Update data
            if (dataThatNeedsUpdate.Count > 0 || forceOverWrite)
            {
                console.LogProcess("Updating " + dataThatNeedsUpdate.Count + " items");

                var updatedResult = await PlayFabService.UpdateCurrencyData(targetTitleID, dataThatNeedsUpdate);
                if (updatedResult == null)
                {
                    console.LogError("Updating currency data failed.");
                    return;
                }
            }

            console.LogSuccess("Completed Migration of currency data");
        }

        async public Task MigrateCloudScriptAsync(string sourceTitleID, string targetTitleID)
        {
            var console = new ConsoleTaskWriter("# Migrating CloudScript Data");
            var lists = await PlayFabService.GetCloudScript(sourceTitleID);
            if (lists == null)
            {
                console.LogError("Failed to fetch CloudScript Data.");
                return;
            }

            console.LogProcess("Migrating script");
            bool success = await PlayFabService.UpdateCloudScript(targetTitleID, lists);
            if (!success)
            {
                console.LogError("Save CloudScript Failed.");
                return;
            }

            console.LogSuccess("Completed cloud script migration.");
        }

        async public Task MigrateCatolgItems(string sourceTitleID, string targetTitleID)
        {
            //TODO: Make this support multiple catalogs
            var console = new ConsoleTaskWriter("# Migrating Catalog Items. Main Catalog only");

            console.LogProcess("Fetching data");
            var catalogItems = await PlayFabService.GetCatalogData(sourceTitleID);
            if (catalogItems == null)
            {
                console.LogError("Error Fetching CloudScript Data, skipping");
                return;
            }

            if(catalogItems.Count == 0)
            {
                console.LogSuccess("Found no catalog items to update");
                return;
            }

            console.LogProcess("Migrating");
            var success = await PlayFabService.UpdateCatalogData(targetTitleID, catalogItems[0].CatalogVersion, true, catalogItems);
            if (!success)
            {
                console.LogError("Save Catalog Failed, skipping.");
                return;
            }
            console.LogSuccess("Completed migration of catalog items");
        }

        async public Task MigrateStores(string sourceTitleID, string targetTitleID, List<String> storeList)
        {
            var console = new ConsoleTaskWriter("# Migrating stores from settings. StoreIDs=" + string.Join(",", storeList.ToArray()));

            if (storeList.Count == 0)
            {
                console.LogError("No stores have been set with SetStores. Skipping migration.");
                return;
            }

            // TODO: Remove any prevoius stores that has been deleted.

            var storeListBufffer = storeList.ToList<string>();
            while(storeListBufffer.Count > 0)
            {
                console.LogProcess("Migrating store");

                var currentStoreId = storeListBufffer[0];
                storeListBufffer.Remove(currentStoreId);
                var result = await PlayFabService.GetStoreData(sourceTitleID, currentStoreId);
                if (result == null)
                {
                    console.LogError("Error Fetching Store Data, trying next store.");
                    continue;
                }

                bool success = await PlayFabService.UpdateStoreData(targetTitleID, currentStoreId, result.CatalogVersion, result.MarketingData, result.Store);
                if (!success)
                {
                    console.LogError("Save Store Failed, trying next store.");
                    continue;
                }
                console.LogProcess("store migrated");
            }

            console.LogSuccess("Completed migration of stores.");
        }

        // Overwrites any table with the same id. 
        // NOTE: Could not find a way to delete the table that have been created
        // TODO: Make this support multiple catalogs
        async public Task MigrateDropTables(string sourceTitleID, string targetTitleID)
        {
            var console = new ConsoleTaskWriter("# Migrating Drop Table Data, Main Catalog only");

            console.LogProcess("Fetching data");
            
            Dictionary<string, RandomResultTableListing>[] results = await Task.WhenAll(
                PlayFabService.GetDropTableData(sourceTitleID),
                PlayFabService.GetDropTableData(targetTitleID)
            );

            Dictionary<string, RandomResultTableListing> sourceCatalog = results[0];
            Dictionary<string, RandomResultTableListing> targetCatalog = results[1];

            if(sourceCatalog == null)
            {
                console.LogError("Error Fetching CloudScript Data, skipping");
                return;
            }

            // Find out if the targetTitle has drop tables that the source dosent have
            // at the time of writing there where no PlayFAbAPI methods for deletion
            // The user has to manually go in in the dashboard and delet any unwanted droptables.
            string divergentMessage = "";
            List<string> deletionKeys = new List<string>();
            foreach (KeyValuePair<string, RandomResultTableListing> targetTableItem in targetCatalog)
            {
                if (!sourceCatalog.ContainsKey(targetTableItem.Key))
                {
                    deletionKeys.Add(targetTableItem.Key);
                }
            }
            if(deletionKeys.Count > 0)
            {
                divergentMessage = "The target title contains " + deletionKeys.Count + 
                                   " items that the source doesnt have. TableIds: " + string.Join(",", deletionKeys.ToArray()) + "." +
                                   " \n If you you want to delete these, you have to do it through the dashboard.";
                
            }

            if (sourceCatalog.Count <= 0)
            {
                console.LogProcess("Found no drop table to migrate, skipping. ");
                console.ReportError(divergentMessage);
                return;
            }

            List<RandomResultTable> dropTables = new List<RandomResultTable>();
            foreach (RandomResultTableListing item in sourceCatalog.Values)
            {
                RandomResultTable dropTable = new RandomResultTable();
                dropTable.TableId = item.TableId;
                dropTable.Nodes = item.Nodes;
                dropTables.Add(dropTable);
            }

            console.LogProcess("Migrating data");
            bool success = await PlayFabService.UpdateDropTableData(targetTitleID, dropTables);
            if(!success)
            {
                console.LogError("Error Fetching CloudScript Data, skipping");
                console.ReportError(divergentMessage);
                return;
            }

            console.LogSuccess("Completed Drop Table migration. ");
            console.ReportError(divergentMessage);
        }


        // - TODO
        //async public Task MigrateFiles()
        //{
        //    // FETCH
        //    var path = AppDomain.CurrentDomain.BaseDirectory + "temp";
        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }
        //    var contentList = await PlayFabService.GetContentList(_commandArgs.FromTitleId);
        //    if (contentList == null)
        //    {
        //        Console.WriteLine("Error Fetching CloudScript Data, skipping");
        //        return;
        //    }

        //    List<PlayFabService.DownloadedFile> downloadedFiles = new List<PlayFabService.DownloadedFile>();
        //    while(contentList.Count > 0)
        //    {
        //        var targetFile = downloadedFiles[0];
        //        downloadedFiles.Remove(targetFile);
        //        var file = await PlayFabService.DownloadFile(_commandArgs.FromTitleId, path, targetFile.Data);
        //        if(file == null)
        //        {
        //            Console.WriteLine("Error downloading file, skipping");
        //            return;
        //        }
        //        downloadedFiles.Add(file);
        //    }

        //    // UPLOAD
        //    while (downloadedFiles.Count > 0)
        //    {
        //        var targetFile = downloadedFiles[0];
        //        downloadedFiles.Remove(targetFile);
        //        var sucess = await PlayFabService.UploadFile(_commandArgs.ToTitleId, targetFile);
        //        if (!sucess)
        //        {
        //            Console.WriteLine("Error upload file, continuing with next file...");
        //        }

        //    }
        //    Directory.Delete(path, true);
        //}

        // Filter keys to be updated and deleted
        private Dictionary<string, string> FilterTitleDataToUpdate(Dictionary<string, string> sourceTitleData, Dictionary<string, string> targetTitleData) {
            Dictionary<string, string> entitiesToBeUpdated = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> sourceTableItem in sourceTitleData) {
                if (!targetTitleData.ContainsKey(sourceTableItem.Key) || targetTitleData.ContainsKey(sourceTableItem.Key) && targetTitleData[sourceTableItem.Key] != sourceTableItem.Value) {
                    entitiesToBeUpdated.Add(sourceTableItem.Key, sourceTableItem.Value);
                }
            }

            // Filter keys to be deleted
            // Add the keys that should be deleted to update table.
            // we need to pass them as null for playfab to delete them.
            List<string> deletionKeys = new List<string>();
            foreach (KeyValuePair<string, string> targetTableItem in targetTitleData) {
                if (!sourceTitleData.ContainsKey(targetTableItem.Key)) {
                    entitiesToBeUpdated.Add(targetTableItem.Key, null);
                }
            }

            return entitiesToBeUpdated;
        }

    }
}
