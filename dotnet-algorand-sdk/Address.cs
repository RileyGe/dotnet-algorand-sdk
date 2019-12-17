using System;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Algorand
{
    //@JsonInclude(JsonInclude.Include.NON_DEFAULT)
    /// 
    /// Address represents a serializable 32-byte length Algorand address.
    /// 
    [JsonConverter(typeof(BytesConverter))]
    public class Address
    {
        /**
         * The length of an address. Equal to the size of a SHA256 checksum.
         */
        public const int LEN_BYTES = 32;

        // the underlying bytes
        //private byte[] bytes = new byte[LEN_BYTES];
        public byte[] Bytes { get; private set; }
        // the length of checksum to append
        private const int CHECKSUM_LEN_BYTES = 4;
        // expected length of base32-encoded checksum-appended addresses
        private const int EXPECTED_STR_ENCODED_LEN = 58;
        // signature algorithm for verifying signature
        //private static String SIGN_ALGO = "EdDSA";
        //private static String KEY_ALGO = "Ed25519";
        // prefix for signing bytes
        private static byte[] BYTES_SIGN_PREFIX = Encoding.UTF8.GetBytes("MX");//.getBytes(StandardCharsets.UTF_8);


        /**
         * Create a new address from a byte array.
         * @param bytes array of 32 bytes
         */
        //@JsonCreator
        [JsonConstructor]
        public Address(byte[] bytes)
        {
            if (bytes == null)
            {
                return;
            }
            if (bytes.Length != LEN_BYTES)
            {
                //throw new IllegalArgumentException(String.format("Given address length is not %s", LEN_BYTES));
            }
            //System.arraycopy(bytes, 0, this.bytes, 0, LEN_BYTES);
            //bytes.CopyTo(this.bytes, 0);
            this.Bytes = bytes;

        }

        // default values for serializer to ignore
        public Address()
        {
            this.Bytes = new byte[LEN_BYTES];
        }

        /**
         * Get the underlying bytes wrapped by this Address.
         * @return 32 byte array
         */
        //@JsonValue
        //public byte[] GetBytes()
        //{
        //    //return Arrays.copyOf(bytes, bytes.length);
        //    return Bytes.Clone() as byte[];
        //}

        /**
         * Create a new address from an encoded string, (encoded by encodeAsString)
         * @param encodedAddr
         */
        public Address(string encodedAddr) //throws NoSuchAlgorithmException
        {
            //Objects.requireNonNull(encodedAddr, "address must not be null");
            if (encodedAddr == null)
            {
                throw new Exception("address must not be null");
            }
            if (IsValid(encodedAddr))
            {
                this.Bytes = GetAdressBytes(encodedAddr);
            }
            else
            {
                throw new ArgumentException("The address is not valid");
            }
        }

        private byte[] GetAdressBytes(string encodedAddr)
        {
            byte[] checksumAddr = Base32.DecodeFromString(encodedAddr);
            return JavaHelper<byte>.ArrayCopyOf(checksumAddr, LEN_BYTES);
        }
        /// <summary>
        /// check if the address is valid
        /// </summary>
        /// <param name="encodedAddress">Address</param>
        /// <returns>valid or not</returns>
        public static bool IsValid(string encodedAddress)
        {
            // interpret as base32
            //Base32 codec = new Base32();
            //    Base32.DecodeFromBase32String
            byte[] checksumAddr = Base32.DecodeFromString(encodedAddress); // may expect padding
                                                                              // sanity check length
            if (checksumAddr.Length != LEN_BYTES + CHECKSUM_LEN_BYTES)
            {
                return false;
            }
            // split into checksum
            byte[] checksum = JavaHelper<byte>.ArrayCopyRange(checksumAddr, LEN_BYTES, checksumAddr.Length);
            //byte[] checksum = new byte[checksumAddr.Length - LEN_BYTES];
            //checksumAddr.CopyTo(checksum, LEN_BYTES);
            //byte[] checksum = Arrays.copyOfRange(checksumAddr, LEN_BYTES, checksumAddr.length);
            byte[] addr = JavaHelper<byte>.ArrayCopyOf(checksumAddr, LEN_BYTES);
            //checksumAddr.CopyTo(addr, 0); // truncates

            //SHA256Managed.Create()

            // compute expected checksum
            byte[] hashedAddr = Digester.Digest(addr);
            //byte[] expectedChecksum = Arrays.copyOfRange(hashedAddr, LEN_BYTES - CHECKSUM_LEN_BYTES, hashedAddr.length);
            byte[] expectedChecksum = JavaHelper<byte>.ArrayCopyRange(hashedAddr,
                LEN_BYTES - CHECKSUM_LEN_BYTES, hashedAddr.Length);


            // compare
            if (Enumerable.SequenceEqual(checksum, expectedChecksum))
            {
                //if (checksum == expectedChecksum) {
                //System.arraycopy(addr, 0, this.bytes, 0, LEN_BYTES);
                //addr.CopyTo(this.Bytes, 0);
                //this.Bytes = addr;
                return true;
            }
            else
            {
                return false;
            }
        }

        /**
         * EncodeAsString converts the address to a human-readable representation, with
         * a 4-byte checksum appended at the end, using SHA256. Note that string representations
         * of addresses generated by different SDKs may not be compatible.
         * @return
         */
        public string EncodeAsString()
        {
            // compute sha512/256 checksum
            byte[] hashedAddr = Digester.Digest(Bytes);

            // take the last 4 bytes, and append to addr           

            byte[] checksum = JavaHelper<byte>.ArrayCopyRange(hashedAddr, LEN_BYTES - CHECKSUM_LEN_BYTES, hashedAddr.Length);
            byte[] checksumAddr = JavaHelper<byte>.ArrayCopyOf(Bytes, Bytes.Length + CHECKSUM_LEN_BYTES);
            JavaHelper<byte>.SyatemArrayCopy(checksum, 0, checksumAddr, Bytes.Length, CHECKSUM_LEN_BYTES);

            // encodeToMsgPack addr+checksum as base32 and return. Strip padding.
            string res = Base32.EncodeToString(checksumAddr, false);
            if (res.Length != EXPECTED_STR_ENCODED_LEN)
            {
                throw new ArgumentException("unexpected address length " + res.Length);
            }
            return res;
        }

        /**
         * verifyBytes verifies that the signature for the message is valid for the public key. The message should have been prepended with "MX" when signing.
         * @param message the message that was signed
         * @param signature
         * @return boolean; true if the signature is valid
         */
        public bool VerifyBytes(byte[] message, Signature signature)
        {
            //var pk = this.ToVerifyKey();
            var pk = new Ed25519PublicKeyParameters(this.Bytes, 0);
            // prepend the message prefix
            List<byte> prefixBytes = new List<byte>(BYTES_SIGN_PREFIX);
            prefixBytes.AddRange(message);

            // verify signature
            // Generate new signature
            
            var signer = new Ed25519Signer();
            signer.Init(false, pk);
            signer.BlockUpdate(prefixBytes.ToArray(), 0, prefixBytes.ToArray().Length);
            return signer.VerifySignature(signature.Bytes);
            //byte[] signature = signer.GenerateSignature();
            //var actualSignature = Base64.getUrlEncoder().encodeToString(signature).replace("=", "");




            //java.security.Signature sig = java.security.Signature.getInstance(SIGN_ALGO);
            //sig.initVerify(pk);
            //sig.update(prefixBytes);
            //return sig.verify(signature.getBytes());
        }

        ///**
        // * toVerifyKey returns address' public key in a form suitable for
        // * java.security.Signature.initVerify
        // *
        // * @return PublicKey
        // * @throws InvalidKeySpecException
        // * @throws NoSuchAlgorithmException
        // */
        //public Ed25519PublicKeyParameters ToVerifyKey()
        //{
        //    //CryptoProvider.setupIfNeeded();
        //    //X509EncodedKeySpec pkS;
        //    //try
        //    //{
        //    // Wrap the public key in ASN.1 format.
        //    //SubjectPublicKeyInfo publicKeyInfo = new SubjectPublicKeyInfo(new AlgorithmIdentifier(EdECObjectIdentifiers.id_Ed25519), this.Bytes);
        //    //    pkS = new X509EncodedKeySpec(publicKeyInfo.getEncoded());
        //    //}
        //    //catch (IOException e)
        //    //{
        //    //    throw new RuntimeException("could not parse raw key bytes", e);
        //    //}

        //    //// create public key
        //    //KeyFactory kf = KeyFactory.getInstance(KEY_ALGO);
        //    //PublicKey pk = kf.generatePublic(pkS);
            
        //    var publicKey = new Ed25519PublicKeyParameters(this.Bytes, 0);

        //    return publicKey;
        //}


        public override string ToString()
        {
            return this.EncodeAsString();
        }


        public override bool Equals(object obj)
        {            
            if (obj is Address && Enumerable.SequenceEqual(this.Bytes, (obj as Address).Bytes))
                return true;
            else
                return false;

        }
        public override int GetHashCode()
        {
            return this.Bytes.GetHashCode();
        }

        // Compare to an address, with default address considered as empty string
        public bool CompareTo(string address)
        {
            if (this.Equals(new Address()))
            {
                return address == null || address.Length == 0;
            }
            return this.ToString().Equals(address);
        }
    }
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
            foreach (var key in publicKeys)
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
