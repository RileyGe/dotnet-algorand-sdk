using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Linq;

namespace Algorand
{
    /**
 * A raw serializable signature class.
 */
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    [JsonConverter(typeof(BytesConverter))]
    public class Signature
    {
        private static int ED25519_SIG_SIZE = 64;
        
        public byte[] Bytes { get; private set; }

        /**
         * Create a new Signature wrapping the given bytes.
         */
        //@JsonCreator
        [JsonConstructor]
        public Signature(byte[] rawBytes)
        {
            if (rawBytes == null)
            {
                Bytes = new byte[ED25519_SIG_SIZE];
                return;
            }
            if (rawBytes.Length != ED25519_SIG_SIZE)
            {
                throw new ArgumentException(string.Format("Given signature length is not {0}", ED25519_SIG_SIZE));
            }
            this.Bytes = rawBytes;
            //JavaHelper<byte>.SyatemArrayCopy(rawBytes, 0, this.Bytes, 0, ED25519_SIG_SIZE);
        }

        // default values for serializer to ignore
        public Signature()
        {
            Bytes = new byte[ED25519_SIG_SIZE];
        }

        ////@JsonValue
        //public byte[] getBytes()
        //{
        //    return this.Bytes;
        //}

        //@Override
        public override bool Equals(object obj)
        {
            if (obj is Signature && Enumerable.SequenceEqual(this.Bytes, (obj as Signature).Bytes))
                return true;
            else return false;
        }
        public override int GetHashCode()
        {
            return Bytes.GetHashCode();
        }
        //public override string ToString()
        //{
        //    return base.ToString();
        //}
        //public static implicit operator byte[](Signature sig)
        //{
        //    return sig.Bytes;
        //}
        //public static implicit operator Signature(byte[] bytes)
        //{
        //    return new Signature(bytes);
        //}
    }    
}