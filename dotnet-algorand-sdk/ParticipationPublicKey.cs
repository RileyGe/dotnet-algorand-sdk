using System;
using System.Linq;
using Newtonsoft.Json;

namespace Algorand
{
    //public class ParticipationPublicKey
    //{
    //    private byte[] votePK;

    //    public ParticipationPublicKey()
    //    {
    //    }

    //    public ParticipationPublicKey(byte[] votePK)
    //    {
    //        this.votePK = votePK;
    //    }
    //}



    //@JsonInclude(Include.NON_DEFAULT)
    [JsonConverter(typeof(BytesConverter))]
    public class ParticipationPublicKey
    {
        private const int KEY_LEN_BYTES = 32;
        public byte[] Bytes { get; private set; }

        //@JsonCreator
        [JsonConstructor]
        public ParticipationPublicKey(byte[] bytes)
        {
            if (bytes != null)
            {
                if (bytes.Length != 32)
                {
                    throw new ArgumentException("participation key wrong length");
                }
                else
                {
                    this.Bytes = bytes;
                }
            }
        }

        public ParticipationPublicKey()
        {
            this.Bytes = new byte[32];
        }

        //@JsonValue
        //public byte[] getBytes()
        //{
        //    return this.bytes;
        //}

        public override bool Equals(object obj)
        {
            return obj is ParticipationPublicKey && Enumerable.SequenceEqual(this.Bytes, ((ParticipationPublicKey)obj).Bytes);
        }
        public override int GetHashCode()
        {
            return this.Bytes.GetHashCode();
        }
    }
}