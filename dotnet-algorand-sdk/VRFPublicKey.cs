using Newtonsoft.Json;
using System;
using System.Linq;

namespace Algorand
{
    //@JsonInclude(Include.NON_DEFAULT)
    [JsonConverter(typeof(BytesConverter))]
    public class VRFPublicKey
    {
        private const int KEY_LEN_BYTES = 32;
        public byte[] Bytes { get; private set; }

        //@JsonCreator
        [JsonConstructor]
        public VRFPublicKey(byte[] bytes)
        {
            if (bytes != null)
            {
                if (bytes.Length != 32)
                {
                    throw new ArgumentException("vrf key wrong length");
                }
                else
                {
                    this.Bytes = bytes;
                }
            }
        }

        public VRFPublicKey()
        {
            Bytes = new byte[32];
        }
        public override bool Equals(Object obj)
        {
            return (obj is VRFPublicKey) && Enumerable.SequenceEqual(this.Bytes, ((VRFPublicKey)obj).Bytes);
        }
        public override int GetHashCode()
        {
            return this.Bytes.GetHashCode();
        }
    }
}