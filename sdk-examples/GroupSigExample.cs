using Algorand;
using Algorand.Client;
using Algorand.Algod.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace sdk_examples
{
    class GroupSigExample
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
            string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
            string DEST_ADDR2 = "OAMCXDCH7LIVYUF2HSNQLPENI2ZXCWBSOLUAOITT47E4FAMFGAMI4NFLYU";
            Account src = new Account(SRC_ACCOUNT);
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
            // let's create a transaction group
            var amount = Utils.AlgosToMicroalgos(1);
            var tx = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR), amount, "pay message", transParams);
            var tx2 = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR2), amount, "pay message", transParams);
            //SignedTransaction signedTx2 = src.SignTransactionWithFeePerByte(tx2, feePerByte);
            Digest gid = TxGroup.ComputeGroupID(new Transaction[] { tx, tx2 });
            tx.AssignGroupID(gid);
            tx2.AssignGroupID(gid);
            // already updated the groupid, sign
            var signedTx = src.SignTransaction(tx);
            var signedTx2 = src.SignTransaction(tx2);
            try
            {
                //contact the signed msgpack
                List<byte> byteList = new List<byte>(Algorand.Encoder.EncodeToMsgPack(signedTx));
                byteList.AddRange(Algorand.Encoder.EncodeToMsgPack(signedTx2));
                var id = algodApiInstance.RawTransaction(byteList.ToArray());
                Console.WriteLine("Successfully sent tx group with first tx id: " + id);
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
