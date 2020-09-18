using System;
using Algorand;
using Account = Algorand.Account;
using Algorand.Algod.Api;
using Algorand.Algod.Model;
using Algorand.Client;
using System.Text;

namespace sdk_examples
{
    public class PureStakeExample
    {
        public static void Main(string[] args)
        {
            // If the protocol is not specified in the address, http is added.
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
            if (src.ToMnemonic() != SRC_ACCOUNT)
            {
                Console.WriteLine("ToMnemonic function is wriong!");
            }

            //sign and verify bytes function test
            var bytes = Encoding.UTF8.GetBytes("examples");
            var siguture = src.SignBytes(bytes);

            Address srcAddr = new Address(src.Address.ToString());
            var verifyed = srcAddr.VerifyBytes(bytes, siguture);

            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

            try
            {
                Supply supply = algodApiInstance.GetSupply();
                Console.WriteLine("Total Algorand Supply: " + supply.TotalMoney);
                Console.WriteLine("Online Algorand Supply: " + supply.OnlineMoney);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Exception when calling algod#getSupply:" + e.Message);
            }

            TransactionParams transParams = null;
            try
            {
                transParams = algodApiInstance.TransactionParams();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }
            var amount = Utils.AlgosToMicroalgos(1);
            var tx = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR), amount, "pay message", transParams);
            //Transaction tx = new Transaction(src.Address, new Address(DEST_ADDR), amount, firstRound, lastRound, genesisID, genesisHash);
            var signedTx = src.SignTransaction(tx);

            Console.WriteLine("Signed transaction with txid: " + signedTx.transactionID);

            // send the transaction to the network
            try
            {
                var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Successfully sent tx with id: " + id.TxId);
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}
