using Newtonsoft.Json;
using System.ComponentModel;

namespace Algorand.Algod.Model
{
    //[ApiModel(description = "AssetParams specifies the parameters for an asset")]
    [JsonObject]
    public class AssetHolding
    {
        //@SerializedName("creator")
        [JsonProperty(PropertyName = "creator")]
        [DefaultValue(null)]
        public string Creator { get; private set; }

        //@SerializedName("amount")
        [JsonProperty(PropertyName = "amount")]
        [DefaultValue(null)]
        public ulong? Amount { get; private set; }

        //@SerializedName("frozen")
        [JsonProperty(PropertyName = "frozen")]
        [DefaultValue(null)]
        public bool? Frozen { get; private set; }

        public AssetHolding() { Amount = null; }
    }
}
