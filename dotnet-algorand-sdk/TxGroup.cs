using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    /// <summary>
    /// TxGroup exports computeGroupID and assignGroupID functions
    /// </summary>
    [JsonObject]
    public class TxGroup
    {
        public static int MAX_TX_GROUP_SIZE = 16;
        private static byte[] TG_PREFIX = Encoding.UTF8.GetBytes("TG");
        [JsonProperty(PropertyName = "txlist")]
        private Digest[] txGroupHashes;
        /// <summary>
        /// Compute group ID for a group of unsigned transactions
        /// </summary>
        /// <param name="txns">array of transactions</param>
        /// <returns>Digest</returns>
        public static Digest ComputeGroupID(params Transaction[] txns)
        {
            if (txns != null && txns.Length > 0)
            {
                if (txns.Length > MAX_TX_GROUP_SIZE)                
                    throw new ArgumentException("max group size is " + MAX_TX_GROUP_SIZE);
                
                Digest[] txIDs = new Digest[txns.Length];
                for (int i = 0; i < txns.Length; ++i)                
                    txIDs[i] = txns[i].RawTxID();
                

                TxGroup txgroup = new TxGroup(txIDs);
                try
                {
                    byte[] gid = Digester.Digest(txgroup.BytesToSign());
                    return new Digest(gid);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("tx computation failed", e);
                }
            }
            else            
                throw new ArgumentException("empty transaction list");            
        }

        /// <summary>
        /// Assigns group id to a given array of unsigned transactions
        /// </summary>
        /// <param name="txns">array of transactions</param>
        /// <returns>array of grouped transactions, optionally filtered with the address parameter.</returns>
        public static Transaction[] AssignGroupID(params Transaction[] txns)
        {
            return AssignGroupID(txns, null);
        }

        /// <summary>
        /// Assigns group id to a given array of unsigned transactions
        /// </summary>
        /// <param name="address">optional sender address specifying which transaction return</param>
        /// <param name="txns">array of transactions</param>
        /// <returns>array of grouped transactions, optionally filtered with the address parameter.</returns>
        public static Transaction[] AssignGroupID(Address address, params Transaction[] txns)
        {
            return AssignGroupID(txns, address);
        }
        /// <summary>
        /// Assigns group id to a given array of unsigned transactions
        /// Deprecated use assignGroupID(address, params Transaction txns)
        /// </summary>
        /// <param name="txns">rray of transactions</param>
        /// <param name="address">optional sender address specifying which transaction return</param>
        /// <returns>array of grouped transactions, optionally filtered with the address parameter.</returns>
        public static Transaction[] AssignGroupID(Transaction[] txns, Address address)
        {
            Digest gid = ComputeGroupID(txns);
            List<Transaction> result = new List<Transaction>();

            for (int i = 0; i < txns.Length; ++i)
            {
                if (address == null || address.ToString() == "" || address == txns[i].sender)
                {
                    Transaction tx = txns[i];
                    tx.AssignGroupID(gid);
                    result.Add(tx);
                }
            }
            return result.ToArray();
        }

        [JsonConstructor]
        private TxGroup([JsonProperty(PropertyName = "txlist")] Digest[] txGroupHashes) {
            this.txGroupHashes = txGroupHashes;
        }
        /// <summary>
        /// Return encoded representation of the transaction with a prefix suitable for signing
        /// </summary>
        /// <returns>bytes</returns>
        private byte[] BytesToSign() {
            byte[] encodedTx = Encoder.EncodeToMsgPack(this);
            byte[] prefixEncodedTx = JavaHelper<byte>.ArrayCopyOf(TG_PREFIX, TG_PREFIX.Length + encodedTx.Length);
            JavaHelper<byte>.SystemArrayCopy(encodedTx, 0, prefixEncodedTx, TG_PREFIX.Length, encodedTx.Length);
            return prefixEncodedTx;
        }
    }
}
