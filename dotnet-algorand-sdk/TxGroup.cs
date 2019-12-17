using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    [JsonObject]
    public class TxGroup
    {
        private static byte[] TG_PREFIX = Encoding.UTF8.GetBytes("TG");
        [JsonProperty(PropertyName = "txlist")]
        private Digest[] txGroupHashes;

        public static Digest ComputeGroupID(Transaction[] txns)
        {
            if (txns != null && txns.Length != 0)
            {
                Digest[] txIDs = new Digest[txns.Length];

                for (int i = 0; i < txns.Length; ++i)
                {
                    txIDs[i] = txns[i].RawTxID();
                }

                TxGroup txgroup = new TxGroup(txIDs);
                byte[] gid = Digester.Digest(txgroup.BytesToSign());
                return new Digest(gid);
            }
            else
            {
                throw new ArgumentException("empty transaction list");
            }
        }

        public static Transaction[] AssignGroupID(Transaction[] txns, Address address)
        {
            Digest gid = ComputeGroupID(txns);
            List<Transaction> result = new List<Transaction>();

            for (int i = 0; i < txns.Length; ++i) {
                if (address == null || address.ToString() == "" || address == txns[i].sender) {
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

        private byte[] BytesToSign() {
            byte[] encodedTx = Encoder.EncodeToMsgPack(this);
            byte[] prefixEncodedTx = JavaHelper<byte>.ArrayCopyOf(TG_PREFIX, TG_PREFIX.Length + encodedTx.Length);
            JavaHelper<byte>.SyatemArrayCopy(encodedTx, 0, prefixEncodedTx, TG_PREFIX.Length, encodedTx.Length);
            return prefixEncodedTx;
        }
    }
}
