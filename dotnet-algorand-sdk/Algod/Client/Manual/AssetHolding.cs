using Newtonsoft.Json;
using System.ComponentModel;

namespace Algorand.Algod.Client.Model
{
    //[ApiModel(description = "AssetParams specifies the parameters for an asset")]
    [JsonObject]
    public class AssetHolding
    {
        //@SerializedName("creator")
        [JsonProperty(PropertyName = "creator")]
        [DefaultValue(null)]
        private string creator = null;

        //@SerializedName("amount")
        [JsonProperty(PropertyName = "amount")]
        [DefaultValue(null)]
        public ulong? Amount { get; private set; }

        //@SerializedName("frozen")
        [JsonProperty(PropertyName = "frozen")]
        [DefaultValue(null)]
        private bool? frozen = null;

        public AssetHolding() { Amount = null; }
    }
}
