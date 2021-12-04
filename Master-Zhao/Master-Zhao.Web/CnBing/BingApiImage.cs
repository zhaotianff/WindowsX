using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Master_Zhao.Web.CnBing
{
    public class Instrumentation
    {
        public string _type { get; set; }

        public string pingUrlBase { get; set; }

        public string pageLoadPingUrl { get; set; }
    }

    public class QueryContext
    {
        public string originalQuery { get; set; }

        public string alterationDisplayQuery { get; set; }

        public string alterationOverrideQuery { get; set; }

        public string alterationMethod { get; set; }

        public string alterationType { get; set; }
    }

    public class Thumbnail
    { 
        public int width { get; set; }

        public int height { get; set; }
    }

    public class InsightsMetadata
    {
        public int pagesIncludingCount { get; set; }

        public int availableSizesCount { get; set; }
    }

    public class ValueItem
    {
        public string webSearchUrl { get; set; }

        public string webSearchUrlPingSuffix { get; set; }

        public string name { get; set; }

        public string thumbnailUrl { get; set; }

        public string datePublished { get; set; }

        public string isFamilyFriendly { get; set; }

        public string contentUrl { get; set; }

        public string contentUrlPingSuffix { get; set; }

        public string hostPageUrl { get; set; }

        public string hostPageUrlPingSuffix { get; set; }

        public string contentSize { get; set; }

        public string encodingFormat { get; set; }

        public string hostPageDisplayUrl { get; set; }

        public int width { get; set; }

        public int height { get; set; }

        public string cDNContentUrl { get; set; }

        public string hostPageDiscoveredDate { get; set; }

        public Thumbnail thumbnail { get; set; }

        public string imageInsightsToken { get; set; }

        public InsightsMetadata insightsMetadata { get; set; }

        public string imageId { get; set; }

        public string accentColor { get; set; }
    }

    public class Root
    {

        public string _type { get; set; }

        public Instrumentation instrumentation { get; set; }

        public string readLink { get; set; }

        public string webSearchUrl { get; set; }

        public QueryContext queryContext { get; set; }

        public int totalEstimatedMatches { get; set; }

        public int nextOffset { get; set; }

        public int currentOffset { get; set; }

        public List<ValueItem> value { get; set; }
    }

}
