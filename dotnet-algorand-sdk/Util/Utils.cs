using Algorand.Algod.Client.Api;
using Algorand.Algod.Client.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    public class Utils
    {
        public static string WaitTransactionToComplete(AlgodApi instance, string txID) //throws Exception
        {
            while (true)
            {
                //try {
                //Check the pending tranactions
                Algod.Client.Model.Transaction b3 = instance.PendingTransactionInformation(txID);
                //algodApiInstance.p
                if (b3.Round != null && b3.Round > 0)
                {
                    //Got the completed Transaction
                    return "Transaction " + b3.Tx + " confirmed in round " + b3.Round;
                }
                //} catch (Exception e) {
                //    throw (e);
                //}
            }
        }
        public static TransactionID SubmitTransaction(AlgodApi instance, SignedTransaction signedTx) //throws Exception
        {
            //try {
            // Msgpack encode the signed transaction
            byte[] encodedTxBytes = Encoder.EncodeToMsgPack(signedTx);             
            return instance.RawTransaction(encodedTxBytes);
            //} catch (ApiException e) {
            //    throw (e);
            //}
        }
        public static ulong AlgosToMicroalgos(double algos)
        {
            return Convert.ToUInt64(Math.Floor(algos * 1000000));
        }
        public static double MicroalgosToAlgos(ulong microAlgos)
        {
            return microAlgos / 1000000.0;
        }
    }
}
