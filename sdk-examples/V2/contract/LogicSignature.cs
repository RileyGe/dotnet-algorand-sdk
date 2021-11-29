using Algorand;
using Algorand.Client;
using Algorand.V2;
using Algorand.V2.Algod;
using System;
using System.Threading.Tasks;

namespace sdk_examples.V2.contract
{
    class LogicSignature
    {
        public async Task Main(params string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }
            string ALGOD_API_TOKEN = args[1];
            //第一个账号用于给智能合约签名，并把签名发布出去
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            Account acct1 = new Account(SRC_ACCOUNT);            
            byte[] program = Convert.FromBase64String("ASABASI="); //int 1
            //byte[] program = Convert.FromBase64String("ASABACI="); //int 0
            

            LogicsigSignature lsig = new LogicsigSignature(program, null);

            // sign the logic signaure with an account sk
            // 这里操作的意义是账号1批准逻辑签名可以操纵我的账号
            acct1.SignLogicsig(lsig);
            var contractSig = Convert.ToBase64String(lsig.sig.Bytes);
            var acct1Address = acct1.Address.ToString();

            //第二步，另一个账号，只用contractSig就可以对acct1中进行符合智能合约逻辑的操作
            //注：此例中是全部返回1,也就是说任何操作，这在实际生产中是非常危险的操作，请注意
            //注：也需要用到acct1的地址（公钥）

            //实际上，在本例中并不需要acct2的私钥，只要有公钥就足够了
            //string acct2_mnemonic = "place blouse sad pigeon wing warrior wild script"
            //                   + " problem team blouse camp soldier breeze twist mother"
            //                   + " vanish public glass code arrow execute convince ability"
            //                   + " there";
            //Account acct2 = new Account(acct2_mnemonic);
            var acct2Address = "AJNNFQN7DSR7QEY766V7JDG35OPM53ZSNF7CU264AWOOUGSZBMLMSKCRIU";

            //为了表示与账号1全部脱离，所以新建一个LogicsigSignature
            LogicsigSignature lsig2 = new LogicsigSignature(program, null, Convert.FromBase64String(contractSig));
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

            Transaction tx = Utils.GetPaymentTransaction(new Address(acct1Address), new Address(acct2Address), 1000000, 
                "draw algo with logic signature", transParams);            
            
            try
            {
                //bypass verify for non-lsig
                SignedTransaction signedTx = Account.SignLogicsigTransaction(lsig2, tx);
                
                var id = await Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
                Console.WriteLine("Confirmed Round is: " +
                        Utils.WaitTransactionToComplete(algodApiInstance, id.TxId).Result.ConfirmedRound);
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
