using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text;

namespace Algorand
{
    /// <summary>
    /// A raw serializable signature class.
    /// </summary>
    [JsonConverter(typeof(BytesConverter))]
    public class Signature
    {
        private static int ED25519_SIG_SIZE = 64;
        
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Create a new Signature wrapping the given bytes.
        /// </summary>
        /// <param name="rawBytes">bytes</param>
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
        }

        /// <summary>
        /// default values for serializer to ignore
        /// </summary>
        public Signature()
        {
            Bytes = new byte[ED25519_SIG_SIZE];
        }

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
    }
    /// <summary>
    /// Serializable logicsig class. 
    /// LogicsigSignature is constructed from a program and optional arguments. 
    /// Signature sig and MultisigSignature msig property are available for modification by it's clients.
    /// </summary>
    [JsonObject]
    public class LogicsigSignature
    {
        [JsonIgnore]
        private static byte[] LOGIC_PREFIX = Encoding.UTF8.GetBytes("Program");//.getBytes(StandardCharsets.UTF_8);

        [JsonProperty(PropertyName = "l")]
        public byte[] logic;

        [JsonProperty(PropertyName = "arg")]
        public List<byte[]> args;

        [JsonProperty(PropertyName = "sig")]
        public Signature sig;

        [JsonProperty(PropertyName = "msig")]
        public MultisigSignature msig;

        /// <summary>
        /// LogicsigSignature
        /// </summary>
        /// <param name="logic">Unsigned logicsig object</param>
        /// <param name="args">Unsigned logicsig object's arguments</param>
        /// <param name="sig"></param>
        /// <param name="msig"></param>
        [JsonConstructor]
        public LogicsigSignature(
            [JsonProperty("l")] byte[] logic,
            [JsonProperty("arg")] List<byte[]> args = null,
            [JsonProperty("sig")] byte[] sig = null,
            [JsonProperty("msig")] MultisigSignature msig = null)
        {
            this.logic = JavaHelper<byte[]>.RequireNotNull(logic, "program must not be null");
            this.args = args;

            if (!Logic.CheckProgram(this.logic, this.args))
                throw new Exception("program verified failed!");

            if (sig != null)
            {
                this.sig = new Signature(sig);
            }
            this.msig = msig;
        }
        /// <summary>
        /// Uninitialized object used for serializer to ignore default values.
        /// </summary>
        public LogicsigSignature()
        {
            this.logic = null;
            this.args = null;
        }
        /// <summary>
        /// alculate escrow address from logic sig program
        /// DEPRECATED
        /// Please use Address property.
        /// The address of the LogicsigSignature
        /// </summary>
        /// <returns>The address of the LogicsigSignature</returns>
        public Address ToAddress()
        {
            return Address;
        }
        /// <summary>
        /// The address of the LogicsigSignature
        /// </summary>
        [JsonIgnore]
        public Address Address { 
            get {
                return new Address(Digester.Digest(BytesToSign()));
            } 
        }
        /// <summary>
        /// Return prefixed program as byte array that is ready to sign
        /// </summary>
        /// <returns>byte array</returns>
        public byte[] BytesToSign()
        {
            List<byte> prefixedEncoded = new List<byte>(LOGIC_PREFIX);
            prefixedEncoded.AddRange(this.logic);
            return prefixedEncoded.ToArray();
        }
        /// <summary>
        /// Perform signature verification against the sender address
        /// </summary>
        /// <param name="address">Address to verify</param>
        /// <returns>bool</returns>
        public bool Verify(Address address)
        {
            if (this.logic == null)
            {
                return false;
            }
            else if (this.sig != null && this.msig != null)
            {
                return false;
            }
            else
            {
                try
                {
                    Logic.CheckProgram(this.logic, this.args);
                }
                catch (Exception)
                {
                    return false;
                }

                if (this.sig == null && this.msig == null)
                {
                    try
                    {
                        return address.Equals(this.ToAddress());
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }                
                
                if (this.sig != null)
                {
                    try
                    {
                        var pk = new Ed25519PublicKeyParameters(address.Bytes, 0);
                        var signer = new Ed25519Signer();
                        signer.Init(false, pk); //false代表用于VerifySignature
                        signer.BlockUpdate(this.BytesToSign(), 0, this.BytesToSign().Length);
                        return signer.VerifySignature(this.sig.Bytes);
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("Message = " + err.Message);
                        return false;
                    }
                }
                else
                {
                    return this.msig.Verify(this.BytesToSign());
                }
                
            }
        }

        private static bool NullCheck(object o1, object o2)
        {
            if (o1 == null && o2 == null)
            {
                return true;
            }
            else if (o1 == null && o2 != null)
            {
                return false;
            }
            else
            {
                return o1 == null || o2 != null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is LogicsigSignature actual)
                if((this.logic is null && actual.logic is null) || (!(this.logic is null || actual.logic is null) && Enumerable.SequenceEqual(this.logic, actual.logic)))                
                    if ((this.sig is null && actual.sig is null) || this.sig.Equals(actual.sig))
                        if ((this.msig is null && actual.msig is null) || this.msig.Equals(actual.msig))
                            if ((this.args is null && actual.args is null) || ArgsEqual(this.args, actual.args))
                                return true;
            return false;            
        }

        public override int GetHashCode()
        {
            return this.logic.GetHashCode() + this.args.GetHashCode() + this.sig.GetHashCode() + this.msig.GetHashCode();
        }

        private static bool ArgsEqual(List<byte[]> args1, List<byte[]> args2)
        {
            bool flag = true;
            if (args1.Count == args2.Count)
            {
                for (int i = 0; i < args1.Count; i++)
                {
                    if (!Enumerable.SequenceEqual(args1[i], args2[i]))
                    {
                        flag = false;
                        break;
                    }
                }
            }
            else
                flag = false;
            return flag;
        }
    }
    /// <summary>
    /// Serializable raw multisig class.
    /// </summary>
    [JsonObject]
    public class MultisigSignature
    {
        [JsonProperty(PropertyName = "v")]
        public int version;

        [JsonProperty(PropertyName = "thr")]
        public int threshold;

        [JsonProperty(PropertyName = "subsig")]
        public List<MultisigSubsig> subsigs;
        /// <summary>
        /// create a multisig signature.
        /// </summary>
        /// <param name="version">required</param>
        /// <param name="threshold">required</param>
        /// <param name="subsigs">can be empty or null</param>
        [JsonConstructor]
        public MultisigSignature(
            [JsonProperty(PropertyName = "v")] int version, 
            [JsonProperty(PropertyName = "thr")] int threshold,
            [JsonProperty(PropertyName = "subsig")] List<MultisigSubsig> subsigs = null)
        {
            this.version = version;
            this.threshold = threshold;
            if (subsigs is null)
                this.subsigs = new List<MultisigSubsig>();
            else
                this.subsigs = subsigs;
        }

        public MultisigSignature()
        {
            this.subsigs = new List<MultisigSubsig>();
        }
        /// <summary>
        /// Performs signature verification
        /// </summary>
        /// <param name="message">raw message to verify</param>
        /// <returns>bool</returns>
        public bool Verify(byte[] message)
        {
            if (this.version == 1 && this.threshold > 0 && this.subsigs.Count != 0)
            {
                if (this.threshold > this.subsigs.Count)
                {
                    return false;
                }
                else
                {
                    int verifiedCount = 0;
                    Signature emptySig = new Signature();

                    for (int i = 0; i < this.subsigs.Count; ++i)
                    {
                        MultisigSubsig subsig = subsigs[i];
                        if (!subsig.sig.Equals(emptySig))
                        {
                            try
                            {
                                var pk = subsig.key;
                                var signer = new Ed25519Signer();
                                signer.Init(false, pk); //for verify
                                signer.BlockUpdate(message, 0, message.Length);
                                bool verified = signer.VerifySignature(subsig.sig.Bytes);
                                if (verified)
                                {
                                    ++verifiedCount;
                                }
                            }
                            catch (Exception var9)
                            {
                                throw new ArgumentException("verification of subsig " + i + "failed", var9);
                            }
                        }
                    }

                    if (verifiedCount < this.threshold)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is MultisigSignature actual)
            {
                if (this.version == actual.version && this.threshold == actual.threshold && Enumerable.SequenceEqual(this.subsigs, actual.subsigs))
                    return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return this.version.GetHashCode() + this.threshold.GetHashCode() + this.subsigs.GetHashCode();
        }
    }
    /// <summary>
    /// Serializable multisig sub-signature
    /// </summary>
    [JsonObject]
    public class MultisigSubsig
    {
        [JsonProperty(PropertyName = "pk")]
        public Ed25519PublicKeyParameters key;

        [JsonProperty(PropertyName = "s")]
        public Signature sig;
        /// <summary>
        /// workaround wrapped json values
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sig"></param>
        [JsonConstructor]
        public MultisigSubsig([JsonProperty("pk")] byte[] key = null, [JsonProperty("s")] byte[] sig = null)
        {
            if (key != null)
                this.key = new Ed25519PublicKeyParameters(key, 0);
            else
                this.key = new Ed25519PublicKeyParameters(new byte[0], 0);

            if (sig != null)
                this.sig = new Signature(sig);
            else
                this.sig = new Signature();
        }

        public MultisigSubsig(Ed25519PublicKeyParameters key, Signature sig = null)
        {
            this.key = JavaHelper<Ed25519PublicKeyParameters>.RequireNotNull(key, "public key cannot be null");
            if (sig is null)
                this.sig = new Signature();
            else
                this.sig = sig;
        }

        public override bool Equals(object obj)
        {
            if ((obj is MultisigSubsig actual))
            {
                return Enumerable.SequenceEqual(this.key.GetEncoded(), actual.key.GetEncoded()) && this.sig.Equals(actual.sig);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return key.GetHashCode() + sig.GetHashCode();
        }
    }
}