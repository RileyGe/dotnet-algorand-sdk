using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorand
{
    //@JsonPropertyOrder(alphabetic = true)
    //@JsonInclude(Include.NON_DEFAULT)
    [JsonObject]
    public class MultisigSignature
    {
        //private const string SIGN_ALGO = "EdDSA";
        //private const int MULTISIG_VERSION = 1;
        //@JsonProperty("v")
        [JsonProperty(PropertyName = "v")]
        public int version;
        //@JsonProperty("thr")
        [JsonProperty(PropertyName = "thr")]
        public int threshold;
        //@JsonProperty("subsig")
        [JsonProperty(PropertyName = "subsig")]
        public List<MultisigSubsig> subsigs;

        //@JsonCreator
        [JsonConstructor]
        public MultisigSignature([JsonProperty(PropertyName = "v")] int version, [JsonProperty(PropertyName = "thr")] int threshold,
            [JsonProperty(PropertyName = "subsig")] List<MultisigSubsig> subsigs = null) {
            //this.subsigs = new List<MultisigSubsig>();
            this.version = version;
            this.threshold = threshold;
            //this.subsigs = (List)Objects.requireNonNull(subsigs, );
            if (subsigs is null)
                this.subsigs = new List<MultisigSubsig>();
            else
                this.subsigs = subsigs;
        }

        //public MultisigSignature(int version, int threshold): this(version, threshold, new List<MultisigSubsig>())
        //{
            
        //}

        public MultisigSignature()
        {
            this.subsigs = new List<MultisigSubsig>();
        }

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
                                signer.Init(true, pk);
                                signer.BlockUpdate(message, 0, message.Length);
                                bool verified = signer.VerifySignature(subsig.sig.Bytes);

                                //PublicKey pk = (new Address(subsig.key.getBytes())).toVerifyKey();
                                //java.security.Signature sig = java.security.Signature.getInstance("EdDSA");
                                //sig.initVerify(pk);
                                //sig.update(message);
                                //boolean verified = sig.verify(subsig.sig.getBytes());
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
            if (obj is MultisigSignature actual) {
                //MultisigSignature actual = (MultisigSignature)obj;
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
    //@JsonPropertyOrder(
    //    alphabetic = true
    //)
    [JsonObject]
    public class MultisigSubsig
    {
        //@JsonProperty("pk")
        [JsonProperty(PropertyName = "pk")]
        public Ed25519PublicKeyParameters key;
        //@JsonProperty("s")
        [JsonProperty(PropertyName = "s")]
        public Signature sig;

        //@JsonCreator
        [JsonConstructor]
        public MultisigSubsig([JsonProperty("pk")] byte[] key = null, [JsonProperty("s")] byte[] sig = null)
        {
            //this.key = new Ed25519PublicKeyParameters(key, 0);
            //this.sig = new Signature();
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
            //this.key = new Ed25519PublicKey();
            //this.sig = new Signature();
            this.key = JavaHelper<Ed25519PublicKeyParameters>.RequireNotNull(key, "public key cannot be null");
            if (sig is null)
                this.sig = new Signature();
            else
                this.sig = sig;
        }

        //public MultisigSubsig(Ed25519PublicKey key)
        //{
        //    this(key, new Signature());
        //}

        //public MultisigSubsig()
        //{
        //    this.key = new Ed25519PublicKey();
        //    this.sig = new Signature();
        //}

        public override bool Equals(object obj)
        {
            if ((obj is MultisigSubsig actual))
            {
                return Enumerable.SequenceEqual(this.key.GetEncoded(), actual.key.GetEncoded()) && this.sig.Equals(actual.sig);
            }
            else
            {
                //MultisigSignature.MultisigSubsig actual = (MultisigSignature.MultisigSubsig)obj;
                return false;
            }
        }
        public override int GetHashCode()
        {
            return key.GetHashCode() + sig.GetHashCode();
        }
    }
}