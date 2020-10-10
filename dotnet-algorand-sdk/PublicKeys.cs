using System;
using System.Linq;
using Newtonsoft.Json;

namespace Algorand
{
    /// <summary>
    /// A serializable class representing a participation key.
    /// </summary>
    [JsonConverter(typeof(BytesConverter))]
    public class ParticipationPublicKey
    {
        private const int KEY_LEN_BYTES = 32;
        public byte[] Bytes { get; private set; }
        /// <summary>
        /// Create a new participation key
        /// </summary>
        /// <param name="bytes">a length 32 byte array</param>
        [JsonConstructor]
        public ParticipationPublicKey(byte[] bytes)
        {
            if (bytes != null)
            {
                if (bytes.Length != KEY_LEN_BYTES)
                {
                    throw new ArgumentException("participation key wrong length");
                }
                else
                {
                    this.Bytes = bytes;
                }
            }else this.Bytes = new byte[KEY_LEN_BYTES];

        }
        /// <summary>
        /// default values for serializer to ignore
        /// </summary>
        public ParticipationPublicKey()
        {
            this.Bytes = new byte[KEY_LEN_BYTES];
        }

        public override bool Equals(object obj)
        {
            return obj is ParticipationPublicKey && Enumerable.SequenceEqual(this.Bytes, ((ParticipationPublicKey)obj).Bytes);
        }
        public override int GetHashCode()
        {
            return this.Bytes.GetHashCode();
        }
    }

    [JsonConverter(typeof(BytesConverter))]
    public class VRFPublicKey
    {
        private const int KEY_LEN_BYTES = 32;
        public byte[] Bytes { get; private set; }

        [JsonConstructor]
        public VRFPublicKey(byte[] bytes)
        {
            if (bytes != null)
            {
                if (bytes.Length != KEY_LEN_BYTES)
                {
                    throw new ArgumentException("vrf key wrong length");
                }
                else
                {
                    this.Bytes = bytes;
                }
            }
            else
                Bytes = new byte[KEY_LEN_BYTES];
        }

        public VRFPublicKey()
        {
            Bytes = new byte[KEY_LEN_BYTES];
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