using Algorand;
using Algorand.Kmd.Api;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using TechTalk.SpecFlow;
using Encoder = Algorand.Encoder;

namespace test
{
    [Binding]
    public class Stepdefs
    {

        //    public static string token = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        //    public static int algodPort = 60000;
        //    public static int kmdPort = 60001;

        Algorand.Algod.Model.TransactionParams transParams;
        SignedTransaction stx;
        //    SignedTransaction[] stxs;
        //    byte[] stxBytes;
        Transaction txn;
        //    TransactionBuilder txnBuilder;
        //    string txid;
        Account account;
        Address pk;
        string address;
        //    byte[] sk;
        ulong fee;
        ulong fv;
        ulong lv;
        Digest gh;
        Address to;
        Address close;
        ulong amt;
        string gen;
        byte[] note;
        MultisigAddress msig;
        //    MultisigSignature msigsig;
        //    string walletName;
        //    string walletPswd;
        //    string walletID;
        Algorand.Algod.Api.AlgodApi acl;
        //    AlgodClient algodClient;
        //KmdApi kcl;
        //    KmdClient kmdClient;
        //    com.algorand.algosdk.v2.client.common.AlgodClient aclv2;
        //    string handle;
        List<string> versions;
        //    Algorand.Algod.Model.NodeStatus status;
        //    Algorand.Algod.Model.NodeStatus statusAfter;
        //    List<byte[]> pks;
        List<string> addresses;
        ulong? lastRound;
        //    bool err;
        //    ulong microalgos;
        //    string mnemonic;
        //    byte[] mdk;
        //    string oldAddr;
        //    Bid bid;
        //    SignedBid oldBid;
        //    SignedBid sbid;
        //    ulong paramsFee;
        ParticipationPublicKey votepk;
        VRFPublicKey vrfpk;
        ulong votefst;
        ulong votelst;
        ulong votekd;
        //    string num;

        //    /* Assets */
        //    string creator = "";
        //    ulong assetID = 1);
        //    string assetName = "testcoin";
        //    string assetUnitName = "coins";
        //    com.algorand.algosdk.transaction.AssetParams expectedParams = null;
        //    AssetParams queriedParams = new AssetParams();

        //    /* Compile / Dryrun */
        //    Response<CompileResponse> compileResponse;
        //    Response<DryrunResponse> dryrunResponse;

        protected Address getAddress(int i)
        {
            if (addresses == null)
            {
                throw new ArgumentException("Addresses not initialized, must use given 'wallet information'");
            }
            if (addresses.Count < i || addresses.Count == 0)
            {
                throw new ArgumentException("Not enough addresses, you may need to update the network template.");
            }
            try
            {
                return new Address(addresses[i]);
            }
            catch (Exception e)
            {
                // Lets not bother recovering from this one 🔥
                throw new ArgumentException(e.Message);
            }
        }

