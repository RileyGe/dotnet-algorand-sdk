using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace Algorand
{
    //@JsonPropertyOrder(alphabetic= true)
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
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
        //@JsonProperty("type")
        [JsonProperty(PropertyName = "type")]
        [DefaultValue("")]
        public Type type = Type.Default;

        // Instead of embedding POJOs and using JsonUnwrapped, we explicitly export inner fields. This circumvents our encoders'
        // inability to sort child fields.
        /* header fields ***********************************************************/
        //@JsonProperty("snd")
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

        /**
         * Create a payment transaction
         * @param fromAddr source address
         * @param toAddr destination address
         * @param fee transaction fee
         * @param amount payment amount
         * @param firstRound first valid round
         * @param lastRound last valid round
         */
        public Transaction(Address fromAddr, Address toAddr, ulong? fee, ulong? amount, ulong? firstRound,
            ulong? lastRound) : this(fromAddr, fee, firstRound, lastRound, null, amount, toAddr, "", new Digest()) { }

        public Transaction(Address fromAddr, Address toAddr, ulong? fee, ulong? amount, ulong? firstRound, ulong? lastRound,
                           string genesisID, Digest genesisHash) : this(fromAddr, fee, firstRound, lastRound, null, amount,
                               toAddr, genesisID, genesisHash) { }

        /**
         * Create a payment transaction. Make sure to sign with a suggested fee.
         * @param fromAddr source address
         * @param toAddr destination address
         * @param amount amount to send
         * @param firstRound first valid round
         * @param lastRound last valid round
         * @param genesisID genesis id
         * @param genesisHash genesis hash
         */
        public Transaction(Address fromAddr, Address toAddr, ulong? amount, ulong? firstRound, ulong? lastRound,
            string genesisID, Digest genesisHash) : this(fromAddr, 0, firstRound, lastRound, null, amount, toAddr, genesisID, genesisHash) { }

        public Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note, ulong? amount,
            Address receiver, string genesisID, Digest genesisHash) : this(sender, fee, firstValid, lastValid, note,
                genesisID, genesisHash, amount, receiver, new Address()) { }

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

        /**
         * Create a key registration transaction. No field can be null except the note field.
         * @param sender source address
         * @param fee transaction fee
         * @param firstValid first valid round
         * @param lastValid last valid round
         * @param note optional notes field (can be null)
         * @param votePK the new participation key to register
         * @param vrfPK the sortition key to register
         * @param voteFirst key reg valid first round
         * @param voteLast key reg valid last round
         * @param voteKeyDilution key reg dilution
         */
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

        /**
         * Create an asset creation transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
         * @param sender source address
         * @param fee transaction fee
         * @param firstValid first valid round
         * @param lastValid last valid round
         * @param note optional note field (can be null)
         * @param genesisID
         * @param genesisHash
         * @param assetTotal total asset issuance
         * @param defaultFrozen whether accounts have this asset frozen by default
         * @param assetUnitName name of unit of the asset
         * @param assetName name of the asset
         * @param url where more information about the asset can be retrieved
         * @param metadataHash specifies a commitment to some unspecified asset metadata. The format of this metadata is up to the application
         * @param manager account which can reconfigure the asset
         * @param reserve account whose asset holdings count as non-minted
         * @param freeze account which can freeze or unfreeze holder accounts
         * @param clawback account which can issue clawbacks against holder accounts
         */
        private Transaction(Address sender, ulong? fee, ulong? firstValid, ulong? lastValid, byte[] note,
                           String genesisID, Digest genesisHash, ulong? assetTotal, bool defaultFrozen,
                           String assetUnitName, String assetName, String url, byte[] metadataHash,
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

            this.assetParams = new AssetParams(assetTotal, defaultFrozen, assetUnitName, assetName, url, metadataHash, manager, reserve, freeze, clawback);
        }

        //    /**
        //     * Create an asset creation transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        //     * @param sender source address
        //     * @param fee transaction fee
        //     * @param firstValid first valid round
        //     * @param lastValid last valid round
        //     * @param note optional note field (can be null)
        //     * @param genesisID
        //     * @param genesisHash
        //     * @param assetTotal total asset issuance
        //     * @param defaultFrozen whether accounts have this asset frozen by default
        //     * @param assetUnitName name of unit of the asset
        //     * @param assetName name of the asset
        //     * @param url where more information about the asset can be retrieved
        //     * @param metadataHash specifies a commitment to some unspecified asset metadata. The format of this metadata is up to the application
        //     * @param manager account which can reconfigure the asset
        //     * @param reserve account whose asset holdings count as non-minted
        //     * @param freeze account which can freeze or unfreeze holder accounts
        //     * @param clawback account which can issue clawbacks against holder accounts
        //     */
        //    public static Transaction createAssetCreateTransaction(Address sender, BigInteger fee, BigInteger firstValid, BigInteger lastValid, byte[] note,
        //                       String genesisID, Digest genesisHash, BigInteger assetTotal, boolean defaultFrozen,
        //                       String assetUnitName, String assetName, String url, byte[] metadataHash,
        //                       Address manager, Address reserve, Address freeze, Address clawback)
        //    {
        //        return new Transaction(
        //                sender, fee, firstValid, lastValid, note,
        //                genesisID, genesisHash, assetTotal, defaultFrozen,
        //                assetUnitName, assetName, url, metadataHash,
        //                manager, reserve, freeze, clawback);
        //    }

        //    /**
        //     * Create an asset configuration transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        //     * @param sender source address
        //     * @param fee transaction fee
        //     * @param firstValid first valid round
        //     * @param lastValid last valid round
        //     * @param note optional note field (can be null)
        //     * @param genesisID
        //     * @param genesisHash
        //     * @param index asset index
        //     * @param manager account which can reconfigure the asset
        //     * @param reserve account whose asset holdings count as non-minted
        //     * @param freeze account which can freeze or unfreeze holder accounts
        //     * @param clawback account which can issue clawbacks against holder accounts
        //     */
        //    private Transaction(Address sender, BigInteger fee, BigInteger firstValid, BigInteger lastValid, byte[] note,
        //                       String genesisID, Digest genesisHash, BigInteger index,
        //                    Address manager, Address reserve, Address freeze, Address clawback)
        //    {

        //        this.type = Type.AssetConfig;
        //        if (sender != null) this.sender = sender;
        //        if (fee != null) this.fee = fee;
        //        if (firstValid != null) this.firstValid = firstValid;
        //        if (lastValid != null) this.lastValid = lastValid;
        //        if (note != null) this.note = note;
        //        if (genesisID != null) this.genesisID = genesisID;
        //        if (genesisHash != null) this.genesisHash = genesisHash;
        //        this.assetParams = new AssetParams(BigInteger.valueOf(0), false, "", "", "", null, manager, reserve, freeze, clawback);
        //        assetIndex = index;
        //    }

        //    /**
        //     * Create an asset configuration transaction. Note can be null. manager, reserve, freeze, and clawback can be zeroed.
        //     * @param sender source address
        //     * @param fee transaction fee
        //     * @param firstValid first valid round
        //     * @param lastValid last valid round
        //     * @param note optional note field (can be null)
        //     * @param genesisID
        //     * @param genesisHash
        //     * @param index asset index
        //     * @param manager account which can reconfigure the asset
        //     * @param reserve account whose asset holdings count as non-minted
        //     * @param freeze account which can freeze or unfreeze holder accounts
        //     * @param clawback account which can issue clawbacks against holder accounts
        //     * @param strictEmptyAddressChecking if true, disallow empty admin accounts from being set (preventing accidental disable of admin features)
        //     */
        //    public static Transaction createAssetConfigureTransaction(
        //            Address sender,
        //            BigInteger fee,
        //            BigInteger firstValid,
        //            BigInteger lastValid,
        //            byte[] note,
        //            String genesisID,
        //            Digest genesisHash,
        //            BigInteger index,
        //            Address manager,
        //            Address reserve,
        //            Address freeze,
        //            Address clawback,
        //            boolean strictEmptyAddressChecking)
        //    {
        //        Address defaultAddr = new Address();
        //        if (strictEmptyAddressChecking && (
        //                (manager == null || manager.equals(defaultAddr)) ||
        //                (reserve == null || reserve.equals(defaultAddr)) ||
        //                (freeze == null || freeze.equals(defaultAddr)) ||
        //                (clawback == null || clawback.equals(defaultAddr))
        //                ))
        //        {
        //            throw new RuntimeException("strict empty address checking requested but "
        //                    + "empty or default address supplied to one or more manager addresses");
        //        }
        //        return new Transaction(
        //                sender,
        //                fee,
        //                firstValid,
        //                lastValid,
        //                note,
        //                genesisID,
        //                genesisHash,
        //                index,
        //                manager,
        //                reserve,
        //                freeze,
        //                clawback);
        //    }

        //    // workaround for nested JsonValue classes
        //@JsonCreator
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

        ///**
        // * Base constructor with flat fee for asset xfer/freeze/destroy transactions.
        // * @param flatFee is the transaction flat fee
        // * @param firstRound is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastRound is the last round this txn is valid
        // * @param note
        // * @param genesisID corresponds to the id of the network
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset index
        // **/
        //private Transaction(
        //        Type type,
        //        BigInteger flatFee,
        //        BigInteger firstRound,
        //        BigInteger lastRound,
        //        byte[] note,
        //        Digest genesisHash)
        //{

        //    this.type = type;
        //    if (flatFee != null) this.fee = flatFee;
        //    if (firstRound != null) this.firstValid = firstRound;
        //    if (lastRound != null) this.lastValid = lastRound;
        //    if (note != null) this.note = note;
        //    if (genesisHash != null) this.genesisHash = genesisHash;
        //}

        ///**
        // * Creates a tx to mark the account as willing to accept the asset.
        // * @param acceptingAccount is a checksummed, human-readable address that
        // * will accept receiving the asset.
        // * @param flatFee is the transaction flat fee
        // * @param firstRound is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastRound is the last round this txn is valid
        // * @param note
        // * @param genesisID corresponds to the id of the network
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset index
        // **/
        //public static Transaction createAssetAcceptTransaction( //AssetTransaction
        //        Address acceptingAccount,
        //        BigInteger flatFee,
        //        BigInteger firstRound,
        //        BigInteger lastRound,
        //        byte[] note,
        //        String genesisID,
        //        Digest genesisHash,
        //        BigInteger assetIndex)
        //{

        //    Transaction tx = createAssetTransferTransaction(
        //            acceptingAccount,
        //            acceptingAccount,
        //            new Address(),
        //            BigInteger.valueOf(0),
        //            flatFee,
        //            firstRound,
        //            lastRound,
        //            note,
        //            genesisID,
        //            genesisHash,
        //            assetIndex);

        //    return tx;
        //}

        ///**
        // * Creates a tx to destroy the asset
        // * @param senderAccount is a checksummed, human-readable address of the sender 
        // * @param flatFee is the transaction flat fee
        // * @param firstValid is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastValid is the last round this txn is valid
        // * @param note
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset ID to destroy
        // **/
        //public static Transaction createAssetDestroyTransaction(
        //        Address senderAccount,
        //        BigInteger flatFee,
        //        BigInteger firstValid,
        //        BigInteger lastValid,
        //        byte[] note,
        //        Digest genesisHash,
        //        BigInteger assetIndex)
        //{
        //    Transaction tx = new Transaction(
        //            Type.AssetConfig,
        //            flatFee,
        //            firstValid,
        //            lastValid,
        //            note,
        //            genesisHash);

        //    if (assetIndex != null) tx.assetIndex = assetIndex;
        //    if (senderAccount != null) tx.sender = senderAccount;
        //    return tx;
        //}

        ///**
        // * Creates a tx to freeze/unfreeze assets
        // * @param senderAccount is a checksummed, human-readable address of the sender 
        // * @param flatFee is the transaction flat fee
        // * @param firstValid is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastValid is the last round this txn is valid
        // * @param note
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset ID to destroy
        // **/
        //public static Transaction createAssetFreezeTransaction(
        //        Address senderAccount,
        //        Address accountToFreeze,
        //        boolean freezeState,
        //        BigInteger flatFee,
        //        BigInteger firstValid,
        //        BigInteger lastValid,
        //        byte[] note,
        //        Digest genesisHash,
        //        BigInteger assetIndex)
        //{
        //    Transaction tx = new Transaction(
        //            Type.AssetFreeze,
        //            flatFee,
        //            firstValid,
        //            lastValid,
        //            note,
        //            genesisHash);

        //    if (senderAccount != null) tx.sender = senderAccount;
        //    if (accountToFreeze != null) tx.freezeTarget = accountToFreeze;
        //    if (assetIndex != null) tx.assetFreezeID = assetIndex;
        //    tx.freezeState = freezeState;
        //    return tx;
        //}

        ///**
        // * Creates a tx for revoking an asset from an account and sending it to another
        // * @param transactionSender is a checksummed, human-readable address that will
        // * send the transaction
        // * @param assetRevokedFrom is a checksummed, human-readable address that will
        // * have assets taken from
        // * @param assetReceiver is a checksummed, human-readable address what will
        // * receive the assets
        // * @param assetAmount is the number of assets to send
        // * @param flatFee is the transaction flat fee
        // * @param firstRound is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastRound is the last round this txn is valid
        // * @param note
        // * @param genesisID corresponds to the id of the network
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset index
        // **/
        //public static Transaction createAssetRevokeTransaction(// AssetTransaction
        //        Address transactionSender,
        //        Address assetRevokedFrom,
        //        Address assetReceiver,
        //        BigInteger assetAmount,
        //        BigInteger flatFee,
        //        BigInteger firstRound,
        //        BigInteger lastRound,
        //        byte[] note,
        //        String genesisID,
        //        Digest genesisHash,
        //        BigInteger assetIndex)
        //{

        //    Transaction tx = new Transaction(
        //            Type.AssetTransfer,
        //            flatFee,    // fee
        //            firstRound, // fv
        //            lastRound, // lv
        //            note, //note
        //            genesisHash); // gh

        //    tx.assetReceiver = assetReceiver; //arcv
        //    tx.assetSender = assetRevokedFrom; //asnd        
        //    tx.assetAmount = assetAmount; // aamt
        //    tx.sender = transactionSender; // snd
        //    if (assetIndex != null) tx.xferAsset = assetIndex;
        //    return tx;
        //}


        ///**
        // * Creates a tx for sending some asset from an asset holder to another user.
        // *  The asset receiver must have marked itself as willing to accept the
        // *  asset.
        // * @param assetSender is a checksummed, human-readable address that will
        // * send the transaction and assets
        // * @param assetReceiver is a checksummed, human-readable address what will
        // * receive the assets
        // * @param assetCloseTo is a checksummed, human-readable address that
        // * behaves as a close-to address for the asset transaction; the remaining
        // * assets not sent to assetReceiver will be sent to assetCloseTo. Leave
        // * blank for no close-to behavior.
        // * @param assetAmount is the number of assets to send
        // * @param flatFee is the transaction flat fee
        // * @param firstRound is the first round this txn is valid (txn semantics
        // * unrelated to asset management)
        // * @param lastRound is the last round this txn is valid
        // * @param note
        // * @param genesisID corresponds to the id of the network
        // * @param genesisHash corresponds to the base64-encoded hash of the genesis
        // * of the network
        // * @param assetIndex is the asset index
        // **/
        //public static Transaction createAssetTransferTransaction(// AssetTransaction
        //        Address assetSender,
        //        Address assetReceiver,
        //        Address assetCloseTo,
        //        BigInteger assetAmount,
        //        BigInteger flatFee,
        //        BigInteger firstRound,
        //        BigInteger lastRound,
        //        byte[] note,
        //        String genesisID,
        //        Digest genesisHash,
        //        BigInteger assetIndex)
        //{

        //    Transaction tx = new Transaction(
        //            Type.AssetTransfer,
        //            flatFee,    // fee
        //            firstRound, // fv
        //            lastRound, // lv
        //            note, //note
        //            genesisHash); // gh

        //    tx.assetReceiver = assetReceiver; //arcv
        //    tx.assetCloseTo = assetCloseTo; // aclose
        //    tx.assetAmount = assetAmount; // aamt
        //    tx.sender = assetSender; // snd
        //    if (assetIndex != null) tx.xferAsset = assetIndex;
        //    return tx;
        //}

        ///** Lease enforces mutual exclusion of transactions.  If this field
        // * is nonzero, then once the transaction is confirmed, it acquires
        // * the lease identified by the (Sender, Lease) pair of the
        // * transaction until the LastValid round passes.  While this
        // * transaction possesses the lease, no other transaction
        // * specifying this lease can be confirmed. 
        // * The Size is fixed at 32 bytes. 
        // * @param lease 32 byte lease
        // **/
        //public void setLease(byte[] lease)
        //{
        //    if (lease.length != LEASE_LENGTH && lease.length != 0)
        //    {
        //        throw new RuntimeException("The lease should be an empty array or a 32 byte array.");
        //    }
        //    this.lease = lease;
        //}

        ///**
        // * TxType represents a transaction type.
        // */
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

            //        private final String value;
            //        Type(String value)
            //{
            //    this.value = value;
            //}

            /**
             * Return the enumeration for the given string value. Required for JSON serialization.
             * @param value string representation
             * @return enumeration type
             */
            //@JsonCreator
            [JsonConstructor]
            public Type(string value)
            {
                this.type = namesMap.GetValueOrDefault(value, 0);
            }

            public Type(int value)
            {
                this.type = value;
            }

            ///**
            // * Return the string value for this enumeration. Required for JSON serialization.
            // * @return string value
            // */
            //@JsonValue
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

        //    /**
        //     * Return encoded representation of the transaction
        //     */
        //    public byte[] bytes() throws IOException
        //{
        //        try {
        //        return Encoder.encodeToMsgPack(this);
        //    } catch (IOException e) {
        //        throw new RuntimeException("serialization failed", e);
        //    }
        //}

        /**
         * Return encoded representation of the transaction with a prefix
         * suitable for signing
         */
        public byte[] BytesToSign()
        {
            //try
            //{
            byte[] encodedTx = Encoder.EncodeToMsgPack(this);
            var retList = new List<byte>();
            retList.AddRange(TX_SIGN_PREFIX);
            retList.AddRange(encodedTx);
            return retList.ToArray();
            //var encodedStr = Encoder.EncodeToJson(this);
            //byte[] prefixEncodedTx = new byte[encodedTx.Length + TX_SIGN_PREFIX.Length];
            //JavaHelper<byte>.SyatemArrayCopy(TX_SIGN_PREFIX, 0, prefixEncodedTx, 0, TX_SIGN_PREFIX.Length);
            //JavaHelper<byte>.SyatemArrayCopy(encodedTx, 0, prefixEncodedTx, TX_SIGN_PREFIX.Length, encodedTx.Length);
            //return Encoding.UTF8.GetBytes(TX_SIGN_PREFIX + encodedStr);
            //}
            //catch (IOException e)
            //{
            //    throw new RuntimeException("serialization failed", e);
            //}
        }

        /**
         * Return transaction ID as Digest
         */
        public Digest RawTxID()
        {
            //try {
            return new Digest(Digester.Digest(this.BytesToSign()));
            //            } catch (IOException e) {
            //                throw new RuntimeException("tx computation failed", e);
            //    } catch (NoSuchAlgorithmException e) {
            //                throw new RuntimeException("tx computation failed", e);
            //}
        }

        /**
         * Return transaction ID as string
         */
        public string TxID(){
                return Base32.ToBase32String(this.RawTxID().Bytes, false);
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
        //}
        //@JsonPropertyOrder(alphabetic= true)
        //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
        public class AssetParams
        {
            //    @JsonProperty("t")
            ///
            /// <summary>
            /// total asset issuance
            /// </summary>
            [JsonProperty(PropertyName = "t")]
            [DefaultValue(0)]
            public ulong? assetTotal = 0;

            //// whether each account has their asset slot frozen for this asset by default
            //@JsonProperty("df")
            [JsonProperty(PropertyName = "df")]
            [DefaultValue(false)]
            public bool assetDefaultFrozen = false;

            //// a hint to the unit name of the asset
            //@JsonProperty("un")
            [JsonProperty(PropertyName = "un")]
            [DefaultValue("")]
            public string assetUnitName = "";

            //// the name of the asset
            //@JsonProperty("an")
            [JsonProperty(PropertyName = "an")]
            [DefaultValue("")]
            public String assetName = "";

            //// URL where more information about the asset can be retrieved
            //@JsonProperty("au")
            [JsonProperty(PropertyName = "au")]
            [DefaultValue("")]
            public String url = "";

            //// MetadataHash specifies a commitment to some unspecified asset
            //// metadata. The format of this metadata is up to the application.
            //@JsonProperty("am")
            [JsonProperty(PropertyName = "am")]
            public byte[] metadataHash;

            //// the address which has the ability to reconfigure the asset
            //@JsonProperty("m")
            [JsonProperty(PropertyName = "m")]
            public Address assetManager = new Address();

            //// the asset reserve: assets owned by this address do not count against circulation
            //@JsonProperty("r")
            [JsonProperty(PropertyName = "r")]
            public Address assetReserve = new Address();

            //// the address which has the ability to freeze/unfreeze accounts holding this asset
            //@JsonProperty("f")
            [JsonProperty(PropertyName = "f")]
            public Address assetFreeze = new Address();

            //// the address which has the ability to issue clawbacks against asset-holding accounts
            //@JsonProperty("c")
            [JsonProperty(PropertyName = "c")]
            public Address assetClawback = new Address();

            public AssetParams(
                   ulong? assetTotal,
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

            //@JsonCreator
            [JsonConstructor]
            private AssetParams([JsonProperty(PropertyName = "t")] ulong? assetTotal,
                [JsonProperty(PropertyName = "df")] bool assetDefaultFrozen,
                [JsonProperty(PropertyName = "un")] string assetUnitName,
                [JsonProperty(PropertyName = "an")] string assetName,
                [JsonProperty(PropertyName = "au")] string url,
                [JsonProperty(PropertyName = "am")] byte[] metadataHash,
                [JsonProperty(PropertyName = "m")] byte[] assetManager,
                [JsonProperty(PropertyName = "r")] byte[] assetReserve,
                [JsonProperty(PropertyName = "f")] byte[] assetFreeze,
                [JsonProperty(PropertyName = "c")] byte[] assetClawback) : 
                this(assetTotal, assetDefaultFrozen, assetUnitName, assetName, url, metadataHash, 
                    new Address(assetManager), new Address(assetReserve), new Address(assetFreeze), new Address(assetClawback))
            {

            }

        //        /**
        //         * Convert the given object to string with each line indented by 4 spaces
        //         * (except the first line).
        //         */
        //        private String toIndentedString(java.lang.Object o)
        //{
        //    if (o == null)
        //    {
        //        return "null";
        //    }
        //    return o.toString().replace("\n", "\n    ");}
    }
    }
}
