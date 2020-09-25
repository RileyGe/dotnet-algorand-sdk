using Algorand;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using System.Collections.Generic;

namespace test
{
    [TestFixture]
    public class TestMultisigAddress
    {
        [Test]
        public void TestToString()
        {
            Address one = new Address("XMHLMNAVJIMAW2RHJXLXKKK4G3J3U6VONNO3BTAQYVDC3MHTGDP3J5OCRU");
            Address two = new Address("HTNOX33OCQI2JCOLZ2IRM3BC2WZ6JUILSLEORBPFI6W7GU5Q4ZW6LINHLA");
            Address three = new Address("E6JSNTY4PVCY3IRZ6XEDHEO6VIHCQ5KGXCIQKFQCMB2N6HXRY4IB43VSHI");

            MultisigAddress addr = new MultisigAddress(1, 2, new List<Ed25519PublicKeyParameters>
            {
                new Ed25519PublicKeyParameters(one.Bytes, 0),
                new Ed25519PublicKeyParameters(two.Bytes, 0),
                new Ed25519PublicKeyParameters(three.Bytes, 0),
            });

            Assert.AreEqual(addr.ToAddress().ToString(), "UCE2U2JC4O4ZR6W763GUQCG57HQCDZEUJY4J5I6VYY4HQZUJDF7AKZO5GM");
            TestUtil.SerializeDeserializeCheck(addr);
        }
    }
}
