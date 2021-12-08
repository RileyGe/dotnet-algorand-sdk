using Algorand;
using Algorand.V2;
using Algorand.V2.Algod;
using System;
using System.Threading.Tasks;
using Account = Algorand.Account;

namespace sdk_examples.V2
{
    /// <summary>
    /// Part 1
    /// rekey from Account 3 to allow to sign from Account 1
    /// Part 2
    /// send from account 3 to account 2 and sign from Account 1
    /// </summary>
    class RekeyExample
    {
        public async Task Main(string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args.Length > 1 ? args[1] : null;
            var account1_passphrase = "fringe model trophy claw stove perfect address market license abstract master slender choice around field embark sudden carbon exclude abuse square bulb front ability violin";
            var account2_passphrase = "impulse nation creek toy carpet amused dream can small long disorder source mail game category damp spread length cupboard theory either baby squeeze about orbit";
            var account3_passphrase = "fade exit sword someone lock minimum scout keen label dance jaguar select conduct luxury rose idea solid major solid lens globe agent assume abstract alien";


            var account1 = new Account(account1_passphrase);
            var account2 = new Account(account2_passphrase);
            //    private_key = mnemonic.to_private_key(account3_passphrase)
            var account3 = new Account(account3_passphrase);

            Console.WriteLine(string.Format("Account 1 : {0}", account1.Address));
            Console.WriteLine(string.Format("Account 2 : {0}", account2.Address));
            Console.WriteLine(string.Format("Account 3 : {0}", account3.Address));

            //Part 1
            //build transaction

            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            var trans = await algodApiInstance.ParamsAsync();   
            Console.WriteLine("Lastround: " + trans.LastRound.ToString());

            
            bool firstRun = false;

            if (firstRun)
            {
                ulong? amount = 0;
                //opt-in send tx to same address as sender and use 0 for amount w rekey account to account 1
                var tx = Utils.GetPaymentTransaction(account3.Address, account3.Address, amount, "pay message", trans);
                tx.RekeyTo = account1.Address;

                var signedTx = account3.SignTransaction(tx);
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
                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            var act = await algodApiInstance.AccountsAsync(account3.Address.ToString(),null);
            Console.WriteLine(act);

            ulong? amount2 = 1000000;
            var tx2 = Utils.GetPaymentTransaction(account3.Address, account2.Address, amount2, "pay message", trans);
            tx2.RekeyTo = account1.Address;
            var signedTx2 = account1.SignTransaction(tx2);
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx2);
                Console.WriteLine("Transaction ID: " + id);
                Console.WriteLine("Confirmed Round is: " +
                    Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }
    }
}
