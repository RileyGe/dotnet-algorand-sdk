using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Algorand
{
    //    @JsonPropertyOrder(    alphabetic = true)
    //@JsonInclude(Include.NON_DEFAULT)
    [JsonObject]
    public class LogicsigSignature
    {
        //@JsonIgnore
        [JsonIgnore]
        private static byte[] LOGIC_PREFIX = Encoding.UTF8.GetBytes("Program");//.getBytes(StandardCharsets.UTF_8);
        //@JsonIgnore
        //[JsonIgnore]
        //private const string SIGN_ALGO = "EdDSA";
        //@JsonProperty("l")
        [JsonProperty(PropertyName = "l")]
        public byte[] logic;
        //@JsonProperty("arg")
        [JsonProperty(PropertyName = "arg")]
        public List<byte[]> args;
        //@JsonProperty("sig")
        [JsonProperty(PropertyName = "sig")]
        public Signature sig;
        //@JsonProperty("msig")
        [JsonProperty(PropertyName = "msig")]
        public MultisigSignature msig;

        //@JsonCreator
        [JsonConstructor]
        public LogicsigSignature(
            [JsonProperty("l")] byte[] logic, 
            [JsonProperty("arg")] List<byte[]> args, 
            [JsonProperty("sig")] byte[] sig = null, 
            [JsonProperty("msig")] MultisigSignature msig = null) {
            this.logic = JavaHelper<byte[]>.RequireNotNull(logic, "program must not be null");
            this.args = args;
            bool verified = false;

            //try {
            verified = Logic.CheckProgram(this.logic, this.args);
            //} catch (IOException var7) {
            //    throw new IllegalArgumentException("invalid program", var7);
            //}
            if (!verified)
                throw new Exception("program verified failed!");
            //assert verified;

            if (sig != null) {
                this.sig = new Signature(sig);
            }
            this.msig = msig;
        }

        //public LogicsigSignature(byte[] logic, List<byte[]> args)
        //{
        //    this(logic, args, (byte[])null, (MultisigSignature)null);
        //}

        public LogicsigSignature()
        {
            this.logic = null;
            this.args = null;
        }

        public Address ToAddress()
        {
            byte[] prefixedEncoded = this.BytesToSign();
            return new Address(Digester.Digest(prefixedEncoded));
        }

        public byte[] BytesToSign()
        {
            byte[] prefixedEncoded = new byte[this.logic.Length + LOGIC_PREFIX.Length];
            JavaHelper<byte>.SyatemArrayCopy(LOGIC_PREFIX, 0, prefixedEncoded, 0, LOGIC_PREFIX.Length);
            JavaHelper<byte>.SyatemArrayCopy(this.logic, 0, prefixedEncoded, LOGIC_PREFIX.Length, this.logic.Length);
            return prefixedEncoded;
        }

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
                else
                {
                    //PublicKey pk;
                    //try
                    //{
                    //    pk = address.ToVerifyKey();
                    //}
                    //catch (Exception)
                    //{
                    //    return false;
                    //}

                    if (this.sig != null)
                    {
                        try
                        {
                            var pk = new Ed25519PublicKeyParameters(address.Bytes, 0);
                            // prepend the message prefix
                            //byte[] prefixBytes = new byte[message.Length + BYTES_SIGN_PREFIX.Length];
                            //JavaHelper<byte>.SyatemArrayCopy(BYTES_SIGN_PREFIX, 0, prefixBytes, 0, BYTES_SIGN_PREFIX.Length);
                            //JavaHelper<byte>.SyatemArrayCopy(message, 0, prefixBytes, BYTES_SIGN_PREFIX.Length, message.Length);
                            // verify signature
                            // Generate new signature

                            var signer = new Ed25519Signer();
                            signer.Init(true, pk);
                            signer.BlockUpdate(this.BytesToSign(), 0, this.BytesToSign().Length);
                            return signer.VerifySignature(this.sig.Bytes);


                            //java.security.Signature sig = java.security.Signature.getInstance("EdDSA");
                            //sig.initVerify(pk);
                            //sig.update(this.bytesToSign());
                            //return sig.verify(this.sig.getBytes());
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return this.msig.Verify(this.BytesToSign());
                    }
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
            {
                if (
                    ((this.logic is null && actual.logic is null) || Enumerable.SequenceEqual(this.logic, actual.logic)) &&
                    ((this.sig is null && actual.sig is null) || this.sig.Equals(actual.sig)) &&
                    ((this.msig is null && actual.msig is null) || this.msig.Equals(actual.msig)) &&
                    ((this.args is null && actual.args is null) || ArgsEqual(this.args, actual.args)))
                    return true;
                else return false;
                //if ((this.logic == null && actual.logic != null) || (this.logic != null && actual.logic == null))
                //{
                //    return false;
                //}
                //else if (this.logic != null && actual != null && (!Enumerable.SequenceEqual(this.logic, actual.logic)))
                //{
                //    return false;
                //}


                //else if (!nullCheck(this.args, actual.args))
                //{
                //    return false;
                //}
                //else
                //{
                //    if (this.args != null)
                //    {
                //        if (this.args.size() != actual.args.size())
                //        {
                //            return false;
                //        }

                //        for (int i = 0; i < this.args.size(); ++i)
                //        {
                //            if (!Arrays.equals((byte[])this.args.get(i), (byte[])actual.args.get(i)))
                //            {
                //                return false;
                //            }
                //        }
                //    }

                //    if (!nullCheck(this.sig, actual.sig))
                //    {
                //        return false;
                //    }
                //    else if (this.sig != null && !this.sig.equals(actual.sig))
                //    {
                //        return false;
                //    }
                //    else if (!nullCheck(this.msig, actual.msig))
                //    {
                //        return false;
                //    }
                //    else
                //    {
                //        return this.msig == null || this.msig.equals(actual.msig);
                //    }
                //}
            }
            else
            {
                return false;
            }
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
                    if(!Enumerable.SequenceEqual(args1[i], args2[i]))
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

        //static {
        //        LOGIC_PREFIX = "Program".getBytes(StandardCharsets.UTF_8);
        //    }
    }
}