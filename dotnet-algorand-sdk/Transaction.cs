using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Algorand
{
    /// 
    /// A raw serializable transaction class, used to generate transactions to broadcast to the network.
    /// This is distinct from algod.model.Transaction, which is only returned for GET requests to algod.
    /// 
    [JsonObject]
    public class Transaction
    {
        //private static readonly byte[] TX_SIGN_PREFIX = Encoding.UTF8.GetBytes("TX");
        private static readonly byte[] TX_SIGN_PREFIX = Encoding.UTF8.GetBytes("TX");
        private const int LEASE_LENGTH = 32;

        [JsonProperty(PropertyName = "type")]
        [DefaultValue("")]
        public Type type = Type.Default;

        // Instead of embedding POJOs and using JsonUnwrapped, we explicitly export inner fields. This circumvents our encoders'
        // inability to sort child fields.
        /* header fields ***********************************************************/
        [JsonProperty(PropertyName = "snd")]
        public Address sender = new Address();
        //@JsonProperty("fee")
        [JsonProperty(PropertyName = "fee")]
        [DefaultValue(0)]
        public ulong? fee = 0;
        //@JsonProperty("fv")
        [JsonProperty(PropertyName = "fv")]
        [DefaultValue(0)]
        public ulong? firstValid = 0;
        //@JsonProperty("lv")
        [JsonProperty(PropertyName = "lv")]
        [DefaultValue(0)]
        public ulong? lastValid = 0;
        //@JsonProperty("note")
        [JsonProperty(PropertyName = "note")]
        public byte[] note;
        //@JsonProperty("gen")
        [JsonProperty(PropertyName = "gen")]
        [DefaultValue("")]
        public string genesisID = "";
        //@JsonProperty("gh")
        [JsonProperty(PropertyName = "gh")]
        public Digest genesisHash = new Digest();
        //@JsonProperty("grp")
        [JsonProperty(PropertyName = "grp")]
        public Digest group = new Digest();
        //@JsonProperty("lx")
        [JsonProperty(PropertyName = "lx")]
        public byte[] lease;

        /* payment fields  *********************************************************/
        //@JsonProperty("amt")
        [JsonProperty(PropertyName = "amt")]
        [DefaultValue(0)]
        public ulong? amount = 0;
        //@JsonProperty("rcv")
        [JsonProperty(PropertyName = "rcv")]
        public Address receiver = new Address();
        //@JsonProperty("close")
        [JsonProperty(PropertyName = "close")]
        public Address closeRemainderTo = new Address(); // can be null, optional

        /* keyreg fields ***********************************************************/
        // VotePK is the participation public key used in key registration transactions
        //@JsonProperty("votekey")
        [JsonProperty(PropertyName = "votekey")]
        public ParticipationPublicKey votePK = new ParticipationPublicKey();

        // selectionPK is the VRF public key used in key registration transactions
        //@JsonProperty("selkey")
        [JsonProperty(PropertyName = "selkey")]
        public VRFPublicKey selectionPK = new VRFPublicKey();
        // voteFirst is the first round this keyreg tx is valid for
        //@JsonProperty("votefst")
        [JsonProperty(PropertyName = "votefst")]
        [DefaultValue(0)]
        public ulong? voteFirst = 0;

        // voteLast is the last round this keyreg tx is valid for
        //@JsonProperty("votelst")
        [JsonProperty(PropertyName = "votelst")]
        [DefaultValue(0)]
        public ulong? voteLast = 0;
        // voteKeyDilution
        //@JsonProperty("votekd")
        [JsonProperty(PropertyName = "votekd")]
        [DefaultValue(0)]
        public ulong? voteKeyDilution = 0;

        /* asset creation and configuration fields *********************************/
        //@JsonProperty("apar")
        [JsonProperty(PropertyName = "apar")]
        public AssetParams assetParams = new AssetParams();
        //@JsonProperty("caid")
        [JsonProperty(PropertyName = "caid")]
        [DefaultValue(0)]
        public ulong? assetIndex = 0;

        /* asset transfer fields ***************************************************/
        //@JsonProperty("xaid")
        [JsonProperty(PropertyName = "xaid")]
        [DefaultValue(0)]
        public ulong? xferAsset = 0;

        // The amount of asset to transfer. A zero amount transferred to self
        // allocates that asset in the account's Assets map.
        //@JsonProperty("aamt")
        [JsonProperty(PropertyName = "aamt")]
        [DefaultValue(0)]
        public ulong? assetAmount = 0;

        // The sender of the transfer.  If this is not a zero value, the real
        // transaction sender must be the Clawback address from the AssetParams. If
        // this is the zero value, the asset is sent from the transaction's Sender.
        //@JsonProperty("asnd")
        [JsonProperty(PropertyName = "asnd")]
        public Address assetSender = new Address();

        // The receiver of the transfer.
        //@JsonProperty("arcv")
        [JsonProperty(PropertyName = "arcv")]
        public Address assetReceiver = new Address();

        // Indicates that the asset should be removed from the account's Assets map,
        // and specifies where the remaining asset holdings should be transferred.
        // It's always valid to transfer remaining asset holdings to the AssetID
        // account.
        //@JsonProperty("aclose")
        [JsonProperty(PropertyName = "aclose")]
        public Address assetCloseTo = new Address();

        /* asset freeze fields */
        //@JsonProperty("fadd")
        [JsonProperty(PropertyName = "fadd")]
        public Address freezeTarget = new Address();
        //@JsonProperty("faid")
        [JsonProperty(PropertyName = "faid")]
        [DefaultValue(0)]
        public ulong? assetFreezeID = 0;
        //@JsonProperty("afrz")
        [JsonProperty(PropertyName = "afrz")]
        [DefaultValue(false)]
        public bool freezeState = false;
        /// <summary>
        /// Create a payment transaction
        /// </summary>
        /// <param name="fromAddr">source address</param>
        /// <param name="toAddr">destination address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="amount">payment amount</param>
        /// <param name="firstRound">first valid round</param>
        /// <param name="lastRound">last valid round</param>
        public Transaction(Address fromAddr, Address toAddr, ulong? fee, ulong? amount, ulong? firstRound,
            ulong? lastRound) : this(fromAddr, fee, firstRound, lastRound, null, amount, toAddr, "", new Digest()) { }

        public Transaction(Address fromAddr, Address toAddr, ulong? fee, ulong? amount, ulong? firstRound, ulong? lastRound,
                           string genesisID, Digest genesisHash) : this(fromAddr, fee, firstRound, lastRound, null, amount,
                               toAddr, genesisID, genesisHash)
        { }
        /// <summary>
        /// Create a payment transaction. Make sure to sign with a suggested fee.
        /// </summary>
        /// <param name="fromAddr">source address</param>
        /// <param name="toAddr">destination address</param>
        /// <param name="amount">amount to send</param>
        /// <param name="firstRound">first valid round</param>
        /// <param name="lastRound">last valid round</param>
        /// <param name="genesisID">genesis id</param>
        /// <param name="genesisHash">genesis hash</param>
        public Transaction(Address fromAddr, Address toAddr, ulong? amount, ulong? firstRound, ulong? lastRound,
            string genesisID, Digest genesisHash) : this(fromAddr, 0, firstRound, lastRound, null, amount, toAddr, genesisID, genesisHash) { }

        public Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note, ulong? amount,
            Address receiver, string genesisID, Digest genesisHash) : this(sender, fee, firstValid, lastValid, note,
                genesisID, genesisHash, amount, receiver, new Address())
        { }

        public Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note, String genesisID, Digest genesisHash,
                           ulong? amount, Address receiver, Address closeRemainderTo)
        {
            this.type = Type.Payment;
            if (sender != null) this.sender = sender;
            if (fee != null) this.fee = fee;
            if (firstValid != null) this.firstValid = firstValid;
            if (lastValid != null) this.lastValid = lastValid;
            if (note != null) this.note = note;
            if (genesisID != null) this.genesisID = genesisID;
            if (genesisHash != null) this.genesisHash = genesisHash;
            if (amount != null) this.amount = amount;
            if (receiver != null) this.receiver = receiver;
            if (closeRemainderTo != null) this.closeRemainderTo = closeRemainderTo;
        }
        /// <summary>
        /// Create a key registration transaction. No field can be null except the note field.
        /// </summary>
        /// <param name="sender">source address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="firstValid">first valid round</param>
        /// <param name="lastValid">last valid round</param>
        /// <param name="note">optional notes field (can be null)</param>
        /// <param name="genesisID">genesis id</param>
        /// <param name="genesisHash">genesis hash</param>
        /// <param name="votePK">the new participation key to register</param>
        /// <param name="vrfPK">the sortition key to register</param>
        /// <param name="voteFirst">key reg valid first round</param>
        /// <param name="voteLast">key reg valid last round</param>
        /// <param name="voteKeyDilution">key reg dilution</param>
        public Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note,
                           String genesisID, Digest genesisHash,
                           ParticipationPublicKey votePK, VRFPublicKey vrfPK,
                           ulong? voteFirst, ulong? voteLast, ulong? voteKeyDilution)
        {
            // populate with default values which will be ignored...
            this.type = Type.KeyRegistration;
            if (sender != null) this.sender = sender;
            if (fee != null) this.fee = fee;
            if (firstValid != null) this.firstValid = firstValid;
            if (lastValid != null) this.lastValid = lastValid;
            if (note != null) this.note = note;
            if (genesisID != null) this.genesisID = genesisID;
            if (genesisHash != null) this.genesisHash = genesisHash;
            if (votePK != null) this.votePK = votePK;
            if (vrfPK != null) this.selectionPK = vrfPK;
            if (voteFirst != null) this.voteFirst = voteFirst;
            if (voteLast != null) this.voteLast = voteLast;
            if (voteKeyDilution != null) this.voteKeyDilution = voteKeyDilution;
        }
        /// <summary>
        /// Create an asset creation transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        /// </summary>
        /// <param name="sender">source address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="firstValid">first valid round</param>
        /// <param name="lastValid">last valid round</param>
        /// <param name="note">optional note field (can be null)</param>
        /// <param name="genesisID"></param>
        /// <param name="genesisHash"></param>
        /// <param name="assetTotal">total asset issuance</param>
        /// <param name="assetDecimals"></param>
        /// <param name="defaultFrozen">whether accounts have this asset frozen by default</param>
        /// <param name="assetUnitName">name of unit of the asset</param>
        /// <param name="assetName">name of the asset</param>
        /// <param name="url">where more information about the asset can be retrieved</param>
        /// <param name="metadataHash">specifies a commitment to some unspecified asset metadata. The format of this metadata is up to the application</param>
        /// <param name="manager">account which can reconfigure the asset</param>
        /// <param name="reserve">account whose asset holdings count as non-minted</param>
        /// <param name="freeze">account which can freeze or unfreeze holder accounts</param>
        /// <param name="clawback">account which can issue clawbacks against holder accounts</param>
        private Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note,
                           string genesisID, Digest genesisHash, ulong? assetTotal, int assetDecimals, bool defaultFrozen,
                           string assetUnitName, string assetName, string url, byte[] metadataHash,
                           Address manager, Address reserve, Address freeze, Address clawback)
        {
            this.type = Type.AssetConfig;
            if (sender != null) this.sender = sender;
            if (fee != null) this.fee = fee;
            if (firstValid != null) this.firstValid = firstValid;
            if (lastValid != null) this.lastValid = lastValid;
            if (note != null) this.note = note;
            if (genesisID != null) this.genesisID = genesisID;
            if (genesisHash != null) this.genesisHash = genesisHash;

            this.assetParams = new AssetParams(assetTotal, assetDecimals, defaultFrozen, assetUnitName, assetName, url, metadataHash, manager, reserve, freeze, clawback);
        }
        /// <summary>
        /// Create an asset creation transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        /// </summary>
        /// <param name="sender">source address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="firstValid">first valid round</param>
        /// <param name="lastValid">last valid round</param>
        /// <param name="note">optional note field (can be null)</param>
        /// <param name="genesisID"></param>
        /// <param name="genesisHash"></param>
        /// <param name="assetTotal">total asset issuance</param>
        /// <param name="assetDecimals">asset decimal precision</param>
        /// <param name="defaultFrozen">whether accounts have this asset frozen by default</param>
        /// <param name="assetUnitName">name of unit of the asset</param>
        /// <param name="assetName">name of the asset</param>
        /// <param name="url">where more information about the asset can be retrieved</param>
        /// <param name="metadataHash">specifies a commitment to some unspecified asset metadata. The format of this metadata is up to the application</param>
        /// <param name="manager">account which can reconfigure the asset</param>
        /// <param name="reserve">account whose asset holdings count as non-minted</param>
        /// <param name="freeze">account which can freeze or unfreeze holder accounts</param>
        /// <param name="clawback">account which can issue clawbacks against holder accounts</param>
        /// <returns></returns>
        public static Transaction CreateAssetCreateTransaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note,
                           string genesisID, Digest genesisHash, ulong? assetTotal, int assetDecimals, bool defaultFrozen,
                           string assetUnitName, string assetName, string url, byte[] metadataHash,
                           Address manager, Address reserve, Address freeze, Address clawback)
        {
            return new Transaction(
                    sender, fee, firstValid, lastValid, note,
                    genesisID, genesisHash, assetTotal, assetDecimals, defaultFrozen,
                    assetUnitName, assetName, url, metadataHash,
                    manager, reserve, freeze, clawback);
        }        
        /// <summary>
        /// Create an asset configuration transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        /// </summary>
        /// <param name="sender">source address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="firstValid">first valid round</param>
        /// <param name="lastValid">last valid round</param>
        /// <param name="note">optional note field (can be null)</param>
        /// <param name="genesisID"></param>
        /// <param name="genesisHash"></param>
        /// <param name="index">asset index</param>
        /// <param name="manager">account which can reconfigure the asset</param>
        /// <param name="reserve">account whose asset holdings count as non-minted</param>
        /// <param name="freeze">account which can freeze or unfreeze holder accounts</param>
        /// <param name="clawback">account which can issue clawbacks against holder accounts</param>
        private Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note,
                           string genesisID, Digest genesisHash, ulong? index,
                        Address manager, Address reserve, Address freeze, Address clawback)
        {

            this.type = Type.AssetConfig;
            if (sender != null) this.sender = sender;
            if (fee != null) this.fee = fee;
            if (firstValid != null) this.firstValid = firstValid;
            if (lastValid != null) this.lastValid = lastValid;
            if (note != null) this.note = note;
            if (genesisID != null) this.genesisID = genesisID;
            if (genesisHash != null) this.genesisHash = genesisHash;
            this.assetParams = new AssetParams(0, 0, false, "", "", "", null, manager, reserve, freeze, clawback);
            assetIndex = index;
        }
        /// <summary>
        /// Create an asset configuration transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        /// </summary>
        /// <param name="sender">source address</param>
        /// <param name="fee">transaction fee</param>
        /// <param name="firstValid">first valid round</param>
        /// <param name="lastValid">last valid round</param>
        /// <param name="note">optional note field (can be null)</param>
        /// <param name="genesisID">corresponds to the id of the network</param>
        /// <param name="genesisHash"></param>
        /// <param name="index">asset index</param>
        /// <param name="manager">account which can reconfigure the asset</param>
        /// <param name="reserve">account whose asset holdings count as non-minted</param>
        /// <param name="freeze">account which can freeze or unfreeze holder accounts</param>
        /// <param name="clawback">account which can issue clawbacks against holder accounts</param>
        /// <param name="strictEmptyAddressChecking">if true, disallow empty admin accounts from being set (preventing accidental disable of admin features)</param>
        /// <returns>the asset configure transaction</returns>
        public static Transaction CreateAssetConfigureTransaction(
                Address sender,
                ulong? fee,
                ulong? firstValid,
                ulong? lastValid,
                byte[] note,
                string genesisID,
                Digest genesisHash,
                ulong? index,
                Address manager,
                Address reserve,
                Address freeze,
                Address clawback,
                bool strictEmptyAddressChecking)
        {
            Address defaultAddr = new Address();
            if (strictEmptyAddressChecking && (
                    (manager == null || manager.Equals(defaultAddr)) || (reserve == null || reserve.Equals(defaultAddr)) ||
                    (freeze == null || freeze.Equals(defaultAddr)) || (clawback == null || clawback.Equals(defaultAddr))))
            {
                throw new Exception("strict empty address checking requested but "
                        + "empty or default address supplied to one or more manager addresses");
            }
            return new Transaction(
                    sender,
                    fee,
                    firstValid,
                    lastValid,
                    note,
                    genesisID,
                    genesisHash,
                    index,
                    manager,
                    reserve,
                    freeze,
                    clawback);
        }

        // workaround for nested JsonValue classes
        // @JsonCreator
        [JsonConstructor]
        private Transaction([JsonProperty(PropertyName = "type")] Type type,
                            //header fields
                            [JsonProperty(PropertyName = "snd")] byte[] sender,
                            [JsonProperty(PropertyName = "fee")] ulong? fee,
                            [JsonProperty(PropertyName = "fv")] ulong? firstValid,
                            [JsonProperty(PropertyName = "lv")] ulong? lastValid,
                            [JsonProperty(PropertyName = "note")] byte[] note,
                            [JsonProperty(PropertyName = "gen")] String genesisID,
                            [JsonProperty(PropertyName = "gh")] byte[] genesisHash,
                            [JsonProperty(PropertyName = "lx")] byte[] lease,
                            [JsonProperty(PropertyName = "grp")] byte[] group,
                            // payment fields
                            [JsonProperty(PropertyName = "amt")] ulong? amount,
                            [JsonProperty(PropertyName = "rcv")] byte[] receiver,
                            [JsonProperty(PropertyName = "close")] byte[] closeRemainderTo,
                            // keyreg fields
                            [JsonProperty(PropertyName = "votekey")] byte[] votePK,
                            [JsonProperty(PropertyName = "selkey")] byte[] vrfPK,
                            [JsonProperty(PropertyName = "votefst")] ulong? voteFirst,
                            [JsonProperty(PropertyName = "votelst")] ulong? voteLast,
                            [JsonProperty(PropertyName = "votekd")] ulong? voteKeyDilution,
                            // asset creation and configuration
                            [JsonProperty(PropertyName = "apar")] AssetParams assetParams,
                            [JsonProperty(PropertyName = "caid")] ulong? assetIndex,
                            // Asset xfer transaction fields
                            [JsonProperty(PropertyName = "xaid")] ulong? xferAsset,
                            [JsonProperty(PropertyName = "aamt")] ulong? assetAmount,
                            [JsonProperty(PropertyName = "asnd")] byte[] assetSender,
                            [JsonProperty(PropertyName = "arcv")] byte[] assetReceiver,
                            [JsonProperty(PropertyName = "aclose")] byte[] assetCloseTo,
                            // asset freeze fields
                            [JsonProperty(PropertyName = "fadd")] byte[] freezeTarget,
                            [JsonProperty(PropertyName = "faid")] ulong? assetFreezeID,
                            [JsonProperty(PropertyName = "afrz")] bool freezeState) : this(
                                type, new Address(sender), fee, firstValid, lastValid, note,
                 genesisID, new Digest(genesisHash), lease, new Digest(group),               // payment fields
                 amount, new Address(receiver), new Address(closeRemainderTo),               // keyreg fields
                 new ParticipationPublicKey(votePK), new VRFPublicKey(vrfPK),
                 voteFirst, voteLast, voteKeyDilution,
                 // asset creation and configuration
                 assetParams, assetIndex,                // asset transfer fields
                 xferAsset, assetAmount, new Address(assetSender), new Address(assetReceiver),
                 new Address(assetCloseTo), new Address(freezeTarget), assetFreezeID, freezeState)
        { }

        /**
         * This is the private constructor which takes all the fields of Transaction
         **/
        private Transaction(Type type,
                            //header fields
                            Address sender, ulong? fee, ulong? firstValid, ulong? lastValid,
                            byte[] note, String genesisID, Digest genesisHash, byte[] lease, Digest group,
                            // payment fields
                            ulong? amount, Address receiver, Address closeRemainderTo,
                            // keyreg fields
                            ParticipationPublicKey votePK, VRFPublicKey vrfPK, ulong? voteFirst, ulong? voteLast,
                            // voteKeyDilution
                            ulong? voteKeyDilution,
                            // asset creation and configuration
                            AssetParams assetParams, ulong? assetIndex,
                            // asset transfer fields
                            ulong? xferAsset, ulong? assetAmount, Address assetSender, Address assetReceiver,
                            Address assetCloseTo, Address freezeTarget, ulong? assetFreezeID, bool freezeState)
        {
            this.type = type;
            if (sender != null) this.sender = sender;
            if (fee != null) this.fee = fee;
            if (firstValid != null) this.firstValid = firstValid;
            if (lastValid != null) this.lastValid = lastValid;
            if (note != null) this.note = note;
            if (genesisID != null) this.genesisID = genesisID;
            if (genesisHash != null) this.genesisHash = genesisHash;
            if (lease != null) this.lease = lease;
            if (group != null) this.group = group;
            if (amount != null) this.amount = amount;
            if (receiver != null) this.receiver = receiver;
            if (closeRemainderTo != null) this.closeRemainderTo = closeRemainderTo;
            if (votePK != null) this.votePK = votePK;
            if (vrfPK != null) this.selectionPK = vrfPK;
            if (voteFirst != null) this.voteFirst = voteFirst;
            if (voteLast != null) this.voteLast = voteLast;
            if (voteKeyDilution != null) this.voteKeyDilution = voteKeyDilution;
            if (assetParams != null) this.assetParams = assetParams;
            if (assetIndex != null) this.assetIndex = assetIndex;
            if (xferAsset != null) this.xferAsset = xferAsset;
            if (assetAmount != null) this.assetAmount = assetAmount;
            if (assetSender != null) this.assetSender = assetSender;
            if (assetReceiver != null) this.assetReceiver = assetReceiver;
            if (assetCloseTo != null) this.assetCloseTo = assetCloseTo;
            if (freezeTarget != null) this.freezeTarget = freezeTarget;
            if (assetFreezeID != null) this.assetFreezeID = assetFreezeID;
            this.freezeState = freezeState;
        }

        public Transaction() { }
        /// <summary>
        /// Base constructor with flat fee for asset xfer/freeze/destroy transactions.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstRound">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastRound">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis of the network</param>
        private Transaction(
                Type type,
                ulong? flatFee,
                ulong? firstRound,
                ulong? lastRound,
                byte[] note,
                Digest genesisHash)
        {

            this.type = type;
            if (flatFee != null) this.fee = flatFee;
            if (firstRound != null) this.firstValid = firstRound;
            if (lastRound != null) this.lastValid = lastRound;
            if (note != null) this.note = note;
            if (genesisHash != null) this.genesisHash = genesisHash;
        }        
        /// <summary>
        /// Creates a tx to mark the account as willing to accept the asset.
        /// </summary>
        /// <param name="acceptingAccount">checksummed, human-readable address that will accept receiving the asset.</param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstRound">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastRound">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisID">corresponds to the id of the network</param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis</param>
        /// <param name="assetIndex">the asset index</param>
        /// <returns></returns>
        public static Transaction CreateAssetAcceptTransaction( //AssetTransaction
                Address acceptingAccount,
                ulong? flatFee,
                ulong? firstRound,
                ulong? lastRound,
                byte[] note,
                string genesisID,
                Digest genesisHash,
                ulong? assetIndex)
        {

            Transaction tx = CreateAssetTransferTransaction(
                    acceptingAccount,
                    acceptingAccount,
                    new Address(),
                    0,
                    flatFee,
                    firstRound,
                    lastRound,
                    note,
                    genesisID,
                    genesisHash,
                    assetIndex);

            return tx;
        }
        /// <summary>
        /// Creates a tx to destroy the asset
        /// </summary>
        /// <param name="senderAccount">checksummed, human-readable address of the sender </param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstValid">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastValid">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis</param>
        /// <param name="assetIndex">the asset ID to destroy</param>
        /// <returns></returns>
        public static Transaction CreateAssetDestroyTransaction(
                Address senderAccount,
                ulong? flatFee,
                ulong? firstValid,
                ulong? lastValid,
                byte[] note,
                Digest genesisHash,
                ulong? assetIndex)
        {
            Transaction tx = new Transaction(
                    Type.AssetConfig,
                    flatFee,
                    firstValid,
                    lastValid,
                    note,
                    genesisHash);

            if (assetIndex != null) tx.assetIndex = assetIndex;
            if (senderAccount != null) tx.sender = senderAccount;
            return tx;
        }
        /// <summary>
        /// Creates a tx to freeze/unfreeze assets
        /// </summary>
        /// <param name="senderAccount">checksummed, human-readable address of the sender </param>
        /// <param name="accountToFreeze"></param>
        /// <param name="freezeState"></param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstValid">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastValid">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis of the network</param>
        /// <param name="assetIndex">the asset ID to destroy</param>
        /// <returns></returns>
        public static Transaction CreateAssetFreezeTransaction(
                Address senderAccount,
                Address accountToFreeze,
                bool freezeState,
                ulong? flatFee,
                ulong? firstValid,
                ulong? lastValid,
                byte[] note,
                Digest genesisHash,
                ulong? assetIndex)
        {
            Transaction tx = new Transaction(
                    Type.AssetFreeze,
                    flatFee,
                    firstValid,
                    lastValid,
                    note,
                    genesisHash);

            if (senderAccount != null) tx.sender = senderAccount;
            if (accountToFreeze != null) tx.freezeTarget = accountToFreeze;
            if (assetIndex != null) tx.assetFreezeID = assetIndex;
            tx.freezeState = freezeState;
            return tx;
        }        
        /// <summary>
        /// Creates a tx for revoking an asset from an account and sending it to another
        /// </summary>
        /// <param name="transactionSender">checksummed, human-readable address that will send the transaction</param>
        /// <param name="assetRevokedFrom">checksummed, human-readable address that will have assets taken from</param>
        /// <param name="assetReceiver">checksummed, human-readable address what will receive the assets</param>
        /// <param name="assetAmount">the number of assets to send</param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstRound">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastRound">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisID">corresponds to the id of the network</param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis of the network</param>
        /// <param name="assetIndex">the asset index</param>
        /// <returns></returns>
        public static Transaction CreateAssetRevokeTransaction(// AssetTransaction
                Address transactionSender,
                Address assetRevokedFrom,
                Address assetReceiver,
                ulong? assetAmount,
                ulong? flatFee,
                ulong? firstRound,
                ulong? lastRound,
                byte[] note,
                string genesisID,
                Digest genesisHash,
                ulong? assetIndex)
        {

            Transaction tx = new Transaction(
                    Type.AssetTransfer,
                    flatFee,    // fee
                    firstRound, // fv
                    lastRound, // lv
                    note, //note
                    genesisHash)
            {
                assetReceiver = assetReceiver, //arcv
                assetSender = assetRevokedFrom, //asnd        
                assetAmount = assetAmount, // aamt
                sender = transactionSender // snd
            }; // gh
            if (assetIndex != null) tx.xferAsset = assetIndex;
            return tx;
        }        
        /// <summary>
        /// Creates a tx for sending some asset from an asset holder to another user.
        /// The asset receiver must have marked itself as willing to accept the asset.
        /// </summary>
        /// <param name="assetSender">checksummed, human-readable address that will send the transaction and assets</param>
        /// <param name="assetReceiver">checksummed, human-readable address what will receive the assets</param>
        /// <param name="assetCloseTo">checksummed, human-readable address that behaves as a close-to address for the asset transaction; the remaining
        /// assets not sent to assetReceiver will be sent to assetCloseTo. Leave blank for no close-to behavior.</param>
        /// <param name="assetAmount">the number of assets to send</param>
        /// <param name="flatFee">the transaction flat fee</param>
        /// <param name="firstRound">the first round this txn is valid (txn semantics unrelated to asset management)</param>
        /// <param name="lastRound">the last round this txn is valid</param>
        /// <param name="note"></param>
        /// <param name="genesisID">corresponds to the id of the network</param>
        /// <param name="genesisHash">corresponds to the base64-encoded hash of the genesis of the network</param>
        /// <param name="assetIndex">the asset index</param>
        /// <returns></returns>
        public static Transaction CreateAssetTransferTransaction(// AssetTransaction
                Address assetSender,
                Address assetReceiver,
                Address assetCloseTo,
                ulong? assetAmount,
                ulong? flatFee,
                ulong? firstRound,
                ulong? lastRound,
                byte[] note,
                string genesisID,
                Digest genesisHash,
                ulong? assetIndex)
        {

            Transaction tx = new Transaction(
                    Type.AssetTransfer,
                    flatFee,    // fee
                    firstRound, // fv
                    lastRound, // lv
                    note, //note
                    genesisHash)
            {
                assetReceiver = assetReceiver, //arcv
                assetCloseTo = assetCloseTo, // aclose
                assetAmount = assetAmount, // aamt
                sender = assetSender // snd
            }; // gh
            if (assetIndex != null) tx.xferAsset = assetIndex;
            return tx;
        }
        ///
        /// TxType represents a transaction type.
        ///
        [JsonConverter(typeof(Type2StringConverter))]
        public class Type
        {
            private int type;
            public static readonly Type Default = new Type(0);
            public static readonly Type Payment = new Type(1);
            public static readonly Type KeyRegistration = new Type(2);
            public static readonly Type AssetConfig = new Type(3);
            public static readonly Type AssetTransfer = new Type(4);
            public static readonly Type AssetFreeze = new Type(5);

            private static Dictionary<string, int> namesMap = new Dictionary<string, int> {
                {"", 0 }, {"pay", 1}, {"keyreg", 2},
                { "acfg", 3}, {"axfer", 4}, { "afrz", 5}
            };
            /// <summary>
            /// Return the enumeration for the given string value. Required for JSON serialization.
            /// </summary>
            /// <param name="value">string representation</param>
            [JsonConstructor]
            public Type(string value)
            {
                if (namesMap.TryGetValue(value, out int typeint))
                    this.type = typeint;
                else this.type = 0;
                //this.type = namesMap.GetValueOrDefault(value, 0);
            }

            public Type(int value)
            {
                this.type = value;
            }
            /// <summary>
            /// Return the string value for this enumeration. Required for JSON serialization.
            /// </summary>
            /// <returns>string value</returns>
            public string ToValue()
            {
                //namesMap.Add()
                foreach (var entry in namesMap)
                {
                    if (entry.Value == this.type)
                        return entry.Key;
                }
                return null;
            }
        }
        /// <summary>
        /// Return encoded representation of the transaction with a prefix
        /// suitable for signing
        /// </summary>
        /// <returns></returns>
        public byte[] BytesToSign()
        {
            byte[] encodedTx = Encoder.EncodeToMsgPack(this);
            var retList = new List<byte>();
            retList.AddRange(TX_SIGN_PREFIX);
            retList.AddRange(encodedTx);
            return retList.ToArray();
        }
        /// <summary>
        /// Return transaction ID as Digest
        /// </summary>
        /// <returns></returns>
        public Digest RawTxID()
        {
            return new Digest(Digester.Digest(this.BytesToSign()));
        }
        /// <summary>
        /// Return transaction ID as string
        /// </summary>
        /// <returns></returns>
        public string TxID()
        {
            return Base32.EncodeToString(this.RawTxID().Bytes, false);
        }

        public void AssignGroupID(Digest gid)
        {
            this.group = gid;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || !(o is Transaction)) return false;
            Transaction that = o as Transaction;
            return type == that.type &&
                    sender.Equals(that.sender) &&
                    fee == that.fee &&
                    firstValid == that.firstValid &&
                    lastValid == that.lastValid &&
                    Enumerable.SequenceEqual(note, that.note) &&
                    genesisID.Equals(that.genesisID) &&
                    genesisHash.Equals(that.genesisHash) &&
                    Enumerable.SequenceEqual(lease, that.lease) &&
                    group.Equals(that.group) &&
                    amount == that.amount &&
                    receiver.Equals(that.receiver) &&
                    closeRemainderTo.Equals(that.closeRemainderTo) &&
                    votePK.Equals(that.votePK) &&
                    selectionPK.Equals(that.selectionPK) &&
                    voteFirst == that.voteFirst &&
                    voteLast == that.voteLast &&
                    voteKeyDilution == that.voteKeyDilution &&
                    assetParams.Equals(that.assetParams) &&
                    assetIndex == that.assetIndex &&
                    xferAsset == that.xferAsset &&
                    assetAmount == that.assetAmount &&
                    assetSender.Equals(that.assetSender) &&
                    assetReceiver.Equals(that.assetReceiver) &&
                    assetCloseTo.Equals(that.assetCloseTo) &&
                    freezeTarget.Equals(that.freezeTarget) &&
                    assetFreezeID == that.assetFreezeID &&
                    freezeState == that.freezeState;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public class AssetParams
        {
            /// <summary>
            /// total asset issuance
            /// </summary>
            [JsonProperty(PropertyName = "t")]
            [DefaultValue(0)]
            public ulong? assetTotal = 0;

            // Decimals specifies the number of digits to display after the decimal
            // place when displaying this asset. A value of 0 represents an asset
            // that is not divisible, a value of 1 represents an asset divisible
            // into tenths, and so on. This value must be between 0 and 19
            // (inclusive).
            [JsonProperty(PropertyName = "dc")]
            [DefaultValue(0)]
            public int assetDecimals = 0;

            //// whether each account has their asset slot frozen for this asset by default
            [JsonProperty(PropertyName = "df")]
            [DefaultValue(false)]
            public bool assetDefaultFrozen = false;

            //// a hint to the unit name of the asset
            [JsonProperty(PropertyName = "un")]
            [DefaultValue("")]
            public string assetUnitName = "";

            //// the name of the asset
            [JsonProperty(PropertyName = "an")]
            [DefaultValue("")]
            public String assetName = "";

            //// URL where more information about the asset can be retrieved
            [JsonProperty(PropertyName = "au")]
            [DefaultValue("")]
            public String url = "";

            //// MetadataHash specifies a commitment to some unspecified asset
            //// metadata. The format of this metadata is up to the application.
            [JsonProperty(PropertyName = "am")]
            public byte[] metadataHash;

            //// the address which has the ability to reconfigure the asset
            [JsonProperty(PropertyName = "m")]
            public Address assetManager = new Address();

            //// the asset reserve: assets owned by this address do not count against circulation
            [JsonProperty(PropertyName = "r")]
            public Address assetReserve = new Address();

            //// the address which has the ability to freeze/unfreeze accounts holding this asset
            [JsonProperty(PropertyName = "f")]
            public Address assetFreeze = new Address();

            //// the address which has the ability to issue clawbacks against asset-holding accounts
            [JsonProperty(PropertyName = "c")]
            public Address assetClawback = new Address();

            public AssetParams(
                   ulong? assetTotal,
                   int assetDecimals,
                   bool defaultFrozen,
                   string assetUnitName,
                   string assetName,
                   string url,
                   byte[] metadataHash,
                   Address manager,
                   Address reserve,
                   Address freeze,
                   Address clawback)
            {
                if (assetTotal != null) this.assetTotal = assetTotal;
                this.assetDecimals = assetDecimals;
                this.assetDefaultFrozen = defaultFrozen;
                if (assetUnitName != null) this.assetUnitName = assetUnitName;
                if (assetName != null) this.assetName = assetName;
                if (url != null) this.url = url;
                if (metadataHash != null) this.metadataHash = metadataHash;
                if (manager != null) this.assetManager = manager;
                if (reserve != null) this.assetReserve = reserve;
                if (freeze != null) this.assetFreeze = freeze;
                if (clawback != null) this.assetClawback = clawback;
            }

            public AssetParams() { }

            public override bool Equals(object o)
            {
                if (this == o) return true;
                if (o == null || !(o is AssetParams)) return false;
                AssetParams that = o as AssetParams;
                return assetTotal == that.assetTotal &&
                    (assetDefaultFrozen == that.assetDefaultFrozen) &&
                    assetName == that.assetName &&
                    assetUnitName == that.assetUnitName &&
                    url == that.url &&
                    ((metadataHash is null && that.metadataHash is null) || Enumerable.SequenceEqual(metadataHash, that.metadataHash)) &&
                    assetManager.Equals(that.assetManager) &&
                    assetReserve.Equals(that.assetReserve) &&
                    assetFreeze.Equals(that.assetFreeze) &&
                    assetClawback.Equals(that.assetClawback);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            [JsonConstructor]
            private AssetParams([JsonProperty(PropertyName = "t")] ulong? assetTotal,
                [JsonProperty(PropertyName = "dc")] int assetDecimals,
                [JsonProperty(PropertyName = "df")] bool assetDefaultFrozen,
                [JsonProperty(PropertyName = "un")] string assetUnitName,
                [JsonProperty(PropertyName = "an")] string assetName,
                [JsonProperty(PropertyName = "au")] string url,
                [JsonProperty(PropertyName = "am")] byte[] metadataHash,
                [JsonProperty(PropertyName = "m")] byte[] assetManager,
                [JsonProperty(PropertyName = "r")] byte[] assetReserve,
                [JsonProperty(PropertyName = "f")] byte[] assetFreeze,
                [JsonProperty(PropertyName = "c")] byte[] assetClawback) :
                this(assetTotal, assetDecimals, assetDefaultFrozen, assetUnitName, assetName, url, metadataHash,
                    new Address(assetManager), new Address(assetReserve), new Address(assetFreeze), new Address(assetClawback))
            { }
        }
    }    
    /// 
    /// A serializable convenience type for packaging transactions with their signatures.
    /// 
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class SignedTransaction
    {
        [JsonProperty(PropertyName = "txn")]
        public Transaction tx = new Transaction();

        [JsonProperty(PropertyName = "sig")]
        public Signature sig = new Signature(); // can be null

        [JsonProperty(PropertyName = "msig")]
        public MultisigSignature mSig = new MultisigSignature();

        [JsonProperty(PropertyName = "lsig")]
        public LogicsigSignature lSig = new LogicsigSignature();

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
            this(tx, sig, new MultisigSignature(), new LogicsigSignature(), txId)
        { }

        public SignedTransaction(Transaction tx, MultisigSignature mSig, string txId) :
            this(tx, new Signature(), mSig, new LogicsigSignature(), txId)
        { }

        public SignedTransaction(Transaction tx, LogicsigSignature lSig, string txId) :
            this(tx, new Signature(), new MultisigSignature(), lSig, txId)
        { }

        public SignedTransaction() { }

        [JsonConstructor]
        public SignedTransaction(Transaction tx, byte[] sig, MultisigSignature mSig, LogicsigSignature lSig)
        {
            if (tx != null) this.tx = tx;
            if (sig != null) this.sig = new Signature(sig);
            if (mSig != null) this.mSig = mSig;
            if (lSig != null) this.lSig = lSig;
            // don't recover the txid yet
        }

        public override bool Equals(object obj)
        {
            if (obj is SignedTransaction actual)
            {
                if (!tx.Equals(actual.tx)) return false;
                if (!sig.Equals(actual.sig)) return false;
                if (!lSig.Equals(actual.lSig)) return false;
                return this.mSig.Equals(actual.mSig);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
