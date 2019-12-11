using System;

namespace Algorand
{
    /**
 * A serializable class representing a SHA512-256 Digest
 */
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    public class Digest
    {
        private static int DIG_LEN_BYTES = 32;
        public byte[] Bytes { get; private set; }

        /**
         * Create a new digest.
         * @param bytes a length 32 byte array.
         */
        //@JsonCreator
        public Digest(byte[] bytes)
        {
            if (bytes == null)
            {
                return;
            }
            if (bytes.Length != DIG_LEN_BYTES)
            {
                throw new ArgumentException("digest wrong length");
            }
            //if (this.Bytes is null) this.Bytes = new byte[bytes.Length];
            //JavaHelper<byte>.SyatemArrayCopy(bytes, 0, this.Bytes, 0, DIG_LEN_BYTES);
            Bytes = bytes;
        }

        // default values for serializer to ignore
        public Digest()
        {
            Bytes = new byte[DIG_LEN_BYTES];
        }

        //@JsonValue
        //public byte[] getBytes()
        //{
        //    return this.bytes;
        //}

        //@Override
        public override bool Equals(object obj)
        {
            if (obj is Digest && this.Bytes.Equals(((Digest)obj).Bytes)) {
                return true;
            } else {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return this.Bytes.GetHashCode();
        }
    }
}