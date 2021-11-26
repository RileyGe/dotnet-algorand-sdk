using Algorand;
using Algorand.Client;
using Algorand.V2;
using Algorand.V2.Algod;
using System;
using System.Threading.Tasks;

namespace sdk_examples.V2.contract
{
    class DryrunDedugging
    {
        public async Task Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
 
            string SRC_ACCOUNT = "buzz genre work meat fame favorite rookie stay tennis demand panic busy hedgehog snow morning acquire ball grain grape member blur armor foil ability seminar";
            Account acct1 = new Account(SRC_ACCOUNT);
            var acct2Address = "QUDVUXBX4Q3Y2H5K2AG3QWEOMY374WO62YNJFFGUTMOJ7FB74CMBKY6LPQ";

            //byte[] source = File.ReadAllBytes("V2\\contract\\sample.teal");
            byte[] program = Convert.FromBase64String("ASABASI=");

            LogicsigSignature lsig = new LogicsigSignature(program, null);

            // sign the logic signaure with an account sk
            acct1.SignLogicsig(lsig);

            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            Algorand.V2.Algod.Model.TransactionParametersResponse transParams;
            try
            {
                transParams = await algodApiInstance.ParamsAsync();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

            Transaction tx = Utils.GetPaymentTransaction(acct1.Address, new Address(acct2Address), 1000000,
                "tx using in dryrun", transParams);

            try
            {
                //bypass verify for non-lsig
                SignedTransaction signedTx = Account.SignLogicsigTransaction(lsig, tx);
                //一切准备就绪，本可以直接发送到网络，也可使得Dryrun的方法来进行调试
                //var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                //Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
                // dryrun source
                //var dryrunResponse = Utils.GetDryrunResponse(algodApiInstance, signedTx, source);                
                //Console.WriteLine("Dryrun compiled repsonse : " + dryrunResponse.ToJson()); // pretty print

                // dryrun logic sig transaction
                var dryrunResponse2 = await Utils.GetDryrunResponse(algodApiInstance, signedTx);                
                Console.WriteLine("Dryrun source repsonse : " + dryrunResponse2.ToJson()); // pretty print
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }

            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }
    }
}
