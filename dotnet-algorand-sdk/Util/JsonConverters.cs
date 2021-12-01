using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using System;

namespace Algorand
{
    public class BytesConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => true;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return (typeof(Signature) == objectType || typeof(Digest) == objectType || typeof(Address) == objectType ||
                typeof(VRFPublicKey) == objectType || typeof(ParticipationPublicKey) == objectType ||
                typeof(Ed25519PublicKeyParameters) == objectType || typeof(TEALProgram) == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(Address))
            {
                byte[] bytes;

                switch (reader.Value)
                {
                    case byte[] b:
                    {
                        bytes = b;
                        break;
                    }
                    case string s:
                    {
                        bytes = Convert.FromBase64String(s);
                        break;
                    }
                    default:
                        bytes = null;
                        break;
                }
                
                if (bytes != null && bytes.Length > 0) return new Address(bytes);
                else return new Address();
            } else
                return new object();
            //throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            byte[] bytes = null;
            if (value is Address)
            {
                var adr = value as Address;
                bytes = adr.Bytes;
            }else if (value is Signature)
            {
                var sig = value as Signature;
                bytes = sig.Bytes;
            }
            else if (value is Digest)
            {
                var dig = value as Digest;
                bytes = dig.Bytes;
            }else if(value is VRFPublicKey)
            {
                var vrf = value as VRFPublicKey;
                bytes = vrf.Bytes;
            }else if(value is ParticipationPublicKey)
            {
                var ppk = value as ParticipationPublicKey;
                bytes = ppk.Bytes;
            }else if(value is Ed25519PublicKeyParameters)
            {
                var key = value as Ed25519PublicKeyParameters;
                bytes = key.GetEncoded();
            }else if(value is TEALProgram)
            {
                var program = value as TEALProgram;
                bytes = program.Bytes;
            }
            //writer.WriteValue(Convert.ToBase64String(bytes));
            writer.WriteValue(bytes);
        }
    }
    public class MultisigAddressConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => false;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(MultisigAddress) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            MultisigAddress mAddress = (MultisigAddress)value;
            writer.WriteStartObject();
            writer.WritePropertyName("version");
            writer.WriteValue(mAddress.version);
            writer.WritePropertyName("threshold");
            writer.WriteValue(mAddress.threshold);
            writer.WritePropertyName("publicKeys");
            writer.WriteStartArray();
            foreach (var item in mAddress.publicKeys)
                writer.WriteValue(item.GetEncoded());
            writer.WriteEnd();
            writer.WriteEndObject();
            //writer.WriteValue(mAddress.publicKeys);
            //base.WriteJson(writer, value, serializer);
            //writer.WriteValue(Convert.ToBase64String(bytes));
            //writer.WriteValue(bytes);
        }
    }
    public class Type2StringConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => true;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return (typeof(Transaction.Type) == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (typeof(Transaction.Type) == objectType)
                return new Transaction.Type(reader.Value.ToString());
            return new object();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //if (value is Address)
            //{
            //    var adr = value as Address;
            //    bytes = adr.Bytes;
            //}
            var type = value as Transaction.Type;
            writer.WriteValue(type.ToValue());
        }

    }
}
