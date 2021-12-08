using System;
using System.Threading.Tasks;
using Algorand.V2;
using Algorand.V2.Algod;
using Account = Algorand.Account;

namespace sdk_examples.V2
{
    class AccountTest
    {
        public async Task Main(string[] args)
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
            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            DefaultApi algodApiInstance = new DefaultApi(httpClient) { BaseUrl = ALGOD_API_ADDR };

            var accountInfo = await algodApiInstance.AccountsAsync(src.Address.ToString(),null);
            Console.WriteLine(string.Format("Account Balance: {0} microAlgos", accountInfo.Amount));

            
            
            
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
        }
    }
}
