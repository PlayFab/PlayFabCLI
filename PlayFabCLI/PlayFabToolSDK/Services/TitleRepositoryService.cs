using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using Awareness;
using PlayFab;
using PlayFab.AdminModels;
using PlayFab.ServerModels;
using PlayFabToolSDK.Awareness;
using PlayFabToolSDK.Models;
using PlayFabToolSDK.Utils;
using CatalogItem = PlayFab.AdminModels.CatalogItem;
using GetCatalogItemsRequest = PlayFab.AdminModels.GetCatalogItemsRequest;
using GetRandomResultTablesRequest = PlayFab.AdminModels.GetRandomResultTablesRequest;
using GetTitleDataRequest = PlayFab.AdminModels.GetTitleDataRequest;
using PlayFabErrorCode = PlayFab.PlayFabErrorCode;
using SetTitleDataRequest = PlayFab.AdminModels.SetTitleDataRequest;

namespace PlayFabToolSDK.Services
{
    public class TitleRepositoryService : ITitleRepositoryService
    {

        private readonly IAuthenticationService _authService;
        private readonly ILogger _logger;
        private readonly IRemoteTransferService _remoteTransferService;

        public TitleRepositoryService(IAuthenticationService authService, IRemoteTransferService remoteTransferService, ILogger logger)
        {
            _authService = authService;
            _remoteTransferService = remoteTransferService;
            _logger = logger;
        }

        public async Task<TitleComposition> Fetch(TitleReference title, FetchConfiguration configuration)
        {
            using (_authService.AuthenticateTitleDeveloper(title))
            {
                _logger.Log("Fetching policies...",this);
                await FetchPoliciesInto(configuration.TemplateComposition);

                _logger.Log("Fetching catalog data...", this);
                await FetchCatalogDataInto(configuration.TemplateComposition);

                _logger.Log("Fetching virtual currency data...", this);
                await FetchVirtualCurrencyDataInto(configuration.TemplateComposition);

                _logger.Log("Fetching drop tables...", this);
                await FetchDropTableDataInto(configuration.TemplateComposition);

                if (configuration.Stores.Any())
                {
                    _logger.Log("Fetching stores...", this);
                    foreach (var store in configuration.Stores)
                    {
                        await FetchStoreDataInto(configuration.TemplateComposition, store);
                    }
                }

                _logger.Log("Fetching statistics definitions...", this);
                await FetchStatisticsDefinitionsInto(configuration.TemplateComposition);

                _logger.Log("Fetching Cloud Script...", this);
                await FetchCloudScriptInto(configuration.TemplateComposition);

                _logger.Log("Fetching regular title data...", this);
                await FetchTitleRegularDataInto(configuration.TemplateComposition);

                _logger.Log("Fetching internal title data...", this);
                await FetchTitleInternalDataInto(configuration.TemplateComposition);

                _logger.Log("Fetching content data...", this);
                await FetchContentListInto(configuration.TemplateComposition);
            }
            return configuration.TemplateComposition;
        }

        public async Task Upload(TitleReference title, UploadConfiguration configuration)
        {

            using (_authService.AuthenticateTitleDeveloper(title))
            {
                // Order matters: VC->Catalogs->DropTables->Stores->Reuploads
                _logger.Log("Uploading virtual currencies...", this);
                await UploadVirtualCurrencyDataFrom(configuration.Composition);

                _logger.Log("Uploading catalogs...", this);
                await UploadCatalogDataFrom(configuration.Composition);

                _logger.Log("Uploading drop tables...", this);
                await UploadDropTableDataFrom(configuration.Composition);

                _logger.Log("Uploading stores...", this);
                await UploadStoresFrom(configuration.Composition);

                _logger.Log("Uploading catalogs (patching)...", this);
                await PatchCatalogDataFrom(configuration.Composition);

                _logger.Log("Uploading policies...", this);
                await UploadPoliciesFrom(configuration.Composition);

                _logger.Log("Uploading statistics definitions...", this);
                await UploadStatisticsDefinitionsFrom(configuration.Composition);

                _logger.Log("Uploading Cloud Script...", this);
                await UploadCloudScriptFrom(configuration.Composition);

                _logger.Log("Uploading regular title data...", this);
                await UploadTitleRegularDataFrom(configuration.Composition);

                _logger.Log("Uploading internal title data...", this);
                await UploadTitleInternalDataFrom(configuration.Composition);

                _logger.Log("Uploading content data...", this);
                await UploadContentListFrom(configuration.Composition);
                if (configuration.UploadNews)
                {
                    _logger.Log("Uploading news data...", this);
                    await UploadTitleNewsFrom(configuration.Composition);
                }
                // Do not forget stores
            }
        }
 
        public async Task FetchTitleInternalDataInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetTitleInternalDataAsync(new GetTitleDataRequest());
            composition.InternalData = response.SafeResult().Data;
        }

