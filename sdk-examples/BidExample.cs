using Algorand;
using Algorand.Client;
using Algorand.Algod.Api;
using System;

namespace sdk_examples
{
    class BidExample
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
            string AUCTION_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            //string DEST_ADDR2 = "OAMCXDCH7LIVYUF2HSNQLPENI2ZXCWBSOLUAOITT47E4FAMFGAMI4NFLYU";
            Account bidder = new Account(SRC_ACCOUNT);
            var algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);
            Algorand.Algod.Model.TransactionParams transParams = null;
            try
            {
                transParams = algodApiInstance.TransactionParams();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }
            // format and send logic sig
            Bid bid = new Bid(bidder.Address, new Address(AUCTION_ADDR), 1000, 10, 2, 56);
            var signedBid = bidder.SignBid(bid);
            var tx = Utils.GetBidTransaction(bidder.Address, new Address(AUCTION_ADDR), signedBid, transParams);
            var signedTx = bidder.SignTransaction(tx);
            try
            {
                var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
                Console.WriteLine("Successfully sent tx logic sig tx id: " + id);
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
