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
    [JsonConverter(typeof(Signature2BytesConverter))]
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

    public class Signature2BytesConverter : JsonConverter
    {
        //是否开启自定义反序列化，值为true时，反序列化时会走ReadJson方法，值为false时，不走ReadJson方法，而是默认的反序列化
        public override bool CanRead => false;
        //是否开启自定义序列化，值为true时，序列化时会走WriteJson方法，值为false时，不走WriteJson方法，而是默认的序列化
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(Signature) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //new一个JObject对象,JObject可以像操作对象来操作json
            //var jobj = new JObject();
            //value参数实际上是你要序列化的Model对象，所以此处直接强转
            var sig = value as Signature;
            //if (model.ID != 1)
            //{
            //    //如果ID值为1，添加一个键位"ID"，值为false
            //    jobj.Add("ID", false);
            //}
            //else
            //{
            //    jobj.Add("ID", true);
            //}
            //通过ToString()方法把JObject对象转换成json
            //var jsonstr = jobj.ToString();
            //调用该方法，把json放进去，最终序列化Model对象的json就是jsonstr，由此，我们就能自定义的序列化对象了
            writer.WriteValue(Convert.ToBase64String(sig.Bytes));
        }
    }
}