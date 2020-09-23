using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Msgpack;
using Org.BouncyCastle.Crypto.Parameters;

namespace Algorand
{
    public class Encoder
    {
        //private static char BASE32_PAD_CHAR = '=';
        /**
         * Convenience method for serializing arbitrary objects.
         * @return serialized object
         * @throws JsonProcessingException if serialization failed
         */
        public static byte[] EncodeToMsgPack(object o)
        {
            //ObjectMapper objectMapper = new ObjectMapper(new MessagePackFactory());
            //// It is important to sort fields alphabetically to match the Algorand canonical encoding
            //var settings = new JsonSerializerSettings()
            //{
            //    ContractResolver = AlgorandContractResolver.Instance,
            //    DefaultValueHandling = DefaultValueHandling.Ignore
            //};
            //objectMapper.configure(MapperFeature.SORT_PROPERTIES_ALPHABETICALLY, true);
            //objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, true);
            //// There's some odd bug in Jackson < 2.8.? where null values are not excluded. See:
            //// https://github.com/FasterXML/jackson-databind/issues/1351. So we will
            //// also annotate all fields manually
            //objectMapper.setSerializationInclusion(JsonInclude.Include.NON_DEFAULT);

            //MemoryStream ms = new MemoryStream();
            //using (var writer = new BsonDataWriter(ms))
            //{
            //    JsonSerializer serializer = new JsonSerializer
            //    {
            //        DefaultValueHandling = DefaultValueHandling.Ignore,
            //        ContractResolver = AlgorandContractResolver.Instance,
            //        Formatting = Formatting.None
            //    };
            //    serializer.Serialize(writer, o);
            //}
            //string data = Convert.ToBase64String(ms.ToArray());
            //MessagePack.MessagePackSerializerOptions options = 
            //    MessagePack.MessagePackSerializerOptions.Standard.

            MemoryStream memoryStream = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ContractResolver = AlgorandContractResolver.Instance,
                Formatting = Formatting.None
            };

            // serialize product to MessagePack
            MessagePackWriter writer = new MessagePackWriter(memoryStream);
            serializer.Serialize(writer, o);
            var bytes = memoryStream.ToArray();
            //Console.WriteLine(BitConverter.ToString(memoryStream.ToArray()));
            //var jstr = EncodeToJson(o);
            //var newObj = JsonConvert.DeserializeObject(jstr);
            //var bytes = MessagePack.MessagePackSerializer.ConvertFromJson(EncodeToJson(o));
            return bytes;
        }

        /**
         * Convenience method for deserializing arbitrary objects encoded with canonical msg-pack
         * @param input byte array representing canonical msg-pack encoding
         * @param tClass class of type of object to deserialize as
         * @param <T> object type
         * @return deserialized object
         * @throws IOException if decoding failed
         */
        public static T DecodeFromMsgPack<T>(byte[] input)
        {
            // See encodedToMsgPack for explanation of settings, and how this makes msgpack canonical
            //ObjectMapper objectMapper = new ObjectMapper(new MessagePackFactory());
            //    objectMapper.configure(MapperFeature.SORT_PROPERTIES_ALPHABETICALLY, true);
            //    objectMapper.configure(DeserializationFeature.FAIL_ON_UNKNOWN_PROPERTIES, true);
            //    objectMapper.setSerializationInclusion(JsonInclude.Include.NON_DEFAULT);
            //    return objectMapper.readValue(input, tClass);
            // deserialize product from MessagePack
            MemoryStream memoryStream = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();
            MessagePackReader reader = new MessagePackReader(memoryStream);
            T deserializedT = serializer.Deserialize<T>(reader);
            return deserializedT;
        }

