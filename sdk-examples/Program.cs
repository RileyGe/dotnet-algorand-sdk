using System;
using Algorand;
using Account = Algorand.Account;
using Algorand.Algod.Api;
using Algorand.Algod.Model;
using Algorand.Client;
using System.Text;

namespace sdk_examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var act = new Account();
            var act2 = Account.AccountFromPrivateKey(act.GetClearTextPrivateKey());
            if (act.Address.ToString() == act2.Address.ToString()) Console.WriteLine("Success!");
            return;
            //V2.BasicExample.Main(args); return;
            //V2.AssetExample.Main(args); return;
            //V2.AtomicTransferExample.Main(args); return;
            //V2.contract.CompileTeal.Main(args); return;
            //V2.contract.ContractAccount.Main(args); return;
            //V2.contract.LogicSignature.Main(args); return;
            //V2.contract.DryrunDedugging.Main(args); return;
            //V2.IndexerExamples.Main(args); return;
            //V2.RekeyExample.Main(args); return;
            //V2.AccountTest.Main(args); return;
            //V2.contract.DryrunStatefulExample.Main(args); return;
            //V2.contract.StatefulContract.Main(args); return;


            //AssetExample.Main(args); return;
            //BidExample.Main(args); return;
            //GroupSigExample.Main(args); return;
            //LogicSigExample.Main(args); return;

            //MultisigExample.Main(args); return;

            // the SDK also support purestake, just use the two lines below replace the line 28~32
            //string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps1";
            //string ALGOD_API_TOKEN = "YOUR API PURESTAKE KEY";
            // If the protocol is not specified in the address, http is added.
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args[1];
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            string DEST_ADDR = "7XVBE6T6FMUR6TI2XGSVSOPJHKQE2SDVPMFA3QUZNWM7IY6D4K2L23ZN2A";

            if (!Address.IsValid(DEST_ADDR))
                Console.WriteLine("The address " + DEST_ADDR + " is not valid!");
            Account src = new Account(SRC_ACCOUNT);
            Console.WriteLine("My account address is:" + src.Address.ToString());
            if(src.ToMnemonic() != SRC_ACCOUNT)
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

            try
            {
                TransactionParams trans = algodApiInstance.TransactionParams();
                var lr = (long?)trans.LastRound;
                Block block = algodApiInstance.GetBlock((long?)trans.LastRound);
                Console.WriteLine("Lastround: " + trans.LastRound.ToString());
                Console.WriteLine("Block txns: " + block.Txns.ToJson());
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
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
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
