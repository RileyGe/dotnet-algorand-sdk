using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Algorand
{
    /**
 * MultisigAddress is a convenience class for handling multisignature public identities.
 */
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    [JsonObject]
    public class MultisigAddress
    {
        //public const int KEY_LEN_BYTES = 32;
        public int version;
        public int threshold;
        public List<Ed25519PublicKeyParameters> publicKeys = new List<Ed25519PublicKeyParameters>();

        private static readonly byte[] PREFIX = Encoding.UTF8.GetBytes("MultisigAddr");//.getBytes(StandardCharsets.UTF_8);

        public MultisigAddress(int version, int threshold,
                List<Ed25519PublicKeyParameters> publicKeys)
        {
            this.version = version;
            this.threshold = threshold;
            this.publicKeys.AddRange(publicKeys);

            if (this.version != 1)
            {
                throw new ArgumentException("Unknown msig version");
            }

            if (
                this.threshold == 0 ||
                this.publicKeys.Count == 0 ||
                this.threshold > this.publicKeys.Count
            )
            {
                throw new ArgumentException("Invalid threshold");
            }
        }

        // building an address object helps us generate string representations
        public Address ToAddress()
        {            
            //int numPkBytes = KEY_LEN_BYTES * this.publicKeys.Count;
            //byte[] hashable = new byte[PREFIX.Length + 2 + numPkBytes];
            List<byte> hashable = new List<byte>(PREFIX);
            //System.arraycopy(PREFIX, 0, hashable, 0, PREFIX.Length);
            hashable.Add(Convert.ToByte(this.version));
            //hashable[PREFIX.Length] = (byte)this.version;
            hashable.Add(Convert.ToByte(this.threshold));
            //hashable[PREFIX.Length + 1] = (byte)this.threshold;
            //for (int i = 0; i < this.publicKeys.Count; i++)
            //{
            //    System.arraycopy(
            //            this.publicKeys[i].getBytes(),
            //            0,
            //            hashable,
            //            PREFIX.length + 2 + i * Ed25519PublicKey.KEY_LEN_BYTES,
            //            Ed25519PublicKey.KEY_LEN_BYTES
            //    );
            //}
            foreach(var key in publicKeys)            
                hashable.AddRange(key.GetEncoded());
            
            return new Address(Digester.Digest(hashable.ToArray()));
        }

        //@Override
        public override string ToString()
        {
            //try
            //{
            return this.ToAddress().ToString();
            //}
            //catch (NoSuchAlgorithmException e)
            //{
            //    throw new RuntimeException(e);
            //}
        }
    }
}
