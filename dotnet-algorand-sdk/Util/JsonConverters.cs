using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

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
                typeof(Ed25519PublicKeyParameters) == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType == typeof(Address))
            {
                var bytes = (byte[])reader.Value;
                return new Address(bytes);
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
            }
            //writer.WriteValue(Convert.ToBase64String(bytes));
            writer.WriteValue(bytes);
        }
    }
    public class Type2StringConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => false;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return (typeof(Type) == objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
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