        public async Task UploadTitleInternalDataFrom(TitleComposition composition)
        {
            foreach (var entry in composition.InternalData)
            {
                var response = await PlayFabAdminAPI.SetTitleInternalDataAsync(new SetTitleDataRequest
                {
                    Key = entry.Key,
                    Value = entry.Value
                });
                response.SafeResult();
            }
        }

        public async Task FetchTitleRegularDataInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetTitleDataAsync(new GetTitleDataRequest());
            composition.RegularData = response.SafeResult().Data;
        }

        public async Task UploadTitleRegularDataFrom(TitleComposition composition)
        {
            foreach (var entry in composition.RegularData)
            {
                var response = await PlayFabAdminAPI.SetTitleDataAsync(new SetTitleDataRequest
                {
                    Key = entry.Key,
                    Value = entry.Value
                });
                response.SafeResult();
            }
        }

        public async Task FetchVirtualCurrencyDataInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.ListVirtualCurrencyTypesAsync(new ListVirtualCurrencyTypesRequest());
            composition.CurrencyData = response.SafeResult().VirtualCurrencies;
        }

        public async Task UploadVirtualCurrencyDataFrom(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.AddVirtualCurrencyTypesAsync(new AddVirtualCurrencyTypesRequest()
            {
                VirtualCurrencies = composition.CurrencyData
            });
            response.SafeResult();
        }

        public async Task FetchCloudScriptInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetCloudScriptRevisionAsync(new GetCloudScriptRevisionRequest());
            composition.CloudScriptFiles = response.SafeResult().Files;
        }

        public async Task UploadCloudScriptFrom(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.UpdateCloudScriptAsync(new UpdateCloudScriptRequest()
            {
                Publish = true,
                Files = composition.CloudScriptFiles
            });
            response.SafeResult();
        }

        public async Task FetchCatalogDataInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetCatalogItemsAsync(new GetCatalogItemsRequest());
            var catalogItems = response.SafeResult().Catalog;
            if (catalogItems.Any())
            {
                composition.DefaultCatalog = catalogItems.First().CatalogVersion;
            }

            composition.CatalogData = new CatalogDataComposition()
            {
                Catalog = catalogItems
                    .Select(i => i.Bundle != null || i.Container != null ? i.Strip() : i)
                    .ToList(),
                ReuploadCatalog = catalogItems
                    .Where(i => i.Bundle != null || i.Container != null)
                    .ToList(),
            };
        }

        public async Task UploadCatalogDataFrom(TitleComposition composition)
        {
            List<CatalogItem> uploadData = composition.CatalogData.Catalog;
            if (uploadData == null || !uploadData.Any()) return;

            var response = await PlayFabAdminAPI.SetCatalogItemsAsync(new UpdateCatalogItemsRequest()
            {
                Catalog = composition.CatalogData.Catalog,
                CatalogVersion = composition.DefaultCatalog,
                SetAsDefaultCatalog = true
            });
            response.SafeResult();

        }


        public async Task PatchCatalogDataFrom(TitleComposition composition)
        {
            List<CatalogItem> uploadData = composition.CatalogData.ReuploadCatalog;
            if (uploadData == null || !uploadData.Any()) return;

            var response = await PlayFabAdminAPI.UpdateCatalogItemsAsync(new UpdateCatalogItemsRequest()
            {
                Catalog = uploadData,
                CatalogVersion = composition.DefaultCatalog,
                SetAsDefaultCatalog = true
            });
            response.SafeResult();

        }

        public async Task FetchPoliciesInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetPolicyAsync(new GetPolicyRequest()
            {
                PolicyName = "ApiPolicy"
            });
            composition.ApiPolicyStatements = response.SafeResult().Statements;
        }

        public async Task UploadPoliciesFrom(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.UpdatePolicyAsync(new UpdatePolicyRequest()
            {
                OverwritePolicy = true,
                PolicyName = "ApiPolicy",
                Statements = composition.ApiPolicyStatements
            });
            response.SafeResult();
        }

        public async Task FetchStatisticsDefinitionsInto(TitleComposition composition)
        {
            var response =
                await PlayFabAdminAPI.GetPlayerStatisticDefinitionsAsync(new GetPlayerStatisticDefinitionsRequest());
            composition.StatisticDefinitions = response.SafeResult().Statistics;
        }

        public async Task UploadStatisticsDefinitionsFrom(TitleComposition composition)
        {
            foreach (var stat in composition.StatisticDefinitions)
            {
                try
                {
                    var response = await PlayFabAdminAPI.CreatePlayerStatisticDefinitionAsync(
                        new CreatePlayerStatisticDefinitionRequest()
                        {
                            AggregationMethod = stat.AggregationMethod,
                            StatisticName = stat.StatisticName,
                            VersionChangeInterval = stat.VersionChangeInterval
                        });
                    response.SafeResult();
                }
                catch (PlayFabException ex) when (ex.Code == PlayFabErrorCode.StatisticNameConflict)
                {
                    _logger.Log($"  Statistic {stat.StatisticName} already exists! Updating instead", this);
                    var response = await PlayFabAdminAPI.UpdatePlayerStatisticDefinitionAsync(
                        new UpdatePlayerStatisticDefinitionRequest()
                        {
                            AggregationMethod = stat.AggregationMethod,
                            StatisticName = stat.StatisticName,
                            VersionChangeInterval = stat.VersionChangeInterval
                        });
                    response.SafeResult();
                }
            }
        }

        public async Task FetchDropTableDataInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetRandomResultTablesAsync(new GetRandomResultTablesRequest());
            composition.DropTablesData = response.SafeResult().Tables;
        }

        public async Task UploadDropTableDataFrom(TitleComposition composition)
        {
            if (composition.DropTablesData == null || !composition.DropTablesData.Any()) return;
            var response = await PlayFabAdminAPI.UpdateRandomResultTablesAsync( new UpdateRandomResultTablesRequest()
            {
                Tables = composition.DropTablesData.Select(entry => new RandomResultTable()
                {
                    Nodes = entry.Value.Nodes,
                    TableId = entry.Value.TableId
                }).ToList()
            });
            response.SafeResult();
        }

        public async Task FetchStoreDataInto(TitleComposition composition, string storeId)
        {
            var response = await PlayFabAdminAPI.GetStoreItemsAsync(new GetStoreItemsRequest()
            {
                StoreId = storeId
            });
            var result = response.SafeResult();
            composition.StoresData.Add(new StoreDataComposition(){
                Store  = result.Store,
                CatalogVersion = result.CatalogVersion,
                MarketingData = result.MarketingData,
                StoreId = result.StoreId
            });
        }

        public async Task FetchTitleNewsInto(TitleComposition composition)
        {
            var response = await PlayFabServerAPI.GetTitleNewsAsync(new GetTitleNewsRequest());
            composition.TitleNews = response.SafeResult().News;
        }

        public async Task UploadTitleNewsFrom(TitleComposition composition)
        {
            foreach (var news in composition.TitleNews)
            {
                var response = await PlayFabAdminAPI.AddNewsAsync(new AddNewsRequest()
                {
                    Title = news.Title,
                    Body = news.Body,
                    Timestamp = news.Timestamp
                });
                response.SafeResult();
            }
            
        }

        public async Task UploadStoresFrom(TitleComposition composition)
        {
            foreach (var store in composition.StoresData)
            {
                var response = await PlayFabAdminAPI.SetStoreItemsAsync(new UpdateStoreItemsRequest()
                {
                    CatalogVersion = store.CatalogVersion,
                    Store = store.Store,
                    StoreId = store.StoreId,
                    MarketingData = store.MarketingData
                });
                response.SafeResult();
            }

        }

        public async Task FetchContentListInto(TitleComposition composition)
        {
            var response = await PlayFabAdminAPI.GetContentListAsync(new GetContentListRequest());
            var result = response.SafeResult();
            composition.ContentListData = new ContentDataComposition()
            {
                TotalItems = result.ItemCount,
                TotalSize = result.TotalSize
            };

            var contents = composition.ContentListData.ContentInfos = result.Contents.Select(c => new ContentInfoComposition()
            {
                ContentInfo = c,
            }).ToList();

            composition.ContentListData.ContentInfos = contents;

            foreach (var content in contents)
            {
                var getUrlResult = await PlayFabServerAPI.GetContentDownloadUrlAsync(new GetContentDownloadUrlRequest()
                {
                    Key = content.ContentInfo.Key,
                    HttpMethod = "GET"
                });
                content.DownloadUrl = getUrlResult.SafeResult().URL;
            }


            await Task.WhenAll(contents.Select(async item =>
            {
                await _remoteTransferService.DownloadFileAsync(item.DownloadUrl, item.ContentInfo.Key);
                item.TempFilePath = item.ContentInfo.Key;
            }));

        }

        public async Task UploadContentListFrom(TitleComposition composition)
        {
            try
            {
                foreach (var content in composition.ContentListData.ContentInfos)
                {
                    var type = MimeMapping.GetMimeMapping(content.TempFilePath);
                    var uploadUrlResult = await PlayFabAdminAPI.GetContentUploadUrlAsync(
                        new GetContentUploadUrlRequest()
                        {
                            Key = content.ContentInfo.Key,
                            ContentType = type
                        });
                    await _remoteTransferService.UploadFileAsync(uploadUrlResult.SafeResult().URL,
                        content.TempFilePath);
                }
            }
            catch (PlayFabException ex) when (ex.Code == PlayFabErrorCode.BillingInformationRequired)
            {
                _logger.Error("Unable to upload content: Please set billing information on the target title!",this);
                return;
            }
            
        }


    }
}