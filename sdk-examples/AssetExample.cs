using Algorand;
using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using Algorand.Util;
using System;
using System.Text;
using Account = Algorand.Account;

namespace sdk_examples
{
    /**
 * Show Creating, modifying, sending and listing assets 
 */
    class AssetExample
    {
        // Inline class to handle changing block parameters
        // Throughout the example
        public class ChangingBlockParms
        {
            public ulong? fee;
            public ulong? firstRound;
            public ulong? lastRound;
            public string genID;
            public Digest genHash;
            public ChangingBlockParms()
            {
                this.fee = 0;
                this.firstRound = 0;
                this.lastRound = 0;
                this.genID = "";
                this.genHash = null;
            }
        };        

        // Utility function to update changing block parameters 
        public static ChangingBlockParms GetChangingParms(AlgodApi algodApiInstance) //throws Exception
        {
            ChangingBlockParms cp = new ChangingBlockParms();
            //try {
            TransactionParams transParams = algodApiInstance.TransactionParams();
            cp.fee = transParams.Fee;
            cp.firstRound = transParams.LastRound;
            cp.lastRound = cp.firstRound + 1000;
            cp.genID = transParams.GenesisID;
            cp.genHash = new Digest(Convert.FromBase64String(transParams.Genesishashb64));
            //} catch (ApiException e) {
            //    throw (e);
            //}
            return cp;
        }

        // Utility function for sending a raw signed transaction to the network
        
        public static void Main(params string[] args) //throws Exception
        {
            string algodApiAddrTmp = args[0];
            if (algodApiAddrTmp.IndexOf("//") == -1)
            {
                algodApiAddrTmp = "http://" + algodApiAddrTmp;
            }

            string ALGOD_API_ADDR = algodApiAddrTmp;
            string ALGOD_API_TOKEN = args[1];

            //    AlgodClient client = new AlgodClient().SetBasePath(ALGOD_API_ADDR);

            //ApiKeyAuth api_key = (ApiKeyAuth)client.getAuthentication("api_key");
            //api_key.setApiKey(ALGOD_API_TOKEN);
            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            // Shown for demonstration purposes. NEVER reveal secret mnemonics in practice.
            // These three accounts are for testing purposes
            string account1_mnemonic = "portion never forward pill lunch organ biology"
                                      + " weird catch curve isolate plug innocent skin grunt"
                                      + " bounce clown mercy hole eagle soul chunk type absorb trim";
            string account2_mnemonic = "place blouse sad pigeon wing warrior wild script"
                               + " problem team blouse camp soldier breeze twist mother"
                               + " vanish public glass code arrow execute convince ability"
                               + " there";
            string account3_mnemonic = "image travel claw climb bottom spot path roast "
                               + "century also task cherry address curious save item "
                               + "clean theme amateur loyal apart hybrid steak about blanket";

            Account acct1 = new Account(account1_mnemonic);
            Account acct2 = new Account(account2_mnemonic);
            Account acct3 = new Account(account3_mnemonic);
            // get last round and suggested tx fee
            // We use these to get the latest round and tx fees
            // These parameters will be required before every 
            // Transaction
            // We will account for changing transaction parameters
            // before every transaction in this example
            ChangingBlockParms cp = null;
            //try {
            cp = GetChangingParms(algodApiInstance);
            //} catch (ApiException e) {
            //    e.printStackTrace();
            //    return;
            //}
            // The following parameters are asset specific
            // and will be re-used throughout the example. 

            // Create the Asset
            // Total number of this asset available for circulation
            ulong? assetTotal = 10000;
            // Whether user accounts will need to be unfrozen before transacting
            bool defaultFrozen = false;
            // Used to display asset units to user
            String unitName = "LATIUM22";
            // Friendly name of the asset
            String assetName = "latikum22";
            // Optional string pointing to a URL relating to the asset 
            String url = "http://this.test.com";
            // Optional hash commitment of some sort relating to the asset. 32 character length.
            String assetMetadataHash = "16efaa3924a6fd9d3a4880099a4ac65d";
            // The following parameters are the only ones
            // that can be changed, and they have to be changed
            // by the current manager
            // Specified address can change reserve, freeze, clawback, and manager
            Address manager = acct2.Address;
            // Specified address is considered the asset reserve
            // (it has no special privileges, this is only informational)
            Address reserve = acct2.Address;
            // Specified address can freeze or unfreeze user asset holdings
            Address freeze = acct2.Address;
            // Specified address can revoke user asset holdings and send 
            // them to other addresses
            Address clawback = acct2.Address;
            Algorand.Transaction tx = Algorand.Transaction.CreateAssetCreateTransaction(acct1.Address, 1000, cp.firstRound, cp.lastRound, null, cp.genID,
            cp.genHash, assetTotal, 0, defaultFrozen, unitName, assetName, url,
            Encoding.UTF8.GetBytes(assetMetadataHash), manager, reserve, freeze, clawback);
            // Update the fee as per what the BlockChain is suggesting
            Account.SetFeeByFeePerByte(tx, cp.fee);

            // Sign the Transaction
            SignedTransaction signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            ulong? assetID = 0;
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                // Now that the transaction is confirmed we can get the assetID
                Algorand.Algod.Client.Model.Transaction ptx = algodApiInstance.PendingTransactionInformation(id.TxId);
                assetID = ptx.Txresults.Createdasset;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }

            Console.WriteLine("AssetID = " + assetID);




