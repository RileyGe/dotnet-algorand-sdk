using Algorand;
using Algorand.V2;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Account = Algorand.Account;

namespace sdk_examples.V2
{
    /// <summary>
    /// Show Creating, modifying, sending and listing assets 
    /// </summary>
    class AssetExample
    {
        // Utility function for sending a raw signed transaction to the network        
        public async Task Main(params string[] args) //throws Exception
        {
            string algodApiAddrTmp = args[0];
            if (algodApiAddrTmp.IndexOf("//") == -1)
            {
                algodApiAddrTmp = "http://" + algodApiAddrTmp;
            }

            string ALGOD_API_ADDR = algodApiAddrTmp;
            string ALGOD_API_TOKEN = args[1];

            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };

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
            var transParams = await algodApiInstance.ParamsAsync();

            // The following parameters are asset specific
            // and will be re-used throughout the example. 

            // Create the Asset
            // Total number of this asset available for circulation            
            var ap = new AssetParams()
            {
                Creator = acct1.Address.ToString(),
                Name =  "latikum22",
                UnitName= "LAT",
                 Total= 10000,
                Decimals= 0,
                Url = @"http://this.test.com", 
                MetadataHash = Encoding.ASCII.GetBytes("16efaa3924a6fd9d3a4880099a4ac65d"),
                Manager = acct2.Address.ToString()
            };

            // Specified address can change reserve, freeze, clawback, and manager
            // you can leave as default, by default the sender will be manager/reserve/freeze/clawback
            // the following code only set the freeze to acct1
            var tx = Utils.GetCreateAssetTransaction(ap, transParams, "asset tx message");

            // Sign the Transaction by sender
            SignedTransaction signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            ulong assetID = 0;
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // Now that the transaction is confirmed we can get the assetID
                var ptx = await  algodApiInstance.PendingGetAsync(id.TxId,null);                
                assetID = ptx.AssetIndex??0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return;
            }
            Console.WriteLine("AssetID = " + assetID);
            // now the asset already created


            // Change Asset Configuration:
            // Next we will change the asset configuration
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            transParams = await algodApiInstance.ParamsAsync();
            Asset ast = await algodApiInstance.AssetsAsync(assetID);

            // Note that configuration changes must be done by
            // The manager account, which is currently acct2
            // Note in this transaction we are re-using the asset
            // creation parameters and only changing the manager
            // and transaction parameters like first and last round
            // now update the manager to acct1
            ast.Params.Manager = acct1.Address.ToString();
            tx = Utils.GetConfigAssetTransaction(acct2.Address, ast, transParams, "config trans");

            // The transaction must be signed by the current manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }

            // Next we will list the newly created asset
            // Get the asset information for the newly changed asset            
            ast = await algodApiInstance.AssetsAsync(assetID);
            //The manager should now be the same as the creator
            Console.WriteLine(ap);



            // Opt in to Receiving the Asset
            // Opting in to transact with the new asset
            // All accounts that want recieve the new asset
            // Have to opt in. To do this they send an asset transfer
            // of the new asset to themseleves with an ammount of 0
            // In this example we are setting up the 3rd recovered account to 
            // receive the new asset        
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            transParams = await algodApiInstance.ParamsAsync();
            tx = Utils.GetAssetOptingInTransaction(acct3.Address, assetID, transParams, "opt in transaction");

            // The transaction must be signed by the current manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct3.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            Algorand.V2.Algod.Model.Account act = null;
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // We can now list the account information for acct3 
                // and see that it can accept the new asseet
                act  = await algodApiInstance.AccountsAsync(acct3.Address.ToString(),null);
                Console.WriteLine(act);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            // Transfer the Asset:
            // Now that account3 can recieve the new asset 
            // we can tranfer assets in from the creator
            // to account3
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            transParams = await algodApiInstance.ParamsAsync();
            // Next we set asset xfer specific parameters
            // We set the assetCloseTo to null so we do not close the asset out
            Address assetCloseTo = new Address();
            ulong assetAmount = 10;
            tx = Utils.GetTransferAssetTransaction(acct1.Address, acct3.Address, assetID, assetAmount, transParams, null, "transfer message");
            // The transaction must be signed by the sender account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // We can now list the account information for acct3 
                // and see that it now has 5 of the new asset
                act = await algodApiInstance.AccountsAsync(acct3.Address.ToString(),null);
                Console.WriteLine(act.Assets.Where(h => h.AssetId == assetID).FirstOrDefault()?.Amount);
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
            transParams = await algodApiInstance.ParamsAsync();
            // Next we set asset xfer specific parameters
            // The sender should be freeze account acct2
            // Theaccount to freeze should be set to acct3
            tx = Utils.GetFreezeAssetTransaction(acct2.Address, acct3.Address, assetID, true, transParams, "freeze transaction");
            // The transaction must be signed by the freeze account acct2
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id.TxId);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // We can now list the account information for acct3 
                // and see that it now frozen 
                // Note--currently no getter method for frozen state
                act = await algodApiInstance.AccountsAsync(acct3.Address.ToString(),null);
                Console.WriteLine(act.Assets.Where(h => h.AssetId == assetID).FirstOrDefault());
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
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
            transParams = await algodApiInstance.ParamsAsync();
            // Next we set asset xfer specific parameters
            assetAmount = 10;
            tx = Utils.GetRevokeAssetTransaction(acct2.Address, acct3.Address, acct1.Address, assetID, assetAmount, transParams, "revoke transaction");
            // The transaction must be signed by the clawback account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct2.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // We can now list the account information for acct3 
                // and see that it now has 0 of the new asset
                act = await algodApiInstance.AccountsAsync(acct3.Address.ToString(),null);
                Console.WriteLine(act.Assets.Where(h => h.AssetId == assetID).FirstOrDefault()?.Amount);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }

            // Destroy the Asset:
            // All of the created assets should now be back in the creators
            // Account so we can delete the asset.
            // If this is not the case the asset deletion will fail
            // The address for the from field must be the creator
            // First we update standard Transaction parameters
            // To account for changes in the state of the blockchain
            transParams = await  algodApiInstance.ParamsAsync();
            // Next we set asset xfer specific parameters
            // The manager must sign and submit the transaction
            // This is currently set to acct1
            tx = Utils.GetDestroyAssetTransaction(acct1.Address, assetID, transParams, "destroy transaction");
            // The transaction must be signed by the manager account
            // We are reusing the signedTx variable from the first transaction in the example    
            signedTx = acct1.SignTransaction(tx);
            // send the transaction to the network and
            // wait for the transaction to be confirmed
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Transaction ID: " + id);
                //waitForTransactionToComplete(algodApiInstance, signedTx.transactionID);
                //Console.ReadKey();
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
                // We can now list the account information for acct1 
                // and see that the asset is no longer there
                act = await algodApiInstance.AccountsAsync(acct1.Address.ToString(),null);
                //Console.WriteLine("Does AssetID: " + assetID + " exist? " +
                //    act.Thisassettotal.ContainsKey(assetID));
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Console.WriteLine(e.Message);
                return;
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }
    }
}
