using System.Collections.Generic;
using PlayFab.AdminModels;
using PlayFab.ServerModels;
using CatalogItem = PlayFab.AdminModels.CatalogItem;
using RandomResultTableListing = PlayFab.AdminModels.RandomResultTableListing;

namespace PlayFabToolSDK.Models
{

    /// <summary>
    /// Reference to a specific title that allows authentication
    /// </summary>
    public struct TitleReference
    {
        public string TitleId;
        public string DeveloperKey;
    }

    /// <summary>
    /// Title composition is an intermediate container that holds every single bit of data fetched from the title
    /// </summary>
    public class TitleComposition
    {
        public List<CloudScriptFile> CloudScriptFiles = new List<CloudScriptFile>();
        public List<VirtualCurrencyData> CurrencyData = new List<VirtualCurrencyData>();

        public Dictionary<string, string> InternalData = new Dictionary<string, string>();
        public Dictionary<string, string> RegularData = new Dictionary<string, string>();

        public Dictionary<string, RandomResultTableListing> DropTablesData =
            new Dictionary<string, RandomResultTableListing>();

        public List<StoreDataComposition> StoresData = new List<StoreDataComposition>();
        public ContentDataComposition ContentListData = null;
        public CatalogDataComposition CatalogData = null;
        public List<TitleNewsItem> TitleNews = new List<TitleNewsItem>();
        public List<PermissionStatement> ApiPolicyStatements = new List<PermissionStatement>();
        public List<PlayerStatisticDefinition> StatisticDefinitions = new List<PlayerStatisticDefinition>();
        public string DefaultCatalog = "1";
    }

    public class CatalogDataComposition
    {
        public List<CatalogItem> Catalog;
        public List<CatalogItem> ReuploadCatalog;
    }

    public class StoreDataComposition
    {
        public List<StoreItem> Store;
        public StoreMarketingModel MarketingData;
        public string StoreId;
        public string CatalogVersion;
    }

    public class ContentDataComposition
    {
        public uint TotalSize;
        public int TotalItems;
        public List<ContentInfoComposition> ContentInfos;
    }

    public class ContentInfoComposition
    {
        public ContentInfo ContentInfo;
        public string DownloadUrl;
        public string TempFilePath;
    }
}