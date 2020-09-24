using Algorand;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;
using Encoder = Algorand.Encoder;

namespace test
{
    [TestFixture]
    class TestLogicsigSignature
    {


        [Test]
        public void testLogicsigCreation()
        {
            byte[] program = {
                0x01, 0x20, 0x01, 0x01, 0x22  // int 1
            };
            List<byte[]> args = new List<byte[]>();
            string programHash = "6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY";
            Address sender = new Address(programHash);

            LogicsigSignature lsig = new LogicsigSignature(program);
            Assert.AreEqual(lsig.logic, program);
            Assert.IsNull(lsig.args);
            Assert.IsNull(lsig.sig);
            Assert.IsNull(lsig.msig);
            bool verified = lsig.Verify(sender);
            Assert.IsTrue(verified);
            Assert.AreEqual(lsig.ToAddress(), sender);

            byte[] arg1 = { 1, 2, 3 };
            byte[] arg2 = { 4, 5, 6 };
            args.Add(arg1);
            args.Add(arg2);

            lsig = new LogicsigSignature(program, args);
            Assert.AreEqual(lsig.logic, program);
            Assert.AreEqual(lsig.args, args);
            Assert.IsNull(lsig.sig);
            Assert.IsNull(lsig.msig);
            verified = lsig.Verify(sender);
            Assert.IsTrue(verified);
            Assert.AreEqual(lsig.ToAddress(), sender);

            // check serialization
            byte[] outBytes = Encoder.EncodeToMsgPack(lsig);
            LogicsigSignature lsig1 = Encoder.DecodeFromMsgPack<LogicsigSignature>(outBytes);
            Assert.AreEqual(lsig, lsig1);

            // check serialization with null args
            lsig = new LogicsigSignature(program);
            outBytes = Encoder.EncodeToMsgPack(lsig);
            lsig1 = Encoder.DecodeFromMsgPack<LogicsigSignature>(outBytes);
            Assert.AreEqual(lsig, lsig1);

            // check modified program fails on verification
            program[3] = 2;
            lsig = new LogicsigSignature(program);
            verified = lsig.Verify(sender);
            Assert.IsFalse(verified);
            TestUtil.SerializeDeserializeCheck(lsig);
        }

        [Test]
        public void testLogicsigSignature()
        {
            byte[] program = {
                0x01, 0x20, 0x01, 0x01, 0x22  // int 1
            };

            LogicsigSignature lsig = new LogicsigSignature(program);
            Account account = new Account();
            lsig = account.SignLogicsig(lsig);
            Assert.AreEqual(lsig.logic, program);
            Assert.IsNull(lsig.args);
            Assert.AreNotEqual(lsig.sig, new Signature());
            Assert.IsNotNull(lsig.sig);

            Assert.IsNull(lsig.msig);
            bool verified = lsig.Verify(account.Address);
            Assert.IsTrue(verified);

            // check serialization
            byte[] outBytes = Encoder.EncodeToMsgPack(lsig);
            LogicsigSignature lsig1 = Encoder.DecodeFromMsgPack<LogicsigSignature>(outBytes);
            Assert.AreEqual(lsig1, lsig);
            TestUtil.SerializeDeserializeCheck(lsig);
        }

        [Test]
        public void testLogicsigMultisigSignature()
        {
            byte[] program = { 0x01, 0x20, 0x01, 0x01, 0x22   /*int 1*/            };

            Address one = new Address("DN7MBMCL5JQ3PFUQS7TMX5AH4EEKOBJVDUF4TCV6WERATKFLQF4MQUPZTA");
            Address two = new Address("BFRTECKTOOE7A5LHCF3TTEOH2A7BW46IYT2SX5VP6ANKEXHZYJY77SJTVM");
            Address three = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            MultisigAddress ma = new MultisigAddress(1, 2, new List<Ed25519PublicKeyParameters>
            {
                new Ed25519PublicKeyParameters(one.Bytes, 0),
                new Ed25519PublicKeyParameters(two.Bytes, 0),
                new Ed25519PublicKeyParameters(three.Bytes, 0),
            });

            string mn1 = "auction inquiry lava second expand liberty glass involve ginger illness length room item discover ahead table doctor term tackle cement bonus profit right above catch";
            string mn2 = "since during average anxiety protect cherry club long lawsuit loan expand embark forum theory winter park twenty ball kangaroo cram burst board host ability left";
            Account acc1 = new Account(mn1);
            Account acc2 = new Account(mn2);
            Account account = new Account();

            LogicsigSignature lsig = new LogicsigSignature(program);
            lsig = acc1.SignLogicsig(lsig, ma);
            Assert.AreEqual(lsig.logic, program);
            Assert.IsNull(lsig.args);
            Assert.IsNull(lsig.sig);
            Assert.AreNotEqual(lsig.msig, new MultisigSignature());
            Assert.IsNotNull(lsig.msig);

            var verified = lsig.Verify(ma.ToAddress());
            Assert.IsFalse(verified);

            LogicsigSignature lsigLambda = lsig;
            //Assert.AreEqualThrownBy(()->account.appendToLogicsig(lsigLambda))
            //        .isInstanceOf(IllegalArgumentException.class)
            //                .hasMessage("Multisig account does not contain this secret key");

            lsig = acc2.AppendToLogicsig(lsig);
            verified = lsig.Verify(ma.ToAddress());
            Assert.IsTrue(verified);

            // Add a single signature and ensure it fails
            LogicsigSignature lsig1 = new LogicsigSignature(program);
            lsig1 = account.SignLogicsig(lsig1);
            lsig.sig = lsig1.sig;
            verified = lsig.Verify(ma.ToAddress());
            Assert.IsFalse(verified);
            verified = lsig.Verify(account.Address);
            Assert.IsFalse(verified);

            // Remove and ensure it still works
            lsig.sig = null;
            verified = lsig.Verify(ma.ToAddress());
            Assert.IsTrue(verified);

            // check serialization
            byte[] outBytes = Encoder.EncodeToMsgPack(lsig);
            LogicsigSignature lsig2 = Encoder.DecodeFromMsgPack<LogicsigSignature>(outBytes);
            Assert.AreEqual(lsig2, lsig);
            verified = lsig2.Verify(ma.ToAddress());
            Assert.IsTrue(verified);
            TestUtil.SerializeDeserializeCheck(lsig2);
        }
    }
}
