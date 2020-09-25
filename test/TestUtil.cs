using System;
using System.Collections.Generic;
using NUnit.Framework;
using Encoder = Algorand.Encoder;

namespace test
{
    public class TestUtil
    {
        public static void SerializeDeserializeCheck<T>(T obj)
        {
            JsonSerializeTests<T>(obj);
            MessagePackSerializeTests<T>(obj);
        }

        private static void JsonSerializeTests<T>(T obj)
        {
            string encoded, encoded2;
            T decoded, decoded2;
            try
            {
                encoded = Encoder.EncodeToJson(obj);
                decoded = Encoder.DecodeFromJson<T>(encoded);
                Assert.AreEqual(decoded, obj);

                encoded2 = Encoder.EncodeToJson(decoded);
                Assert.AreEqual(encoded2, encoded);

                decoded2 = Encoder.DecodeFromJson<T>(encoded2);
                Assert.AreEqual(decoded2, decoded);
            }
            catch (Exception e)
            {
                Assert.Fail("Should not have thrown an exception.", e);
            }
        }

        private static void MessagePackSerializeTests<T>(T obj)
        {
            string encoded, encoded2;
            object decoded, decoded2;
            try
            {
                encoded = Convert.ToBase64String(Encoder.EncodeToMsgPack(obj));
                decoded = Encoder.DecodeFromMsgPack<T>(Convert.FromBase64String(encoded));
                Assert.AreEqual(decoded, obj);

                encoded2 = Convert.ToBase64String(Encoder.EncodeToMsgPack(decoded));
                Assert.AreEqual(encoded2, encoded);

                decoded2 = Encoder.DecodeFromMsgPack<T>(Convert.FromBase64String(encoded2));
                Assert.AreEqual(decoded2, decoded);
            }
            catch (Exception e)
            {
                Assert.Fail("Should not have thrown an exception.", e);
            }
        }
        
        public static void ContainsExactlyElementsOf<T>(List<T> superList, List<T> subList)
        {
            CollectionAssert.IsSubsetOf(subList, superList);
            List<int> orderList = new List<int>();
            foreach(var item in subList)
            {
                orderList.Add(superList.IndexOf(item));
            }
            CollectionAssert.IsOrdered(orderList);
        }
    }
}
