using Algorand;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace test
{
    [TestFixture]
    public class AddressTests
    {
        [Test]
        public void testEncodeDecodeStr()
        {
            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                byte[] randKey = new byte[32];
                r.NextBytes(randKey);
                Address addr = new Address(randKey);
                string addrStr = addr.EncodeAsString();
                Address reencAddr = new Address(addrStr);
                Assert.AreEqual(reencAddr, addr);
            }
        }

        [Test]
        public void testGoldenValues()
        {
            string golden = "7777777777777777777777777777777777777777777777777774MSJUVU";
            byte[] bytes = new byte[32];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)0xff; // careful with signedness
            }
            Assert.AreEqual(new Address(bytes).ToString(), golden);
        }

        [Test]
        public void testAddressSerializable()
        {
            Address a = new Address("VKM6KSCTDHEM6KGEAMSYCNEGIPFJMHDSEMIRAQLK76CJDIRMMDHKAIRMFQ");
            MemoryStream ms = new MemoryStream();
            //创建序列化的实例
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, a);//序列化对象，写入ms流中  
            byte[] outBytes = ms.GetBuffer();

            ms = new MemoryStream(outBytes)
            {
                Position = 0
            };
            formatter = new BinaryFormatter();
            Address o = (Address)formatter.Deserialize(ms);//把内存流反序列成对象
            
            Assert.AreEqual(o, a);
            Assert.AreEqual("VKM6KSCTDHEM6KGEAMSYCNEGIPFJMHDSEMIRAQLK76CJDIRMMDHKAIRMFQ", o.EncodeAsString());
        }
    }
}
