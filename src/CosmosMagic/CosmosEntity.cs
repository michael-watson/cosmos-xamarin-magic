using Newtonsoft.Json;

namespace CosmosMagic
{
    public class CosmosEntity
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty("_etag")]
        public string _etag { get; set; }

        [JsonProperty("_rid")]
        public string _rid { get; set; }

        [JsonProperty("_self")]
        public string _self { get; set; }
    }
}