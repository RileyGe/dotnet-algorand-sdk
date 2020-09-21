using Algorand.V2;
using System;

namespace sdk_examples.V2
{
    public class IndexerExamples
    {
        public static void Main(string[] args)
        {
            string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/idx2";
            string ALGOD_API_TOKEN = "GeHdp7CCGt7ApLuPNppXN4LtrW07Mm1kaFNJ5Ovr";

            IndexerApi indexer = new IndexerApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            //AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            var health = indexer.MakeHealthCheck();
            Console.WriteLine("Make Health Check: " + health.ToJson());
            
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var address = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            var acctInfo = indexer.LookupAccountByID(address);
            Console.WriteLine("Look up account by id: " + acctInfo.ToJson());
            
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var transInfos = indexer.LookupAccountTransactions(address, 10);
            Console.WriteLine("Look up account transactions(limit 10): " + transInfos.ToJson());

            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var appsInfo = indexer.SearchForApplications(limit: 10);
            Console.WriteLine("Search for application(limit 10): " + appsInfo.ToJson());

            var appIndex = appsInfo.Applications[0].Id;
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var appInfo = indexer.LookupApplicationByID(appIndex);
            Console.WriteLine("Look up application by id: " + appInfo.ToJson());

            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var assetsInfo = indexer.SearchForAssets(limit: 10, unit: "LAT");
            Console.WriteLine("Search for assets" + assetsInfo.ToJson());

            var assetIndex = assetsInfo.Assets[0].Index;
            System.Threading.Thread.Sleep(1200); //test in purestake, imit 1 req/sec
            var assetInfo = indexer.LookupAssetByID(assetIndex);
            Console.WriteLine("Look up asset by id:" + assetInfo.ToJson());            

            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exist.");
            Console.ReadKey();
        }
    }
}
