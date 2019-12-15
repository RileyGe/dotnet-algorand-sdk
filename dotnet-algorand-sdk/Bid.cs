using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Algorand
{
    /**
 * A raw serializable Bid class.
 */
    //    @JsonPropertyOrder(alphabetic= true)
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    [JsonObject]
    public class Bid
    {
        //@JsonProperty("bidder")
        [JsonProperty(PropertyName = "bidder")]
        public Address bidderKey = new Address(); // cannot be null
        //@JsonProperty("auc")
        [JsonProperty(PropertyName = "auc")]
        public Address auctionKey = new Address(); // cannot be null
        //@JsonProperty("cur")
        [JsonProperty(PropertyName = "cur")]
        [DefaultValue(0)]
        public ulong? bidCurrency = 0;
        //@JsonProperty("price")
        [JsonProperty(PropertyName = "price")]
        [DefaultValue(0)]
        public ulong? maxPrice = 0;
        //@JsonProperty("id")
        [JsonProperty(PropertyName = "id")]
        [DefaultValue(0)]
        public ulong? bidID = 0;
        //@JsonProperty("aid")
        [JsonProperty(PropertyName = "aid")]
        [DefaultValue(0)]
        public ulong? auctionID = 0;

        /**
         * Create a new bid
         * @param bidderKey
         * @param auctionKey
         * @param bidCurrency
         * @param maxPrice
         * @param bidID
         * @param auctionID
         */
        public Bid(Address bidderKey, Address auctionKey, ulong? bidCurrency, ulong? maxPrice, ulong? bidID, ulong? auctionID)
        {
            if (bidderKey != null) this.bidderKey = bidderKey;
            if (auctionKey != null) this.auctionKey = auctionKey;
            if (bidCurrency != null) this.bidCurrency = bidCurrency;
            if (maxPrice != null) this.maxPrice = maxPrice;
            if (bidID != null) this.bidID = bidID;
            if (auctionID != null) this.auctionID = auctionID;
        }

        //@JsonCreator
        [JsonConstructor]
        public Bid(
            [JsonProperty(PropertyName = "bidder")] byte[] bidderKey,
            [JsonProperty(PropertyName = "auc")] byte[] auctionKey,
            [JsonProperty(PropertyName = "cur")] ulong? bidCurrency,
            [JsonProperty(PropertyName = "price")] ulong? maxPrice,
            [JsonProperty(PropertyName = "id")] ulong? bidID,
            [JsonProperty(PropertyName = "aid")] ulong? auctionID)
        {
            if (bidderKey != null) this.bidderKey = new Address(bidderKey);
            if (auctionKey != null) this.auctionKey = new Address(auctionKey);
            if (bidCurrency != null) this.bidCurrency = bidCurrency;
            if (maxPrice != null) this.maxPrice = maxPrice;
            if (bidID != null) this.bidID = bidID;
            if (auctionID != null) this.auctionID = auctionID;
        }

        public Bid() { }

        //@Override
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is Bid)) return false;
            Bid bid = o as Bid;
            return bidderKey.Equals(bid.bidderKey) &&
                    auctionKey.Equals(bid.auctionKey) &&
                    bidCurrency == bid.bidCurrency &&
                    maxPrice == bid.maxPrice &&
                    bidID == bid.bidID &&
                    auctionID == bid.auctionID;
        }
        public override int GetHashCode()
        {
            return bidderKey.GetHashCode() + auctionKey.GetHashCode() + bidCurrency.GetHashCode() +
                maxPrice.GetHashCode() + bidID.GetHashCode() + auctionID.GetHashCode();
        }

    }
    /**
 * A serializable raw signed bid class.
 */
    //    @JsonPropertyOrder(alphabetic= true)
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    [JsonObject]
    public class SignedBid
    {
        //@JsonProperty("bid")
        [JsonProperty(PropertyName = "bid")]
        public Bid bid = new Bid();
        //@JsonProperty("sig")
        [JsonProperty(PropertyName = "sig")]
        public Signature sig = new Signature();
        //@JsonProperty("msig")
        [JsonProperty(PropertyName = "msig")]
        public MultisigSignature mSig = new MultisigSignature();

        public SignedBid(Bid bid, Signature sig, MultisigSignature mSig)
        {
            this.bid = JavaHelper<Bid>.RequireNotNull(bid, "tx must not be null");
            this.mSig = JavaHelper<MultisigSignature>.RequireNotNull(mSig, "mSig must not be null");
            this.sig = JavaHelper<Signature>.RequireNotNull(sig, "sig must not be null");
        }

        public SignedBid(Bid bid, Signature sig) : this(bid, sig, new MultisigSignature()) { }

        public SignedBid(Bid bid, MultisigSignature mSig) : this(bid, new Signature(), mSig) { }

        public SignedBid() { }

        //@JsonCreator
        [JsonConstructor]
        public SignedBid([JsonProperty(PropertyName = "bid")] Bid bid,
            [JsonProperty(PropertyName = "sig")] byte[] sig,
            [JsonProperty(PropertyName = "msig")] MultisigSignature mSig)
        {
            if (bid != null) this.bid = bid;
            if (sig != null) this.sig = new Signature(sig);
            if (mSig != null) this.mSig = mSig;
        }

        //@Override
        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is SignedBid)) return false;
            SignedBid signedBid = o as SignedBid;
            return bid.Equals(signedBid.bid) &&
                    sig.Equals(signedBid.sig) &&
                    mSig.Equals(signedBid.mSig);
        }
        public override int GetHashCode()
        {
            return bid.GetHashCode() + sig.GetHashCode() + mSig.GetHashCode();
        }
    }
}