        /**
         * Encode an object as json.
         * @param o object to encode
         * @return json string
         * @throws JsonProcessingException error
         */
        public static string EncodeToJson(object o)
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = AlgorandContractResolver.Instance,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var ostr = JsonConvert.SerializeObject(o, settings);
            return ostr;
        }

        /// <summary>
        /// Convenience method for writing bytes as hex.
        /// </summary>
        /// <param name="bytes">bytes input to encodeToMsgPack as hex string</param>
        /// <returns>encoded hex string</returns>
        public static string EncodeToHexStr(byte[] bytes)
        {
            return BitConverter.ToString(bytes, 0).Replace("-", string.Empty).ToLower();
        }

        ///**
        // * Convenience method for decoding bytes from hex.
        // * @param hexStr hex string to decode
        // * @return byte array
        // * @throws DecoderException
        // */
        //public static byte[] decodeFromHexStr(String hexStr) throws DecoderException
        //{
        //        return Hex.decodeHex(hexStr);
        //}

        /**
         * Convenience method for writing bytes as base32
         * @param bytes input
         * @return base32 string with stripped whitespace
         */
        //public static string EncodeToBase32StripPad(byte[] bytes)
        //{
        //    //Base32 codec = new Base32((byte)BASE32_PAD_CHAR);            
        //    string paddedStr = Base32.ToBase32String(bytes, true);
        //    // strip padding
        //    //int i = 0;
        //    //for (; i < paddedStr.Length; i++)
        //    //{
        //    //    if (paddedStr[i] == BASE32_PAD_CHAR)
        //    //    {
        //    //        break;
        //    //    }
        //    //}
        //    int i = paddedStr.IndexOf(BASE32_PAD_CHAR);
        //    return paddedStr.Substring(0, i);
        //}

        ///**
        // * Encode to base64 string. Does not strip padding.
        // * @param bytes input
        // * @return base64 string with appropriate padding
        // */
        //public static String encodeToBase64(byte[] bytes)
        //{
        //    Base64 codec = new Base64();
        //    return codec.encodeToString(bytes);
        //}

        ///**
        // * Decode from base64 string.
        // * @param str input
        // * @return decoded bytes
        // */
        //public static byte[] decodeFromBase64(String str)
        //{
        //    Base64 codec = new Base64();
        //    return codec.decode(str);
        //}
    }

    public class AlgorandContractResolver : DefaultContractResolver
    {
        public static readonly AlgorandContractResolver Instance = new AlgorandContractResolver();
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
            //JsonProperty property = null;
            //var itemList = ret.FindAll(p => p.DeclaringType == typeof(LogicsigSignature));
            //if (itemList.Count > 0)
            //    itemList.ForEach(item =>
            //    {
            //        item.ShouldSerialize = instance => {
            //            var lsig = instance as LogicsigSignature;
            //            return !lsig.Equals(new LogicsigSignature());
            //        };
            //    });                
            //return ret;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);            
            if (property.DeclaringType == typeof(SignedTransaction))
            {                
                if(property.PropertyType == typeof(LogicsigSignature) && property.PropertyName == "lsig")
                {
                    property.ShouldSerialize =  instance =>
                    {
                        var st = instance as SignedTransaction;
                        return !st.lSig.Equals(new LogicsigSignature());
                    };
                }
                else if (property.PropertyType == typeof(MultisigSignature) && property.PropertyName == "msig")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var st = instance as SignedTransaction;
                        return !st.mSig.Equals(new MultisigSignature());
                    };
                }
                else if (property.PropertyType == typeof(Transaction) && property.PropertyName == "txn")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var st = instance as SignedTransaction;
                        return !st.tx.Equals(new Transaction());
                    };
                }
                else if (property.PropertyType == typeof(Signature) && property.PropertyName == "sig")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var st = instance as SignedTransaction;
                        return !st.sig.Equals(new Signature());
                    };
                }
            }
            else if(property.DeclaringType == typeof(Transaction))
            {
                if(property.PropertyType == typeof(Address) && property.PropertyName == "snd")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.sender.Equals(new Address());
                    };
                }else if(property.PropertyType == typeof(byte[]) && property.PropertyName == "note")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;                        
                        return !(trans.note == null || trans.note.Length == 0);
                    };
                }else if(property.PropertyType == typeof(Digest) && property.PropertyName == "gh")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;                        
                        return !trans.genesisHash.Equals(new Digest());
                    };
                }else if(property.PropertyType == typeof(Digest) && property.PropertyName == "grp")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;                        
                        return !trans.group.Equals(new Digest());
                    };
                }else if(property.PropertyType == typeof(byte[]) && property.PropertyName == "lx")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;                        
                        return !(trans.lease == null || trans.lease.Length == 0);
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "rcv")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.receiver.Equals(new Address());
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "close")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.closeRemainderTo.Equals(new Address());
                    };
                }else if(property.PropertyType == typeof(ParticipationPublicKey) && property.PropertyName == "votekey")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.votePK.Equals(new ParticipationPublicKey());
                    };
                }else if(property.PropertyType == typeof(VRFPublicKey) && property.PropertyName == "selkey")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.selectionPK.Equals(new VRFPublicKey());
                    };
                }else if(property.PropertyType == typeof(Transaction.AssetParams) && property.PropertyName == "apar")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.assetParams.Equals(new Transaction.AssetParams());
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "asnd")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.assetSender.Equals(new Address());
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "arcv")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.assetReceiver.Equals(new Address());
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "aclose")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !(trans.assetCloseTo is null || trans.assetCloseTo.Equals(new Address()));
                    };
                }else if(property.PropertyType == typeof(Address) && property.PropertyName == "fadd")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var trans = instance as Transaction;
                        return !trans.freezeTarget.Equals(new Address());
                    };
                }
            }
            else if(property.DeclaringType == typeof(MultisigSignature))
            {
                if(property.PropertyType == typeof(List<MultisigSubsig>) &&
                    property.PropertyName == "subsig")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var msig = instance as MultisigSignature;
                        return !(msig.subsigs == null || msig.subsigs.Count == 0);
                    };
                    

                }
            }
            else if(property.DeclaringType == typeof(LogicsigSignature))
            {
                if(property.PropertyType == typeof(byte[]) && property.PropertyName == "l")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var lsig = instance as LogicsigSignature;
                        return !(lsig.logic == null || lsig.logic.Length == 0);
                    };
                }else if(property.PropertyType == typeof(List<byte[]>) && property.PropertyName == "arg")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var lsig = instance as LogicsigSignature;
                        return !(lsig.args == null || lsig.args.Count == 0);
                    };
                }else if(property.PropertyType == typeof(MultisigSignature) && property.PropertyName == "msig")
                {
                    property.ShouldSerialize = instance =>
                    {
                        var lsig = instance as LogicsigSignature;
                        return !(lsig.msig is null || lsig.msig.Equals(new MultisigSignature()));
                    };
                }
            }
            else if(property.DeclaringType == typeof(MultisigSubsig))
            {
                if (property.PropertyName == "pk" && property.PropertyType == typeof(Ed25519PublicKeyParameters))
                {
                    property.Converter = new BytesConverter();
                    //int i = 0;
                }else if (property.PropertyName == "s" && property.PropertyType == typeof(Signature))
                {
                    property.ShouldSerialize = instance =>
                    {
                        var msig = instance as MultisigSubsig;
                        return !(msig.sig.Equals(new Signature()));
                    };
                }
            }
            return property;
        }
    }
}
