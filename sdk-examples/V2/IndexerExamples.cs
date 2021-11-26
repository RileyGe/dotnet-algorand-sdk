using Algorand.V2;
using Algorand.V2.Indexer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace sdk_examples.V2
{
    public class IndexerExamples
    {
        public  async Task Main(string[] args)
        {
            string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/idx2";
            string ALGOD_API_TOKEN = "GeHdp7CCGt7ApLuPNppXN4LtrW07Mm1kaFNJ5Ovr";

            var httpClient = HttpClientConfigurator.ConfigureHttpClient(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            LookupApi lookupApi = new LookupApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            SearchApi searchApi = new SearchApi(httpClient) { BaseUrl = ALGOD_API_ADDR };
            CommonApi commonApi = new CommonApi(httpClient) { BaseUrl = ALGOD_API_ADDR };

            //AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            var health = await commonApi.HealthAsync();
            Console.WriteLine("Make Health Check: " + health.ToJson());
            
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var address = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            var acctInfo = await lookupApi.AccountsAsync(address,null,null);
            Console.WriteLine("Look up account by id: " + acctInfo.ToJson());
            
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var transInfos = await lookupApi.TransactionsGetAsync( 10,null,null,null,null, address,null, null,null,null,null,null,null,null,null,null);
            Console.WriteLine("Look up account transactions(limit 10): " + transInfos.ToJson());

            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var appsInfo = await searchApi.ApplicationsAsync(null,null, 10,null);
            Console.WriteLine("Search for application(limit 10): " + appsInfo.ToJson());

            var appIndex = appsInfo.Applications.FirstOrDefault()?.Id;
            if (appIndex != null)
            {
                System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
                var appInfo = await lookupApi.ApplicationsAsync(appIndex.Value, null);
                Console.WriteLine("Look up application by id: " + appInfo.ToJson());
            }

            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var assetsInfo = await searchApi.AssetsAsync(null, 10,null,null,null, "LAT",null);
            Console.WriteLine("Search for assets" + assetsInfo.ToJson());

            var assetIndex = assetsInfo.Assets.FirstOrDefault().Index;
           
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var assetInfo = await lookupApi.AssetsAsync(assetIndex,null);
            Console.WriteLine("Look up asset by id:" + assetInfo.ToJson());
            
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}