        /// <summary>
        /// Convenience method to prepare the parameter object.
        /// </summary>
        protected void getParams()
        {
            try
            {
                transParams = acl.TransactionParams();
                lastRound = transParams.LastRound;

            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        ///// <summary>
        ///// Convenience method to lookup a secret key and sign a transaction with the key.
        ///// </summary>
        ///// <param name="tx"></param>
        ///// <param name="addr"></param>
        ///// <returns></returns>
        //public SignedTransaction signWithAddress(Transaction tx, Address addr){
        //    Algorand.Kmd.Model.ExportKeyRequest req = new Algorand.Kmd.Model.ExportKeyRequest();
        //    req.setAddress(addr.ToString());
        //    req.setWalletHandleToken(handle);
        //    req.setWalletPassword(walletPswd);
        //    byte[] secretKey = kcl.exportKey(req).getPrivateKey();
        //    Account acct = new Account(Arrays.copyOfRange(secretKey, 0, 32));
        //    return acct.signTransaction(tx);
        //}

        ///**
        // * Convenience method to export a key and initialize an account to use for signing.
        // */
        //public void exportKeyAndSetAccount(Address addr) throws com.algorand.algosdk.kmd.client.ApiException, NoSuchAlgorithmException {
        //    ExportKeyRequest req = new ExportKeyRequest();
        //    req.setAddress(addr.ToString());
        //    req.setWalletHandleToken(handle);
        //    req.setWalletPassword(walletPswd);
        //    sk = kcl.exportKey(req).getPrivateKey();
        //    account = new Account(Arrays.copyOfRange(sk, 0, 32));
        //}

        //    [When("I create a wallet")]
        //public void createWallet()
        //    {
        //        walletName = "Walletjava";
        //        walletPswd = "";
        //        CreateWalletRequest req = new CreateWalletRequest();
        //    req.setWalletName(walletName);
        //    req.setWalletPassword(walletPswd);
        //    req.setWalletDriverName("sqlite");
        //    walletID = kcl.createWallet(req).getWallet().getId();
        //}

        //    [Then]("the wallet should exist")
        //    public void walletExist() throws com.algorand.algosdk.kmd.client.ApiException
        //    {
        //        bool exists = false;
        //        APIV1GETWalletsResponse resp = kcl.listWallets();
        //        for (APIV1Wallet w : resp.getWallets()){
        //            if (w.getName().Equals(walletName))
        //            {
        //                exists = true;
        //            }
        //        }
        //        Assert.AreEqual(exists).isTrue();
        //    }

        //    [When]("I get the wallet handle")
        //    public void getHandle() throws com.algorand.algosdk.kmd.client.ApiException
        //    {
        //        InitWalletHandleTokenRequest req = new InitWalletHandleTokenRequest();
        //    req.setWalletId(walletID);
        //        req.setWalletPassword(walletPswd);
        //        handle = kcl.initWalletHandleToken(req).getWalletHandleToken();

        //}

        //[Then]("I can get the master derivation key")
        //    public void getMdk() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    ExportMasterKeyRequest req = new ExportMasterKeyRequest();
        //req.setWalletHandleToken(handle);
        //req.setWalletPassword(walletPswd);
        //byte[] mdk = kcl.exportMasterKey(req).getMasterDerivationKey();
        //Assert.AreEqual(mdk.length).isGreaterThan(0);
        //    }

        //    [When]("I rename the wallet")
        //    public void renameWallet() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    RenameWalletRequest req = new RenameWalletRequest();
        //walletName = "Walletjava_new";
        //req.setWalletId(walletID);
        //req.setWalletPassword(walletPswd);
        //req.setWalletName(walletName);
        //kcl.renameWallet(req);
        //    }

        //    [Then]("I can still get the wallet information with the same handle")
        //    public void getWalletInfo() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    WalletInfoRequest req = new WalletInfoRequest();
        //req.setWalletHandleToken(handle);
        //string name = kcl.getWalletInfo(req).getWalletHandle().getWallet().getName();
        //Assert.AreEqual(name, walletName);
        //    }

        //    [When]("I renew the wallet handle")
        //    public void renewHandle() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    RenewWalletHandleTokenRequest req = new RenewWalletHandleTokenRequest();
        //req.setWalletHandleToken(handle);
        //kcl.renewWalletHandleToken(req);
        //    }

        //    [When]("I release the wallet handle")
        //    public void releaseHandle() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    ReleaseWalletHandleTokenRequest req = new ReleaseWalletHandleTokenRequest();
        //req.setWalletHandleToken(handle);
        //kcl.releaseWalletHandleToken(req);
        //    }

        //    [Then]("the wallet handle should not work")
        //    public void tryHandle() throws com.algorand.algosdk.kmd.client.ApiException
        //{
        //    RenewWalletHandleTokenRequest req = new RenewWalletHandleTokenRequest();
        //req.setWalletHandleToken(handle);
        //err = false;
        //try
        //{
        //    kcl.renewWalletHandleToken(req);
        //}
        //catch (Exception e)
        //{
        //    err = true;
        //}
        //Assert.AreEqual(err).isTrue();
        //    }

        [Given(@"payment transaction parameters (\d+) (\d+) (\d+) (.*) (.*) (.*) (\d+) (.*) (.*)")]
        public void transactionParameters(ulong fee, ulong fv, ulong lv, string gh, string to,
            string close, ulong amt, string gen, string note)
        {
            this.fee = fee;
            this.fv = fv;
            this.lv = lv;
            this.gh = new Digest(gh);
            this.to = new Address(to);
            if (!close.Equals("none"))
            {
                this.close = new Address(close);
            }
            this.amt = amt;
            if (!gen.Equals("none"))
            {
                this.gen = gen;
            }
            if (!note.Equals("none"))
            {
                this.note = Convert.FromBase64String(note);
            }
        }

        [Given(@"key registration transaction parameters (\d+) (\d+) (\d+) (.*) (.*) (.*) (\d+) (\d+) (\d+) (.*) (.*)")]
        public void keyregTxnParameters(ulong fee, ulong fv, ulong lv, string gh, string votepk,
            string vrfpk, ulong votefst, ulong votelst, ulong votekd, string gen, string note)
        {
            this.fee = fee;
            this.fv = fv;
            this.lv = lv;
            this.gh = new Digest(gh);
            this.votepk = new ParticipationPublicKey(Convert.FromBase64String(votepk));
            this.vrfpk = new VRFPublicKey(Convert.FromBase64String(vrfpk));
            this.votefst = votefst;
            this.votelst = votelst;
            this.votekd = votekd;
            if (!gen.Equals("none"))
            {
                this.gen = gen;
            }
            if (!note.Equals("none"))
            {
                this.note = Convert.FromBase64String(note);
            }
        }

        [Given("mnemonic for private key (.*)")]
        public void mn_for_sk(string mn)
        {
            account = new Account(mn);
            pk = account.Address;
        }

        [When("I create the payment transaction")]
        public void createPaytxn()
        {
            txn = new Transaction(pk, fee, fv, lv, note, amt, to, gen, gh)
            {
                closeRemainderTo = close,
            };
        }

        [When("I create the key registration transaction")]
        public void createKeyregTxn()
        {
            txn = Transaction.CreateKeyRegistrationTransaction(pk, fee, fv, lv, note, gen, gh,
                votepk, vrfpk, votefst, votelst, votekd);
            Account.SetFeeByFeePerByte(txn, fee);
        }

        [Given("multisig addresses (.*)")]
        public void msig_addresses(string addresses)
        {
            string[] addrs = addresses.Split(" ");
            List<Ed25519PublicKeyParameters> addrlist = new List<Ed25519PublicKeyParameters>();
            foreach (var item in addrs)
            {
                addrlist.Add(new Ed25519PublicKeyParameters(new Address(item).Bytes, 0));
            }
            msig = new MultisigAddress(1, 2, addrlist);
            pk = new Address(msig.ToString());
        }

        [When("I create the multisig payment transaction")]
        public void createMsigTxn()
        {
            txn = new Transaction(new Address(msig.ToString()), fee, fv, lv, note, amt, to, gen, gh)
            { closeRemainderTo = close };

            Account.SetFeeByFeePerByte(txn, fee);
        }

        [When("I sign the multisig transaction with the private key")]
        public void signMsigTxn()
        {
            stx = account.SignMultisigTransaction(msig, txn);
        }

        [When("I sign the transaction with the private key")]
        public void signTxn()
        {
            stx = account.SignTransaction(txn);
        }

        [Then("the signed transaction should equal the golden (.*)")]
        public void equalGolden(string golden)
        {
            byte[] signedTxBytes = Encoder.EncodeToMsgPack(stx);
            Assert.AreEqual(Convert.ToBase64String(signedTxBytes), golden);
        }

        [Then("the multisig transaction should equal the golden (.*)")]
        public void equalMsigGolden(string golden)
        {
            byte[]
        signedTxBytes = Encoder.EncodeToMsgPack(stx);
            Assert.AreEqual(Convert.ToBase64String(signedTxBytes), golden);
        }


        [Then("the multisig address should equal the golden (.*)")]
        public void equalMsigAddrGolden(string golden)
        {
            Assert.AreEqual(msig.ToString(), golden);
        }

        [When("I get versions with algod")]
        public void aclV()
        {
            versions = acl.GetVersion().Versions;
        }

        [Then("v1 should be in the versions")] 
            public void v1InVersions()
        {
            CollectionAssert.Contains(versions, "v1");
        }

        //[When("I get versions with kmd")]
        //public void kclV()
        //{
        //    versions = kcl.GetVersion().Versions;
        //}

    //[When]("I get the status")
    //    public void status() throws ApiException
    //{
    //    status = acl.getStatus();
    //}

    //[When]("I get status after this block")
    //    public void statusBlock() throws ApiException, InterruptedException {
    //        Thread.sleep(4000);
    //statusAfter = acl.waitForBlock(status.LastRound);
    //    }

    //    [Then]("I can get the block info")
    //    public void block() throws ApiException
    //{
    //    acl.getBlock(status.LastRound.add(1)));
    //    }

    //    [When]("I import the multisig")
    //    public void importMsig() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ImportMultisigRequest req = new ImportMultisigRequest();
    //req.setMultisigVersion(msig.version);
    //req.setThreshold(msig.threshold);
    //req.setWalletHandleToken(handle);
    //req.setPks(msig.publicKeys);
    //kcl.importMultisig(req);
    //    }

    //    [Then]("the multisig should be in the wallet")
    //    public void msigInWallet() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ListMultisigRequest req = new ListMultisigRequest();
    //req.setWalletHandleToken(handle);
    //List<String> msigs = kcl.listMultisig(req).getAddresses();
    //bool exists = false;
    //for (string m : msigs)
    //{
    //    if (m.Equals(msig.ToString()))
    //    {
    //        exists = true;
    //    }
    //}
    //Assert.AreEqual(exists).isTrue();
    //    }

    //    [When]("I export the multisig")
    //    public void expMsig() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ExportMultisigRequest req = new ExportMultisigRequest();
    //req.setAddress(msig.ToString());
    //req.setWalletHandleToken(handle);
    //pks = kcl.exportMultisig(req).getPks();

    //    }

    //    [Then]("the multisig should equal the exported multisig")
    //    public void msigEq()
    //{
    //    bool eq = true;
    //    for (int x = 0; x < msig.publicKeys.Count; x++)
    //    {
    //        if (!Convert.ToBase64String(msig.publicKeys.get(x).getBytes()).Equals(Convert.ToBase64String(pks.get(x))))
    //        {
    //            eq = false;
    //        }
    //    }
    //    Assert.AreEqual(eq).isTrue();
    //}
    //[When]("I delete the multisig")
    //    public void deleteMsig() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    DeleteMultisigRequest req = new DeleteMultisigRequest();
    //req.setAddress(msig.ToString());
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //kcl.deleteMultisig(req);
    //    }

    //    [Then]("the multisig should not be in the wallet")
    //    public void msigNotInWallet()throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ListMultisigRequest req = new ListMultisigRequest();
    //req.setWalletHandleToken(handle);
    //List<String> msigs = kcl.listMultisig(req).getAddresses();
    //bool exists = false;
    //if (msigs != null)
    //{
    //    for (string m : msigs)
    //    {
    //        if (m.Equals(msig.ToString()))
    //        {
    //            exists = true;
    //        }
    //    }
    //}
    //Assert.AreEqual(exists).isFalse();
    //    }

    //    [When]("I generate a key using kmd")
    //    public void genKeyKmd() throws com.algorand.algosdk.kmd.client.ApiException, NoSuchAlgorithmException{
    //    GenerateKeyRequest req = new GenerateKeyRequest();
    //    req.setDisplayMnemonic(false);
    //    req.setWalletHandleToken(handle);
    //    pk = new Address(kcl.generateKey(req).Address);
    //}

    //[Then]("the key should be in the wallet")
    //    public void keyInWallet() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ListKeysRequest req = new ListKeysRequest();
    //req.setWalletHandleToken(handle);
    //List<String> keys = kcl.listKeysInWallet(req).getAddresses();
    //bool exists = false;
    //for (string k : keys)
    //{
    //    if (k.Equals(pk.ToString()))
    //    {
    //        exists = true;
    //    }
    //}
    //Assert.AreEqual(exists).isTrue();
    //    }

    //    [When]("I delete the key")
    //    public void deleteKey() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    DeleteKeyRequest req = new DeleteKeyRequest();
    //req.setAddress(pk.ToString());
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //kcl.deleteKey(req);
    //    }

    //    [Then]("the key should not be in the wallet")
    //    public void keyNotInWallet() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ListKeysRequest req = new ListKeysRequest();
    //req.setWalletHandleToken(handle);
    //List<String> keys = kcl.listKeysInWallet(req).getAddresses();
    //bool exists = false;
    //for (string k : keys)
    //{
    //    if (k.Equals(pk.ToString()))
    //    {
    //        exists = true;
    //    }
    //}
    //Assert.AreEqual(exists).isFalse();
    //    }

    //    [When]("I generate a key")
    //    public void genKey()throws NoSuchAlgorithmException, GeneralSecurityException{
    //        account = new Account();
    //pk = account.Address;
    //address = pk.ToString();
    //sk = Mnemonic.toKey(account.toMnemonic());
    //    }

    //    [When]("I import the key")
    //    public void importKey() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ImportKeyRequest req = new ImportKeyRequest();
    //req.setWalletHandleToken(handle);
    //req.setPrivateKey(sk);
    //kcl.importKey(req);
    //    }

    //    [When]("I get the private key")
    //    public void getSk() throws com.algorand.algosdk.kmd.client.ApiException, GeneralSecurityException{
    //    ExportKeyRequest req = new ExportKeyRequest();
    //    req.setAddress(pk.ToString());
    //    req.setWalletHandleToken(handle);
    //    req.setWalletPassword(walletPswd);
    //    sk = kcl.exportKey(req).getPrivateKey();
    //    account = new Account(Arrays.copyOfRange(sk, 0, 32));
    //}

    //[Then]("the private key should be equal to the exported private key")
    //    public void expSkEq() throws com.algorand.algosdk.kmd.client.ApiException
    //{
    //    ExportKeyRequest req = new ExportKeyRequest();
    //req.setAddress(pk.ToString());
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //byte[] exported = Arrays.copyOfRange(kcl.exportKey(req).getPrivateKey(), 0, 32);
    //Assert.AreEqual(Convert.ToBase64String(sk), Convert.ToBase64String(exported));
    //DeleteKeyRequest deleteReq = new DeleteKeyRequest();
    //deleteReq.setAddress(pk.ToString());
    //deleteReq.setWalletHandleToken(handle);
    //deleteReq.setWalletPassword(walletPswd);
    //kcl.deleteKey(deleteReq);
    //    }

    //    [Given]("a kmd client")
    //    public void kClient() throws FileNotFoundException, IOException, NoSuchAlgorithmException{
    //        kmdClient = new KmdClient();
    //kmdClient.setConnectTimeout(30000);
    //kmdClient.setReadTimeout(30000);
    //kmdClient.setWriteTimeout(30000);
    //kmdClient.setApiKey(token);
    //kmdClient.setBasePath("http://localhost:" + kmdPort);
    //kcl = new KmdApi(kmdClient);
    //    }

    //    [Given]("an algod client")
    //    public void aClient() throws FileNotFoundException, IOException{
    //        algodClient = new AlgodClient();
    //algodClient.setConnectTimeout(30000);
    //algodClient.setReadTimeout(30000);
    //algodClient.setWriteTimeout(30000);
    //algodClient.setApiKey(token);
    //algodClient.setBasePath("http://localhost:" + algodPort);
    //acl = new AlgodApi(algodClient);
    //    }
    //    [Given]("an algod v2 client")
    //    public void aClientv2() throws FileNotFoundException, IOException{
    //        aclv2 = new com.algorand.algosdk.v2.client.common.AlgodClient(
    //            "http://localhost", algodPort, token
    //        );
    //    }

    //    [Given]("wallet information")
    //    public void walletInfo() throws com.algorand.algosdk.kmd.client.ApiException, NoSuchAlgorithmException{
    //    walletName = "unencrypted-default-wallet";
    //    walletPswd = "";
    //    List<APIV1Wallet> wallets = kcl.listWallets().getWallets();
    //    for (APIV1Wallet w: wallets)
    //    {
    //        if (w.getName().Equals(walletName))
    //        {
    //            walletID = w.getId();
    //        }
    //    }
    //    InitWalletHandleTokenRequest tokenreq = new InitWalletHandleTokenRequest();
    //    tokenreq.setWalletId(walletID);
    //    tokenreq.setWalletPassword(walletPswd);
    //    handle = kcl.initWalletHandleToken(tokenreq).getWalletHandleToken();
    //    ListKeysRequest req = new ListKeysRequest();
    //    req.setWalletHandleToken(handle);
    //    addresses = kcl.listKeysInWallet(req).getAddresses();
    //    pk = getAddress(0);
    //}

    //[Given]("default transaction with parameters (\d+) (.*)")
    //    public void defaultTxn(int amt, string note) throws ApiException, NoSuchAlgorithmException{
    //        getParams();
    //if (note.Equals("none"))
    //{
    //    this.note = null;
    //}
    //else
    //{
    //    this.note = Convert.FromBase64String(note);
    //}
    //txnBuilder = Transaction.PaymentTransactionBuilder()
    //        .sender(getAddress(0))
    //        .suggestedParams(transParams)
    //        .note(this.note)
    //        .amount(amt)
    //        .receiver(getAddress(1));
    //txn = txnBuilder.build();
    //pk = getAddress(0);
    //    }

    //    [Given]("default multisig transaction with parameters (\d+) (.*)")
    //    public void defaultMsigTxn(int amt, string note) throws ApiException, NoSuchAlgorithmException{
    //        getParams();
    //if (note.Equals("none"))
    //{
    //    this.note = null;
    //}
    //else
    //{
    //    this.note = Convert.FromBase64String(note);
    //}
    //Ed25519PublicKey[] addrlist = new Ed25519PublicKey[addresses.Count];
    //for (int x = 0; x < addresses.Count; x++)
    //{
    //    addrlist[x] = new Ed25519PublicKey((getAddress(x)).getBytes());
    //}
    //msig = new MultisigAddress(1, 1, Arrays.asList(addrlist));
    //txn = Transaction.PaymentTransactionBuilder()
    //        .sender(msig.ToString())
    //        .suggestedParams(transParams)
    //        .note(this.note)
    //        .amount(amt)
    //        .receiver(getAddress(1))
    //        .build();
    //pk = getAddress(0);
    //    }

    //    [When]("I send the transaction")
    //    public void sendTxn() throws JsonProcessingException, ApiException{
    //        txid = acl.rawTransaction(Encoder.EncodeToMsgPack(stx)).getTxId();
    //    }

    //    [When]("I send the multisig transaction")
    //    public void sendMsigTxn() throws JsonProcessingException, ApiException{
    //        try{
    //            acl.rawTransaction(Encoder.EncodeToMsgPack(stx));
    //        } catch (Exception e)
    //{
    //    err = true;
    //}
    //    }

    //    [Then]("the transaction should go through")
    //    public void checkTxn() throws ApiException, InterruptedException{
    //        string ans = acl.pendingTransactionInformation(txid).getFrom();
    //Assert.AreEqual(this.txn.sender.ToString(), ans);
    //acl.waitForBlock(lastRound.add(2)));
    //string senderFromResponse = acl.transactionInformation(txn.sender.ToString(), txid).getFrom();
    //Assert.AreEqual(senderFromResponse, txn.sender.ToString());
    //Assert.AreEqual(acl.transaction(txid).getFrom(), senderFromResponse);
    //    }

    //    [Then]("I can get the transaction by ID")
    //    public void txnbyID() throws ApiException, InterruptedException{
    //        acl.waitForBlock(lastRound.add(2)));
    //Assert.AreEqual(acl.transaction(txid).getFrom(), pk.ToString());
    //    }

    //    [Then]("the transaction should not go through")
    //    public void txnFail()
    //{
    //    Assert.AreEqual(err).isTrue();
    //}

    //[When]("I sign the transaction with kmd")
    //    public void signKmd() throws JsonProcessingException, com.algorand.algosdk.kmd.client.ApiException, NoSuchAlgorithmException{
    //        SignTransactionRequest req = new SignTransactionRequest();
    //req.setTransaction(Encoder.EncodeToMsgPack(txn));
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //stxBytes = kcl.signTransaction(req).getSignedTransaction();
    //    }
    //    [Then]("the signed transaction should equal the kmd signed transaction")
    //    public void signBothEqual() throws JsonProcessingException
    //{
    //    Assert.AreEqual(Convert.ToBase64String(stxBytes), Convert.ToBase64String(Encoder.EncodeToMsgPack(stx)));
    //}

    //[When]("I sign the multisig transaction with kmd")
    //    public void signMsigKmd() throws JsonProcessingException, com.algorand.algosdk.kmd.client.ApiException, IOException{
    //        ImportMultisigRequest importReq = new ImportMultisigRequest();
    //importReq.setMultisigVersion(msig.version);
    //importReq.setThreshold(msig.threshold);
    //importReq.setWalletHandleToken(handle);
    //importReq.setPks(msig.publicKeys);
    //kcl.importMultisig(importReq);

    //SignMultisigRequest req = new SignMultisigRequest();
    //req.setTransaction(Encoder.EncodeToMsgPack(txn));
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //req.setPublicKey(pk.getBytes());
    //stxBytes = kcl.signMultisigTransaction(req).getMultisig();
    //    }

    //    [Then]("the multisig transaction should equal the kmd signed multisig transaction")
    //    public void signMsigBothEqual() throws JsonProcessingException, com.algorand.algosdk.kmd.client.ApiException {
    //        Assert.AreEqual(Convert.ToBase64String(stxBytes), Convert.ToBase64String(Encoder.EncodeToMsgPack(stx.mSig)));
    //DeleteMultisigRequest req = new DeleteMultisigRequest();
    //req.setAddress(msig.ToString());
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //kcl.deleteMultisig(req);
    //    }

    //    [When]("I read a transaction (.*) from file (.*)")
    //    public void readTxn(string encodedTxn, string num) throws IOException
    //{
    //    string path = System.getProperty("user.dir");
    //    Path p = Paths.get(path);
    //    this.num = num;
    //    path = p.getParent() + "/temp/raw" + this.num + ".tx";
    //    FileInputStream inputStream = new FileInputStream(path);
    //    File file = new File(path);
    //    byte[] data = new byte[(int)file.length()];
    //    inputStream.read(data);
    //    stx = Encoder.decodeFromMsgPack(data, SignedTransaction.class);
    //inputStream.close();
    //    }

    //    [When]("I write the transaction to file")
    //    public void writeTxn() throws JsonProcessingException, IOException{
    //        string path = System.getProperty("user.dir");
    //Path p = Paths.get(path);
    //path = p.getParent() + "/temp/raw" + this.num + ".tx";
    //byte[] data = Encoder.EncodeToMsgPack(stx);
    //FileOutputStream out = new FileOutputStream(path);
    //        out.write(data);
    //        out.close();
    //    }

    //    [Then]("the transaction should still be the same")
    //    public void checkEnc() throws IOException
    //{
    //    string path = System.getProperty("user.dir");
    //    Path p = Paths.get(path);
    //    path = p.getParent() + "/temp/raw" + this.num + ".tx";
    //    FileInputStream inputStream = new FileInputStream(path);
    //    File file = new File(path);
    //    byte[] data = new byte[(int)file.length()];
    //    inputStream.read(data);
    //    SignedTransaction stxnew = Encoder.decodeFromMsgPack(data, SignedTransaction.class);
    //inputStream.close();

    //path = p.getParent() + "/temp/old" + this.num + ".tx";
    //inputStream = new FileInputStream(path);
    //file = new File(path);
    //data = new byte[(int)file.length()];
    //inputStream.read(data);
    //SignedTransaction stxold = Encoder.decodeFromMsgPack(data, SignedTransaction.class);
    //inputStream.close();
    //Assert.AreEqual(stxnew, stxold);
    //    }

    //    [Then]("I do my part")
    //    public void signSaveTxn() throws IOException, JsonProcessingException, NoSuchAlgorithmException, com.algorand.algosdk.kmd.client.ApiException, Exception{
    //        string path = System.getProperty("user.dir");
    //Path p = Paths.get(path);
    //path = p.getParent() + "/temp/txn.tx";
    //FileInputStream inputStream = new FileInputStream(path);
    //File file = new File(path);
    //byte[] data = new byte[(int)file.length()];
    //inputStream.read(data);
    //inputStream.close();

    //txn = Encoder.decodeFromMsgPack(data, Transaction.class);
    //exportKeyAndSetAccount(txn.sender);

    //stx = account.signTransaction(txn);
    //data = Encoder.EncodeToMsgPack(stx);
    //FileOutputStream out = new FileOutputStream(path);
    //        out.write(data);
    //        out.close();
    //    }

    //    [Then]("the node should be healthy")
    //    public void nodeHealth() throws ApiException
    //{
    //    acl.healthCheck();
    //}

    //[Then]("I get the ledger supply")
    //    public void getLedger() throws ApiException
    //{
    //    acl.getSupply();
    //}

    //[Then]("I get transactions by address and round")
    //    public void txnsByAddrRound() throws ApiException
    //{
    //    Assert.AreEqual(acl.transactions(addresses.get(0), 1), acl.getStatus().LastRound, null, null, 10)).getTransactions())
    //                .isInstanceOf(List.class);
    //        //Assert.assertTrue(acl.transactions(addresses.get(0), 1), acl.getStatus().LastRound, null, null, 10)).getTransactions() instanceof List<?>);
    //    }

    //    [Then]("I get transactions by address only")
    //    public void txnsByAddrOnly() throws ApiException
    //{
    //    Assert.AreEqual(acl.transactions(addresses.get(0), null, null, null, null, 10)).getTransactions())
    //                .isInstanceOf(List.class);
    //        //Assert.assertTrue(acl.transactions(addresses.get(0), null, null, null, null, 10)).getTransactions() instanceof List<?>);
    //    }

    //    [Then]("I get transactions by address and date")
    //    public void txnsByAddrDate() throws ApiException
    //{
    //    Assert.AreEqual(acl.transactions(addresses.get(0), null, null, LocalDate.now(), LocalDate.now(), 10)).getTransactions())
    //                .isInstanceOf(List.class);
    //        //Assert.assertTrue(acl.transactions(addresses.get(0), null, null, LocalDate.now(), LocalDate.now(), 10)).getTransactions() instanceof List<?>);
    //    }

    //    [Then]("I get pending transactions")
    //    public void pendingTxns() throws ApiException
    //{
    //    Assert.AreEqual(acl.getPendingTransactions(10)).getTruncatedTxns())
    //            .isInstanceOf(TransactionList.class);
    //        //Assert.assertTrue(acl.getPendingTransactions(10)).getTruncatedTxns() instanceof TransactionList);
    //    }

    //    [When]("I get the suggested params")
    //    public void suggestedParams() throws ApiException
    //{
    //    paramsFee = acl.TransactionParams().getFee();
    //}

    //[When]("I get the suggested fee")
    //    public void suggestedFee() throws ApiException
    //{
    //    fee = acl.suggestedFee().getFee();
    //}

    //[Then]("the fee in the suggested params should equal the suggested fee")
    //    public void checkSuggested()
    //{
    //    Assert.AreEqual(paramsFee, fee);
    //}

    //[When]("I create a bid")
    //    public void createBid() throws NoSuchAlgorithmException
    //{
    //    account = new Account();
    //pk = account.Address;
    //address = pk.ToString();
    //bid = new Bid(pk, pk, 1L), 2L), 3L), 4L));
    //    }

    //    [When]("I encode and decode the bid")
    //    public void encDecBid() throws JsonProcessingException, IOException{
    //        sbid = Encoder.decodeFromMsgPack(Encoder.EncodeToMsgPack(sbid), SignedBid.class);
    //    }

    //    [When]("I sign the bid")
    //    public void signBid() throws NoSuchAlgorithmException
    //{
    //    sbid = account.signBid(bid);
    //    oldBid = account.signBid(bid);
    //}

    //[Then]("the bid should still be the same")
    //    public void checkBid()
    //{
    //    Assert.AreEqual(sbid, oldBid);
    //}

    //[When]("I decode the address")
    //    public void decAddr() throws NoSuchAlgorithmException
    //{
    //    pk = new Address(address);
    //oldAddr = address;
    //    }

    //    [When]("I encode the address")
    //    public void encAddr()
    //{
    //    address = pk.ToString();
    //}

    //[Then]("the address should still be the same")
    //    public void checkAddr()
    //{
    //    Assert.AreEqual(address, oldAddr);
    //}

    //[When]("I convert the private key back to a mnemonic")
    //    public void skToMn()
    //{
    //    mnemonic = account.toMnemonic();
    //}

    //[Then]("the mnemonic should still be the same as (.*)")
    //    public void checkMn(string mn)
    //{
    //    Assert.AreEqual(mnemonic, mn);
    //}

    //[Given]("mnemonic for master derivation key (.*)")
    //    public void mnforMdk(string mn) throws GeneralSecurityException
    //{
    //    mdk = Mnemonic.toKey(mn);
    //}

    //[When]("I convert the master derivation key back to a mnemonic")
    //    public void mdkToMn()
    //{
    //    mnemonic = Mnemonic.fromKey(mdk);
    //}

    //[When]("I create the flat fee payment transaction")
    //    public void createPaytxnFlat() throws NoSuchAlgorithmException
    //{
    //    txn = Transaction.PaymentTransactionBuilder()
    //                .sender(pk)
    //                .flatFee(fee)
    //                .firstValid(fv)
    //                .lastValid(lv)
    //                .note(note)
    //                .genesisID(gen)
    //                .genesisHash(gh)
    //                .amount(amt)
    //                .receiver(to)
    //                .closeRemainderTo(close)
    //                .build();
    //}

    //[Given]("encoded multisig transaction (.*)")
    //    public void encMsigTxn(string encTxn) throws IOException
    //{
    //    stx = Encoder.decodeFromMsgPack(Convert.FromBase64String(encTxn), SignedTransaction.class);
    //Ed25519PublicKey[] addrlist = new Ed25519PublicKey[stx.mSig.subsigs.Count];
    //for (int x = 0; x < addrlist.length; x++)
    //{
    //    addrlist[x] = stx.mSig.subsigs.get(x).key;
    //}
    //msig = new MultisigAddress(stx.mSig.version, stx.mSig.threshold, Arrays.asList(addrlist));
    //    }

    //    [When]("I append a signature to the multisig transaction")
    //    public void appendMsig() throws NoSuchAlgorithmException
    //{
    //    stx = account.appendMultisigTransaction(msig, stx);
    //}

    //[Given]("encoded multisig transactions (.*)")
    //    public void encMsigTxns(string encTxns) throws IOException
    //{
    //    string []
    //    txnArray = encTxns.split(" ");
    //    stxs = new SignedTransaction[txnArray.length];
    //for (int t = 0; t < txnArray.length; t++)
    //{
    //    stxs[t] = Encoder.decodeFromMsgPack(Convert.FromBase64String(txnArray[t]), SignedTransaction.class);
    //        }
    //    }

    //    [When]("I merge the multisig transactions")
    //    public void mergeMsig()
    //{
    //    stx = Account.mergeMultisigTransactions(stxs);
    //}

    //[When]("I convert {long} microalgos to algos and back")
    //    public void microToAlgo(long ma)
    //{
    //    microalgos = ma);
    //    BigDecimal algos = AlgoConverter.toAlgos(microalgos);
    //    microalgos = AlgoConverter.toMicroAlgos(algos);
    //}

    //[Then]("it should still be the same amount of microalgos {long}")
    //    public void checkMicro(long ma)
    //{
    //    Assert.AreEqual(microalgos, ma));
    //}

    //[Then]("I get account information")
    //    public void accInfo() throws ApiException
    //{
    //    acl.accountInformation(addresses.get(0));
    //    }

    //    [Then]("I can get account information")
    //    public void newAccInfo() throws ApiException, NoSuchAlgorithmException, com.algorand.algosdk.kmd.client.ApiException {
    //        acl.accountInformation(pk.encodeAsString());
    //DeleteKeyRequest req = new DeleteKeyRequest();
    //req.setAddress(pk.encodeAsString());
    //req.setWalletHandleToken(handle);
    //req.setWalletPassword(walletPswd);
    //kcl.deleteKey(req);
    //    }

    //    [When]("I get recent transactions, limited by (\d+) transactions")
    //    public void i_get_recent_transactions_limited_by_count(int cnt) throws ApiException
    //{
    //    Assert.AreEqual(acl.transactions(addresses.get(0), null, null, null, null, cnt)).getTransactions())
    //                .isInstanceOf(List.class);
    //        //Assert.assertTrue(acl.transactions(addresses.get(0), null, null, null, null, cnt)).getTransactions() instanceof List<?>);
    //    }

    //    [Given]("asset test fixture")
    //    public void asset_test_fixture()
    //{
    //    // Implemented by the construction of Stepdefs;
    //}

    //[Given]("default asset creation transaction with total issuance (\d+)")
    //    public void default_asset_creation_transaction_with_total_issuance(int assetTotal) throws NoSuchAlgorithmException, ApiException, InvalidKeySpecException {
    //        getParams();

    //Transaction tx = Transaction.AssetCreateTransactionBuilder()
    //        .sender(getAddress(0))
    //        .suggestedParams(transParams)
    //        .note(this.note)
    //        .assetTotal(assetTotal)
    //        .assetDecimals(0)
    //        .assetName(this.assetName)
    //        .assetUnitName(this.assetUnitName)
    //        .manager(getAddress(0))
    //        .reserve(getAddress(0))
    //        .clawback(getAddress(0))
    //        .freeze(getAddress(0))
    //        .build();

    //this.creator = addresses.get(0);
    //this.txn = tx;
    //this.expectedParams = tx.assetParams;
    //    }

    //    [When]("I get the asset info")
    //    public void i_get_the_asset_info() throws ApiException
    //{
    //        this.queriedParams = acl.assetInformation(this.assetID);
    //}

    //[Then]("the asset info should match the expected asset info")
    //    public void the_asset_info_should_match_the_expected_asset_info() throws JsonProcessingException, NoSuchAlgorithmException {
    //        // Can't use a regular assertj call because 'compareTo' isn't a regular comparator.
    //        Assert.AreEqual(this.expectedParams.assetManager.compareTo(this.queriedParams.getManagerkey())).isTrue();
    //Assert.AreEqual(this.expectedParams.assetReserve.compareTo(this.queriedParams.getReserveaddr())).isTrue();
    //Assert.AreEqual(this.expectedParams.assetFreeze.compareTo(this.queriedParams.getFreezeaddr())).isTrue();
    //Assert.AreEqual(this.expectedParams.assetClawback.compareTo(this.queriedParams.getClawbackaddr())).isTrue();
    //    }

    //    [When]("I create a no-managers asset reconfigure transaction")
    //    public void i_create_a_no_managers_asset_reconfigure_transaction() throws NoSuchAlgorithmException, ApiException, InvalidKeySpecException {
    //        getParams();

    //Transaction tx = Transaction.AssetConfigureTransactionBuilder()
    //        .sender(this.creator)
    //        .suggestedParams(transParams)
    //        .note(this.note)
    //        .assetIndex(this.assetID)
    //        .manager(this.creator)
    //        .strictEmptyAddressChecking(false)
    //        .build();
    //this.txn = tx;
    //this.expectedParams = tx.assetParams;
    //    }

    //    [When]("I create an asset destroy transaction")
    //    public void i_create_an_asset_destroy_transaction() throws NoSuchAlgorithmException, ApiException, InvalidKeySpecException {
    //        getParams();

    //Transaction tx = Transaction.AssetDestroyTransactionBuilder()
    //        .sender(this.creator)
    //        .suggestedParams(this.transParams)
    //        .note(this.note)
    //        .assetIndex(this.assetID)
    //        .build();
    //this.txn = tx;
    //this.expectedParams = tx.assetParams;
    //    }

    //    [Then]("I should be unable to get the asset info")
    //    public void i_should_be_unable_to_get_the_asset_info()
    //{
    //    bool exists = true;
    //    try
    //    {
    //        this.i_get_the_asset_info();
    //    }
    //    catch (ApiException e)
    //    {
    //        exists = false;
    //    }
    //    Assert.AreEqual(exists).isFalse();
    //}

    //[When]("I create a transaction transferring (\d+) assets from creator to a second account")
    //    public void i_create_a_transaction_transferring_assets_from_creator_to_a_second_account(int int1) throws NoSuchAlgorithmException, ApiException, InvalidKeySpecException {
    //        getParams();

    //Transaction tx = Transaction.AssetTransferTransactionBuilder()
    //        .sender(this.creator)
    //        .assetReceiver(getAddress(1))
    //        .assetAmount(int1)
    //        .suggestedParams(this.transParams)
    //        .note(this.note)
    //        .assetIndex(this.assetID)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(0);
    //    }

    //    [Then]("the creator should have (\d+) assets remaining")
    //    public void the_creator_should_have_assets_remaining(int expectedBal) throws ApiException
    //{
    //    com.algorand.algosdk.algod.client.model.Account accountResp =
    //                this.acl.accountInformation(this.creator);
    //    AssetHolding holding = accountResp.getHolding(this.assetID);
    //    Assert.AreEqual(holding.getAmount(), expectedBal));
    //}

    //[Then]("I update the asset index")
    //    public void i_update_the_asset_index() throws ApiException
    //{
    //    com.algorand.algosdk.algod.client.model.Account accountResp = acl.accountInformation(this.creator);
    //    Set<java.math.BigInteger> keys = accountResp.getThisassettotal().keySet();
    //        this.assetID = Collections.max(keys);
    //}

    //[When]("I send the bogus kmd-signed transaction")
    //    public void i_send_the_bogus_kmd_signed_transaction()
    //{
    //    try
    //    {
    //        txid = acl.rawTransaction(this.stxBytes).getTxId();
    //    }
    //    catch (ApiException e)
    //    {
    //        this.err = true;
    //    }
    //}

    //[Then]("I create a transaction for a second account, signalling asset acceptance")
    //    public void i_create_a_transaction_for_a_second_account_signalling_asset_acceptance() throws ApiException, NoSuchAlgorithmException {
    //        getParams();

    //Transaction tx = Transaction.AssetAcceptTransactionBuilder()
    //        .acceptingAccount(getAddress(1))
    //        .suggestedParams(this.transParams)
    //        .note(this.note)
    //        .assetIndex(this.assetID)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(1);
    //    }

    //    [Then]("I send the kmd-signed transaction")
    //    public void i_send_the_kmd_signed_transaction() throws ApiException
    //{
    //    txid = acl.rawTransaction(this.stxBytes).getTxId();
    //}

    //[When]("I create a freeze transaction targeting the second account")
    //    public void i_create_a_freeze_transaction_targeting_the_second_account() throws NoSuchAlgorithmException, ApiException, com.algorand.algosdk.kmd.client.ApiException {
    //        this.renewHandle(); // to avoid handle expired error
    //getParams();

    //Transaction tx = Transaction.AssetFreezeTransactionBuilder()
    //        .sender(getAddress(0))
    //        .freezeTarget(getAddress(1))
    //        .freezeState(true)
    //        .assetIndex(this.assetID)
    //        .note(this.note)
    //        .suggestedParams(this.transParams)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(0);
    //    }

    //    [When]("I create a transaction transferring (\d+) assets from a second account to creator")
    //    public void i_create_a_transaction_transferring_assets_from_a_second_account_to_creator(int int1) throws ApiException, NoSuchAlgorithmException {
    //        getParams();

    //Transaction tx = Transaction.AssetTransferTransactionBuilder()
    //        .sender(getAddress(1))
    //        .assetReceiver(this.creator)
    //        .assetAmount(int1)
    //        .note(this.note)
    //        .assetIndex(this.assetID)
    //        .suggestedParams(this.transParams)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(1);
    //    }

    //    [When]("I create an un-freeze transaction targeting the second account")
    //    public void i_create_an_un_freeze_transaction_targeting_the_second_account() throws ApiException, NoSuchAlgorithmException, com.algorand.algosdk.kmd.client.ApiException  {
    //        this.renewHandle(); // to avoid handle expired error
    //getParams();

    //Transaction tx = Transaction.AssetFreezeTransactionBuilder()
    //        .sender(getAddress(0))
    //        .freezeTarget(getAddress(1))
    //        .freezeState(false)
    //        .assetIndex(this.assetID)
    //        .note(this.note)
    //        .suggestedParams(this.transParams)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(0);
    //    }

    //    [Given]("default-frozen asset creation transaction with total issuance (\d+)")
    //    public void default_frozen_asset_creation_transaction_with_total_issuance(int int1) throws ApiException, NoSuchAlgorithmException {
    //        getParams();

    //Transaction tx = Transaction.AssetCreateTransactionBuilder()
    //        .sender(getAddress(0))
    //        .suggestedParams(this.transParams)
    //        .note(this.note)
    //        .assetTotal(int1)
    //        .assetDecimals(0)
    //        .defaultFrozen(true)
    //        .assetName(this.assetName)
    //        .assetUnitName(this.assetUnitName)
    //        .manager(getAddress(0))
    //        .reserve(getAddress(0))
    //        .freeze(getAddress(0))
    //        .clawback(getAddress(0))
    //        .build();
    //Account.setFeeByFeePerByte(tx, tx.fee);
    //this.creator = addresses.get(0);
    //this.txn = tx;
    //this.expectedParams = tx.assetParams;
    //    }

    //    [When]("I create a transaction revoking (\d+) assets from a second account to creator")
    //    public void i_create_a_transaction_revoking_assets_from_a_second_account_to_creator(int int1) throws ApiException, NoSuchAlgorithmException {
    //        getParams();

    //Transaction tx = Transaction.AssetClawbackTransactionBuilder()
    //        .sender(getAddress(0))
    //        .assetClawbackFrom(getAddress(1))
    //        .assetReceiver(getAddress(0))
    //        .assetAmount(int1)
    //        .assetIndex(this.assetID)
    //        .note(this.note)
    //        .suggestedParams(this.transParams)
    //        .build();
    //this.txn = tx;
    //this.pk = getAddress(0);
    //    }

    //    [When]("I add a rekeyTo field with address (.*)")
    //    public void i_add_a_rekeyTo_field_with_address(string string)
    //{
    //    txnBuilder.rekey(string);
    //    txn = txnBuilder.build();
    //}

    //[When]("I add a rekeyTo field with the private key algorand address")
    //    public void i_add_a_rekeyTo_field_with_the_private_key_algorand_address()
    //{
    //    txnBuilder.rekey(this.pk.ToString());
    //    txn = txnBuilder.build();
    //}

    //[When]("I compile a teal program (.*)")
    //    public void i_compile_teal_program(string path) throws Exception
    //{
    //        byte[]
    //    source = loadResource(path);
    //    compileResponse = aclv2.TealCompile().source(source).execute();
    //}

    //[Then]("it is compiled with (\d+) and (.*) and (.*)")
    //    public void it_is_compiled_with(int status, string result, string hash)
    //{
    //    Assert.AreEqual(compileResponse.code(), status);
    //    CompileResponse body = compileResponse.body();
    //    if (body != null)
    //    {
    //        Assert.AreEqual(compileResponse.isSuccessful()).isTrue();
    //        Assert.AreEqual(body.result, result);
    //        Assert.AreEqual(body.hash, hash);
    //    }
    //    else
    //    {
    //        Assert.AreEqual(compileResponse.isSuccessful()).isFalse();
    //    }
    //}

    //[When]("I dryrun a (.*) program (.*)")
    //    public void i_dryrun_a_program(string kind, string path) throws Exception
    //{
    //        byte[]
    //    data = loadResource(path);
    //    List<DryrunSource> sources = new ArrayList<DryrunSource>();
    //List<SignedTransaction> stxns = new ArrayList<SignedTransaction>();
    //Account account = new Account();
    //Address pk = account.Address;
    //Digest gh = new Digest(Convert.FromBase64String("ZIkPs8pTDxbRJsFB1yJ7gvnpDu0Q85FRkl2NCkEAQLU="));
    //Transaction txn = Transaction.PaymentTransactionBuilder()
    //    .sender(pk)
    //    .fee(1000)
    //    .firstValid(1)
    //    .lastValid(100)
    //    .amount(1000)
    //    .genesisHash(gh)
    //    .receiver(pk)
    //    .build();

    //if (kind.Equals("compiled"))
    //{
    //    LogicsigSignature lsig = new LogicsigSignature(data);
    //    SignedTransaction stxn = new SignedTransaction(txn, lsig);
    //    stxns.add(stxn);
    //}
    //else if (kind.Equals("source"))
    //{
    //    DryrunSource drs = new DryrunSource();
    //    drs.fieldName = "lsig";
    //    drs.source = new String(data);
    //    drs.txnIndex = 0l;
    //    sources.add(drs);
    //    SignedTransaction stxn = new SignedTransaction(txn, new Signature());
    //    stxns.add(stxn);
    //}
    //else
    //{
    //    fail("kind " + kind + " not in (compiled, source)");
    //}

    //DryrunRequest dr = new DryrunRequest();
    //dr.txns = stxns;
    //dr.sources = sources;
    //dryrunResponse = aclv2.TealDryrun().request(dr).execute();
    //    }

    //    [When]("I get execution result (.*)")
    //    public void i_get_execution_result(string result) throws Exception
    //{
    //    DryrunResponse ddr = dryrunResponse.body();
    //    Assert.AreEqual(ddr).isNotNull();
    //    Assert.AreEqual(ddr.txns).isNotNull();
    //    Assert.AreEqual(ddr.txns.Count).isGreaterThan(0);
    //    List<String> msgs = new ArrayList<String>();
    //if (ddr.txns.get(0).appCallMessages.Count > 0)
    //{
    //    msgs = ddr.txns.get(0).appCallMessages;
    //}
    //else if (ddr.txns.get(0).logicSigMessages.Count > 0)
    //{
    //    msgs = ddr.txns.get(0).logicSigMessages;
    //}
    //Assert.AreEqual(msgs.Count).isGreaterThan(0);
    //Assert.AreEqual(msgs.get(msgs.Count - 1), result);
    //    }
}
}
