using System;
using Algorand.V2;
using Account = Algorand.Account;

namespace sdk_examples.V2
{
    class AccountTest
    {
        public static void Main(string[] args)
        {
            string ALGOD_API_ADDR = args[0];
            if (ALGOD_API_ADDR.IndexOf("//") == -1)
            {
                ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
            }

            string ALGOD_API_TOKEN = args.Length > 1 ? args[1] : null;
            string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
            Account src = new Account(SRC_ACCOUNT);
            Console.WriteLine("My account address is:" + src.Address.ToString());

            AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);            

            var accountInfo = algodApiInstance.AccountInformation(src.Address.ToString());
            Console.WriteLine(string.Format("Account Balance: {0} microAlgos", accountInfo.Amount));

            var task = algodApiInstance.AccountInformationAsync(src.Address.ToString());
            task.Wait();
            var asyncAccountInfo = task.Result;
            Console.WriteLine(string.Format("Account Balance(Async): {0} microAlgos", asyncAccountInfo.Amount));
            
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }
    }
}
