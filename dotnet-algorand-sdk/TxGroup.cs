using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    //@JsonPropertyOrder(    alphabetic = true)
    //@JsonInclude(Include.NON_DEFAULT)
    [JsonObject]
    public class TxGroup
    {
        private static byte[] TG_PREFIX = Encoding.UTF8.GetBytes("TG");// (StandardCharsets.UTF_8);;
        //@JsonProperty("txlist")
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

                //try {
                byte[] gid = Digester.Digest(txgroup.BytesToSign());
                return new Digest(gid);
                //} catch (NoSuchAlgorithmException var4) {
                //    throw new RuntimeException("tx computation failed", var4);
                //}
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
            //return (Transaction[])result.toArray(new Transaction[result.size()]);
        }

        //@JsonCreator
        [JsonConstructor]
        private TxGroup([JsonProperty(PropertyName = "txlist")] Digest[] txGroupHashes) {
            this.txGroupHashes = txGroupHashes;
        }

        private byte[] BytesToSign() {
            //try {
            byte[] encodedTx = Encoder.EncodeToMsgPack(this);
            byte[] prefixEncodedTx = JavaHelper<byte>.ArrayCopyOf(TG_PREFIX, TG_PREFIX.Length + encodedTx.Length);
            JavaHelper<byte>.SyatemArrayCopy(encodedTx, 0, prefixEncodedTx, TG_PREFIX.Length, encodedTx.Length);
            return prefixEncodedTx;
            //} catch (IOException var3) {
            //    throw new RuntimeException("serialization failed", var3);
            //}
        }
    }
}