            // Change Asset Configuration:
            // Next we will change the asset configuration
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            //ulong? assetTotal = 10000;
            // Note that configuration changes must be done by
            // The manager account, which is currently acct2
            // Note in this transaction we are re-using the asset
            // creation parameters and only changing the manager
            // and transaction parameters like first and last round
            tx = Algorand.Transaction.CreateAssetConfigureTransaction(acct2.Address, 1000,
                cp.firstRound, cp.lastRound, null, cp.genID, cp.genHash, assetID, acct1.Address, reserve, freeze, clawback, false);
            // Update the fee as per what the BlockChain is suggesting
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the current manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }

            // Next we will list the newly created asset
            // Get the asset information for the newly changed asset
            AssetParams assetInfo = algodApiInstance.AssetInformation((long?)assetID);
            //The manager should now be the same as the creator
            Console.WriteLine(assetInfo);





            // Opt in to Receiving the Asset
            // Opting in to transact with the new asset
            // All accounts that want recieve the new asset
            // Have to opt in. To do this they send an asset transfer
            // of the new asset to themseleves with an ammount of 0
            // In this example we are setting up the 3rd recovered account to 
            // receive the new asset        
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //e.printStackTrace();
                return;
            }
            tx = Algorand.Transaction.CreateAssetAcceptTransaction(acct3.Address, 1000, cp.firstRound,
            cp.lastRound, null, cp.genID, cp.genHash, assetID);
            // Update the fee based on the network suggested fee
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the current manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct3.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            Algorand.Algod.Client.Model.Account act = null;
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                // We can now list the account information for acct3 
                // and see that it can accept the new asseet
                act = algodApiInstance.AccountInformation(acct3.Address.ToString());
                Console.WriteLine(act);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }





            // Transfer the Asset:
            // Now that account3 can recieve the new asset 
            // we can tranfer assets in from the creator
            // to account3
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }
            // Next we set asset xfer specific parameters
            // We set the assetCloseTo to null so we do not close the asset out
            Address assetCloseTo = new Address();
            ulong? assetAmount = 10;
            tx = Algorand.Transaction.CreateAssetTransferTransaction(acct1.Address,
                acct3.Address, assetCloseTo, assetAmount, 1000,
                cp.firstRound, cp.lastRound, null, cp.genID, cp.genHash, assetID);
            // Update the fee based on the network suggested fee
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the sender account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                // We can now list the account information for acct3 
                // and see that it now has 5 of the new asset
                act = algodApiInstance.AccountInformation(acct3.Address.ToString());
                
                Console.WriteLine(act.GetHolding(assetID).Amount);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }





            // Freeze the Asset:
            // The asset was created and configured to allow freezing an account
            // If the freeze address is blank, it will no longer be possible to do this.
            // In this example we will now freeze account3 from transacting with the 
            // The newly created asset. 
            // Thre freeze transaction is sent from the freeze acount
            // Which in this example is account2 
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }
            // Next we set asset xfer specific parameters
            bool freezeState = true;
            // The sender should be freeze account acct2
            // Theaccount to freeze should be set to acct3
            tx = Algorand.Transaction.CreateAssetFreezeTransaction(acct2.Address,
                acct3.Address, freezeState, 1000, cp.firstRound,
                cp.lastRound, null, cp.genHash, assetID);
            // Update the fee based on the network suggested fee
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the freeze account acct2
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                // We can now list the account information for acct3 
                // and see that it now frozen 
                // Note--currently no getter method for frozen state
                act = algodApiInstance.AccountInformation(acct3.Address.ToString());
                Console.WriteLine(act.GetHolding(assetID).ToString());
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return;
            }




            // Revoke the asset:
            // The asset was also created with the ability for it to be revoked by 
            // clawbackaddress. If the asset was created or configured by the manager
            // not allow this by setting the clawbackaddress to a blank address  
            // then this would not be possible.
            // We will now clawback the 10 assets in account3. Account2
            // is the clawbackaccount and must sign the transaction
            // The sender will be be the clawback adress.
            // the recipient will also be be the creator acct1 in this case  
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return;
            }
            // Next we set asset xfer specific parameters
            assetAmount = (10);
            tx = Algorand.Transaction.CreateAssetRevokeTransaction(acct2.Address,
            acct3.Address, acct1.Address, assetAmount, (1000), cp.firstRound,
            cp.lastRound, null, cp.genID, cp.genHash, assetID);
            // Update the fee based on the network suggested fee
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the clawback account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                // We can now list the account information for acct3 
                // and see that it now has 0 of the new asset
                act = algodApiInstance.AccountInformation(acct3.Address.ToString());
                Console.WriteLine(act.GetHolding(assetID).Amount);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return;
            }



            // Destroy the Asset:
            // All of the created assets should now be back in the creators
            // Account so we can delete the asset.
            // If this is not the case the asset deletion will fail
            // The address for the from field must be the creator
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            try
            {
                cp = GetChangingParms(algodApiInstance);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return;
            }
            // Next we set asset xfer specific parameters
            // The manager must sign and submit the transaction
            // This is currently set to acct1
            tx = Algorand.Transaction.CreateAssetDestroyTransaction(acct1.Address,
                (1000), cp.firstRound, cp.lastRound, null, cp.genHash, assetID);
            // Update the fee based on the network suggested fee
            Account.SetFeeByFeePerByte(tx, cp.fee);
            // The transaction must be signed by the manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                TransactionID id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
                // We can now list the account information for acct1 
                // and see that the asset is no longer there
                act = algodApiInstance.AccountInformation(acct1.Address.ToString());
                //Console.WriteLine("Does AssetID: " + assetID + " exist? " +
                //    act.Thisassettotal.ContainsKey(assetID));
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                return;
            }

        }
    }
}
