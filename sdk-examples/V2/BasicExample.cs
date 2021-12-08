using Algorand;
using Algorand.V2;
using Algorand.Client;

using System;
using Account = Algorand.Account;
using Algorand.V2.Algod;
using Algorand.V2.Algod.Model;
using System.Threading.Tasks;

namespace sdk_examples.V2
{
    class BasicExample
    {
        public async Task Main(string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args[1];            
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            if (!Address.IsValid(DEST_ADDR))
                Console.WriteLine("The address " + DEST_ADDR + " is not valid!");
            Account src = new Account(SRC_ACCOUNT);
            Console.WriteLine("My account address is:" + src.Address.ToString());
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            

            try
            {
                var supply = await algodApiInstance.SupplyAsync();
                Console.WriteLine("Total Algorand Supply: " + supply.TotalMoney);
                Console.WriteLine("Online Algorand Supply: " + supply.OnlineMoney);
 
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling algod#getSupply:" + e.Message);
            }

            var accountInfo = await algodApiInstance.AccountsAsync(src.Address.ToString(),null);
            Console.WriteLine(string.Format("Account Balance: {0} microAlgos", accountInfo.Amount));

            try
            {
                var trans = await algodApiInstance.ParamsAsync();
                var lr = trans.LastRound;
                var block = await algodApiInstance.BlocksAsync(lr,null);
                
                Console.WriteLine("Lastround: " + trans.LastRound.ToString());
                Console.WriteLine("Block txns: " + block.Block.ToString());
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                Console.WriteLine("Exception when calling algod#getSupply:" + e.Message);
            }

            TransactionParametersResponse transParams;
            try
            {
                transParams = await algodApiInstance.ParamsAsync();                
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                throw new Exception("Could not get params", e);
            }
            var amount = Utils.AlgosToMicroalgos(1);
            var tx = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR), amount, "pay message", transParams);
            var signedTx = src.SignTransaction(tx);

            Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

            // send the transaction to the network
            try
            {
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
                var resp = await Utils.WaitTransactionToComplete(algodApiInstance, id.TxId);
                Console.WriteLine("Confirmed Round is: " + resp.ConfirmedRound);
            }
            catch (Algorand.V2.Algod.Model.ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }
    }
}
