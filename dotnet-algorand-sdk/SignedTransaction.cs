using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Algorand
{
    /// 
    /// A serializable convenience type for packaging transactions with their signatures.
    /// 
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SignedTransaction
    {
        //@JsonProperty("txn")
        [JsonProperty(PropertyName = "txn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        //[DefaultValue(typeof(Transaction))]
        public Transaction tx = new Transaction();
        //@JsonProperty("sig")
        [JsonProperty(PropertyName = "sig", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Signature sig = new Signature(); // can be null
        //@JsonProperty("msig")
        [JsonProperty(PropertyName = "msig", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public MultisigSignature mSig = new MultisigSignature();
        //@JsonProperty("lsig")
        [JsonProperty(PropertyName = "lsig", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public LogicsigSignature lSig = new LogicsigSignature();

        //@JsonIgnore
        [JsonIgnore]
        public string transactionID = "";

        public SignedTransaction(Transaction tx, Signature sig, MultisigSignature mSig, LogicsigSignature lSig, string transactionID)
        {
            this.tx = JavaHelper<Transaction>.RequireNotNull(tx, "tx must not be null");
            this.mSig = JavaHelper<MultisigSignature>.RequireNotNull(mSig, "mSig must not be null");
            this.sig = JavaHelper<Signature>.RequireNotNull(sig, "sig must not be null");
            this.lSig = JavaHelper<LogicsigSignature>.RequireNotNull(lSig, "lSig must not be null");
            this.transactionID = JavaHelper<string>.RequireNotNull(transactionID, "txID must not be null");
        }

        public SignedTransaction(Transaction tx, Signature sig, string txId) :
            this(tx, sig, new MultisigSignature(), new LogicsigSignature(), txId) { }

        public SignedTransaction(Transaction tx, MultisigSignature mSig, string txId) :
            this(tx, new Signature(), mSig, new LogicsigSignature(), txId) { }

        public SignedTransaction(Transaction tx, LogicsigSignature lSig, string txId) :
            this(tx, new Signature(), new MultisigSignature(), lSig, txId) { }

        public SignedTransaction() { }

        //@JsonCreator
        //public SignedTransaction(
        //        @JsonProperty("txn") Transaction tx,
        //        @JsonProperty("sig") byte[] sig,
        //        @JsonProperty("msig") MultisigSignature mSig,
        //        @JsonProperty("lsig") LogicsigSignature lSig    )  
        [JsonConstructor]
        public SignedTransaction(Transaction tx, byte[] sig, MultisigSignature mSig, LogicsigSignature lSig)
        {
            if (tx != null) this.tx = tx;
            if (sig != null) this.sig = new Signature(sig);
            if (mSig != null) this.mSig = mSig;
            if (lSig != null) this.lSig = lSig;
            // don't recover the txid yet
        }

        //@Override
        //public override bool Equals(Object obj)
        //{
        //    if (obj is SignedTransaction actual)
        //    {
        //        if (!tx.Equals(actual.tx)) return false;
        //        if (!sig.Equals(actual.sig)) return false;
        //        if (!lSig.Equals(actual.lSig)) return false;
        //        return this.mSig.Equals(actual.mSig);
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
        //public override int GetHashCode()
        //{
        //    return base.GetHashCode();
        //}
    }
}