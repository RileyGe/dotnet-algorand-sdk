using Algorand;
using Algorand.Client;
using Algorand.V2;
using System;

namespace sdk_examples.V2.contract
{
    class ContractAccount
    {
        public static void Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args[1];
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            Account src = new Account(SRC_ACCOUNT);
            var algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            Algorand.V2.Model.TransactionParametersResponse transParams;
            try
            {
                transParams = algodApiInstance.TransactionParams();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }
            // format and send logic sig
            byte[] program = Convert.FromBase64String("ASABASI=");

            LogicsigSignature lsig = new LogicsigSignature(program, null);
            Console.WriteLine("Escrow address: " + lsig.Address.ToString());

            var tx = Utils.GetPaymentTransaction(lsig.Address, src.Address, 100000, "draw algo from contract", transParams);
          
            if (!lsig.Verify(tx.sender))
            {
                string msg = "Verification failed";
                Console.WriteLine(msg);
            }
            else
            {
                try
                {
                    SignedTransaction signedTx = Account.SignLogicsigTransaction(lsig, tx);
                    var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                    Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
                }
                catch (ApiException e)
                {
                    // This is generally expected, but should give us an informative error message.
                    Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
                }
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}
