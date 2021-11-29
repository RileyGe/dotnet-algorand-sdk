using Algorand;
using NUnit.Framework;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;

namespace test
{
    [TestFixture]
    public class AccountTests
    {
        [Test]
        public void testSignsTransactionE2E()
        {
            string REF_SIG_TXN = "82a3736967c4403f5a5cbc5cb038b0d29a53c0adf8a643822da0e41681bcab050e406fd40af20aa56a2f8c0e05d3bee8d4e8489ef13438151911b31b5ed5b660cac6bae4080507a374786e87a3616d74cd04d2a3666565cd03e8a26676ce0001a04fa26c76ce0001a437a3726376c4207d3f99e53d34ae49eb2f458761cf538408ffdaee35c70d8234166de7abe3e517a3736e64c4201bd63dc672b0bb29d42fcafa3422a4d385c0c8169bb01595babf8855cf596979a474797065a3706179";
            string REF_TX_ID = "BXSNCHKYEXB4AQXFRROUJGZ4ZWD7WL2F5D27YUPFR7ONDK5TMN5Q";
            string FROM_ADDR = "DPLD3RTSWC5STVBPZL5DIIVE2OC4BSAWTOYBLFN2X6EFLT2ZNF4SMX64UA";
            string FROM_SK = "actress tongue harbor tray suspect odor load topple vocal avoid ignore apple lunch unknown tissue museum once switch captain place lemon sail outdoor absent creek";
            string TO_ADDR = "PU7ZTZJ5GSXET2ZPIWDWDT2TQQEP7WXOGXDQ3ARUCZW6PK7D4ULSE6NYCE";

            var tx = new Transaction(new Address(FROM_ADDR), 1000, 106575, 107575, null, 1234,
                new Address(TO_ADDR), null, new Digest());

            byte[] seed = Mnemonic.ToKey(FROM_SK);
            Account account = new Account(seed);
            // make sure public key generated from mnemonic is correct
            Assert.That(new Address(account.GetClearTextPublicKey()).ToString() == FROM_ADDR);
            // make sure address was also correctly computed
            Assert.That(account.Address.ToString() == FROM_ADDR);

            // sign the transaction
            SignedTransaction signedTx = account.SignTransaction(tx);
            byte[] signedTxBytes = Encoder.EncodeToMsgPack(signedTx);
            string signedTxHex = Encoder.EncodeToHexStr(signedTxBytes);
            Assert.AreEqual(signedTxHex, REF_SIG_TXN);

            // verify transaction ID
            string txID = signedTx.transactionID;
            Assert.AreEqual(txID, REF_TX_ID);
        }

        [Test]
        public void testSignsTransactionZeroValE2E()
        {
            string REF_SIG_TXN = "82a3736967c440fc12c24dc9d7c48ff0bfb3464c3f4d429088ffe98353a844ba833fd32aaef577e78b49e2674f9998fa5ddfc49db52d8e0c258cafdb5d55ab73edd6678d4b230ea374786e86a3616d74cd04d2a3666565cd03e8a26c76ce0001a437a3726376c4207d3f99e53d34ae49eb2f458761cf538408ffdaee35c70d8234166de7abe3e517a3736e64c4201bd63dc672b0bb29d42fcafa3422a4d385c0c8169bb01595babf8855cf596979a474797065a3706179";
            string REF_TX_ID = "LH7ZXC6OO2LMDSDUIGA42WTILX7TX2K6HE4JVHGAR2UFYU6JZQOA";
            string FROM_ADDR = "DPLD3RTSWC5STVBPZL5DIIVE2OC4BSAWTOYBLFN2X6EFLT2ZNF4SMX64UA";
            string FROM_SK = "actress tongue harbor tray suspect odor load topple vocal avoid ignore apple lunch unknown tissue museum once switch captain place lemon sail outdoor absent creek";
            string TO_ADDR = "PU7ZTZJ5GSXET2ZPIWDWDT2TQQEP7WXOGXDQ3ARUCZW6PK7D4ULSE6NYCE";



            // build unsigned transaction
            var tx = new Transaction(new Address(FROM_ADDR), 1000, 0, 107575, null, 1234,
                new Address(TO_ADDR), null, new Digest());


            byte[] seed = Mnemonic.ToKey(FROM_SK);
            Account account = new Account(seed);
            // make sure public key generated from mnemonic is correct
            Assert.AreEqual(new Address(account.GetClearTextPublicKey()).ToString(), FROM_ADDR);
            // make sure address was also correctly computed
            Assert.AreEqual(account.Address.ToString(), FROM_ADDR);

            // sign the transaction
            SignedTransaction signedTx = account.SignTransaction(tx);
            byte[] signedTxBytes = Encoder.EncodeToMsgPack(signedTx);
            string signedTxHex = Encoder.EncodeToHexStr(signedTxBytes);
            Assert.AreEqual(signedTxHex, REF_SIG_TXN);

            // verify transaction ID
            string txID = signedTx.transactionID;
            Assert.AreEqual(txID, REF_TX_ID);
        }

        [Test]
        public void testKeygen()
        {
            for (int i = 0; i < 100; i++)
            {
                Account account = new Account();
                Assert.IsNotEmpty(account.GetClearTextPublicKey());
                Assert.IsNotNull(account.Address);
                Assert.AreEqual(account.GetClearTextPublicKey(), account.Address.Bytes);
            }
        }

        [Test]
        public void ToMnemonicTest()
        {
            // Arrange
            string FROM_SK = "actress tongue harbor tray suspect odor load topple vocal avoid ignore apple lunch unknown tissue museum once switch captain place lemon sail outdoor absent creek";

            // Act
            byte[] seed = Mnemonic.ToKey(FROM_SK);
            Account account = new Account(seed);
            // Assert
            Assert.AreEqual(account.ToMnemonic(), FROM_SK);
        }

        private MultisigAddress makeTestMsigAddr()
        {
            Address one = new Address("DN7MBMCL5JQ3PFUQS7TMX5AH4EEKOBJVDUF4TCV6WERATKFLQF4MQUPZTA");
            Address two = new Address("BFRTECKTOOE7A5LHCF3TTEOH2A7BW46IYT2SX5VP6ANKEXHZYJY77SJTVM");
            Address three = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            return new MultisigAddress(1, 2, new List<Ed25519PublicKeyParameters>
            {
                new Ed25519PublicKeyParameters(one.Bytes, 0),
                new Ed25519PublicKeyParameters(two.Bytes, 0),
                new Ed25519PublicKeyParameters(three.Bytes, 0),
            });
        }

        [Test]
        public void testSignMultisigTransaction()
        {
            MultisigAddress addr = makeTestMsigAddr();

            // build unsigned transaction
            var tx = new Transaction(addr.ToAddress(), 217000, 972508, 973508,
                Convert.FromBase64String("tFF5Ofz60nE="), 5000,
                new Address("DN7MBMCL5JQ3PFUQS7TMX5AH4EEKOBJVDUF4TCV6WERATKFLQF4MQUPZTA"),
                "testnet-v31.0", new Digest());

            byte[] seed = Mnemonic.ToKey("auction inquiry lava second expand liberty glass involve ginger illness length room item discover ahead table doctor term tackle cement bonus profit right above catch");
            Account account = new Account(seed);
            SignedTransaction stx = account.SignMultisigTransaction(addr, tx);
            byte[] enc = Encoder.EncodeToMsgPack(stx);

            // check main signature is correct
            byte[] golden = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAdvZ3y9GsInBPutdwKc7Jy+an13CcjSV1lcvRAYQKYOxXwfgT5B/mK14R57ueYJTYyoDO8zBY6kQmBalWkm95AIGicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgaJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGjdGhyAqF2AaN0eG6Jo2FtdM0TiKNmZWXOAANPqKJmds4ADtbco2dlbq10ZXN0bmV0LXYzMS4womx2zgAO2sSkbm90ZcQItFF5Ofz60nGjcmN2xCAbfsCwS+pht5aQl+bL9AfhCKcFNR0LyYq+sSIJqKuBeKNzbmTEII2StImQAXOgTfpDWaNmamr86ixCoF3Zwfc+66VHgDfppHR5cGWjcGF5");
            Assert.AreEqual(enc, golden);
        }

        [Test]
        public void testDecodeSignedTransaction()
        {
            var nmemonic = "auction inquiry lava second expand liberty glass involve ginger illness length room item discover ahead table doctor term tackle cement bonus profit right above catch";
            Account account = new Account(nmemonic);

            // build unsigned transaction
            var tx = new Transaction(account.Address, 217000, 972508, 973508,
                Convert.FromBase64String("tFF5Ofz60nE="), 5000,
                new Address("DN7MBMCL5JQ3PFUQS7TMX5AH4EEKOBJVDUF4TCV6WERATKFLQF4MQUPZTA"),
                "testnet-v31.0", new Digest());
            
            SignedTransaction stx = account.SignTransaction(tx);
            string json = Encoder.EncodeToJson(stx);
            SignedTransaction deStx = Encoder.DecodeFromJson<SignedTransaction>(json);
            // check main signature is correct
            Assert.AreEqual(stx, deStx);

            var byt = Encoder.EncodeToMsgPack(stx);
            var deStx2 = Encoder.DecodeFromMsgPack<SignedTransaction>(byt);
            Assert.AreEqual(stx, deStx2);
        }



        [Test]
        public void testAppendMultisigTransaction()
        {
            MultisigAddress addr = makeTestMsigAddr();
            byte[] firstTxBytes = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAdvZ3y9GsInBPutdwKc7Jy+an13CcjSV1lcvRAYQKYOxXwfgT5B/mK14R57ueYJTYyoDO8zBY6kQmBalWkm95AIGicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgaJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGjdGhyAqF2AaN0eG6Jo2FtdM0TiKNmZWXOAANPqKJmds4ADtbco2dlbq10ZXN0bmV0LXYzMS4womx2zgAO2sSkbm90ZcQItFF5Ofz60nGjcmN2xCAbfsCwS+pht5aQl+bL9AfhCKcFNR0LyYq+sSIJqKuBeKNzbmTEII2StImQAXOgTfpDWaNmamr86ixCoF3Zwfc+66VHgDfppHR5cGWjcGF5");

            byte[] seed = Mnemonic.ToKey("since during average anxiety protect cherry club long lawsuit loan expand embark forum theory winter park twenty ball kangaroo cram burst board host ability left");
            Account account = new Account(seed);

            byte[] appended = account.AppendMultisigTransactionBytes(addr, firstTxBytes);
            byte[] expected = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAdvZ3y9GsInBPutdwKc7Jy+an13CcjSV1lcvRAYQKYOxXwfgT5B/mK14R57ueYJTYyoDO8zBY6kQmBalWkm95AIKicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxoXPEQE4cdVDpoVoVVokXRGz6O9G3Ojljd+kd6d2AahXLPGDPtT/QA9DI1rB4w8cEDTy7gd5Padkn5EZC2pjzGh0McAeBonBrxCDn8PhNBoEd+fMcjYeLEVX0Zx1RoYXCAJCGZ/RJWHBooaN0aHICoXYBo3R4bomjYW10zROIo2ZlZc4AA0+oomZ2zgAO1tyjZ2VurXRlc3RuZXQtdjMxLjCibHbOAA7axKRub3RlxAi0UXk5/PrScaNyY3bEIBt+wLBL6mG3lpCX5sv0B+EIpwU1HQvJir6xIgmoq4F4o3NuZMQgjZK0iZABc6BN+kNZo2ZqavzqLEKgXdnB9z7rpUeAN+mkdHlwZaNwYXk=");
            Assert.AreEqual(appended, expected);
        }

        [Test]
        public void testSignMultisigKeyRegTransaction()
        {
            MultisigAddress addr = makeTestMsigAddr();
            byte[] encKeyRegTx = Convert.FromBase64String("gaN0eG6Jo2ZlZc4AA8jAomZ2zgAO+dqibHbOAA79wqZzZWxrZXnEIDISKyvWPdxTMZYXpapTxLHCb+PcyvKNNiK1aXehQFyGo3NuZMQgjZK0iZABc6BN+kNZo2ZqavzqLEKgXdnB9z7rpUeAN+mkdHlwZaZrZXlyZWemdm90ZWtkzScQp3ZvdGVrZXnEIHAb1/uRKwezCBH/KB2f7pVj5YAuICaJIxklj3f6kx6Ip3ZvdGVsc3TOAA9CQA==");
            SignedTransaction wrappedTx = Encoder.DecodeFromMsgPack<SignedTransaction>(encKeyRegTx);

            byte[] seed = Mnemonic.ToKey("auction inquiry lava second expand liberty glass involve ginger illness length room item discover ahead table doctor term tackle cement bonus profit right above catch");
            Account account = new Account(seed);
            SignedTransaction stx = account.SignMultisigTransaction(addr, wrappedTx.tx);
            byte[] enc = Encoder.EncodeToMsgPack(stx);

            // check main signature is correct
            byte[] golden = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAujReoxR7FeTUTqgOn+rS20XOF3ENA+JrSgZ5yvrDPg3NQAzQzUXddB0PVvPRn490oVSQaHEIY05EDJXVBFPJD4GicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgaJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGjdGhyAqF2AaN0eG6Jo2ZlZc4AA8jAomZ2zgAO+dqibHbOAA79wqZzZWxrZXnEIDISKyvWPdxTMZYXpapTxLHCb+PcyvKNNiK1aXehQFyGo3NuZMQgjZK0iZABc6BN+kNZo2ZqavzqLEKgXdnB9z7rpUeAN+mkdHlwZaZrZXlyZWemdm90ZWtkzScQp3ZvdGVrZXnEIHAb1/uRKwezCBH/KB2f7pVj5YAuICaJIxklj3f6kx6Ip3ZvdGVsc3TOAA9CQA==");
            Assert.AreEqual(enc, golden);
        }

        [Test]
        public void testAppendMultisigKeyRegTransaction()
        {
            MultisigAddress addr = makeTestMsigAddr();
            byte[] firstTxBytes = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAujReoxR7FeTUTqgOn+rS20XOF3ENA+JrSgZ5yvrDPg3NQAzQzUXddB0PVvPRn490oVSQaHEIY05EDJXVBFPJD4GicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgaJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGjdGhyAqF2AaN0eG6Jo2ZlZc4AA8jAomZ2zgAO+dqibHbOAA79wqZzZWxrZXnEIDISKyvWPdxTMZYXpapTxLHCb+PcyvKNNiK1aXehQFyGo3NuZMQgjZK0iZABc6BN+kNZo2ZqavzqLEKgXdnB9z7rpUeAN+mkdHlwZaZrZXlyZWemdm90ZWtkzScQp3ZvdGVrZXnEIHAb1/uRKwezCBH/KB2f7pVj5YAuICaJIxklj3f6kx6Ip3ZvdGVsc3TOAA9CQA==");

            byte[] seed = Mnemonic.ToKey("advice pudding treat near rule blouse same whisper inner electric quit surface sunny dismiss leader blood seat clown cost exist hospital century reform able sponsor");
            Account account = new Account(seed);
            byte[] appended = account.AppendMultisigTransactionBytes(addr, firstTxBytes);
            byte[] expected = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAujReoxR7FeTUTqgOn+rS20XOF3ENA+JrSgZ5yvrDPg3NQAzQzUXddB0PVvPRn490oVSQaHEIY05EDJXVBFPJD4GicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgqJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGhc8RArIVZWayeobzKSv+zpJJmbrjsglY5J09/1KU37T5cSl595mMotqO7a2Hmz0XaRxoS6pVhsc2YSkMiU/YhHJCcA6N0aHICoXYBo3R4bomjZmVlzgADyMCiZnbOAA752qJsds4ADv3CpnNlbGtlecQgMhIrK9Y93FMxlhelqlPEscJv49zK8o02IrVpd6FAXIajc25kxCCNkrSJkAFzoE36Q1mjZmpq/OosQqBd2cH3PuulR4A36aR0eXBlpmtleXJlZ6Z2b3Rla2TNJxCndm90ZWtlecQgcBvX+5ErB7MIEf8oHZ/ulWPlgC4gJokjGSWPd/qTHoindm90ZWxzdM4AD0JA");

            
            Assert.AreEqual(appended, expected);
        }

        [Test]
        public void testMergeMultisigTransaction()
        {
            byte[] firstAndThird = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAujReoxR7FeTUTqgOn+rS20XOF3ENA+JrSgZ5yvrDPg3NQAzQzUXddB0PVvPRn490oVSQaHEIY05EDJXVBFPJD4GicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxgqJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGhc8RArIVZWayeobzKSv+zpJJmbrjsglY5J09/1KU37T5cSl595mMotqO7a2Hmz0XaRxoS6pVhsc2YSkMiU/YhHJCcA6N0aHICoXYBo3R4bomjZmVlzgADyMCiZnbOAA752qJsds4ADv3CpnNlbGtlecQgMhIrK9Y93FMxlhelqlPEscJv49zK8o02IrVpd6FAXIajc25kxCCNkrSJkAFzoE36Q1mjZmpq/OosQqBd2cH3PuulR4A36aR0eXBlpmtleXJlZ6Z2b3Rla2TNJxCndm90ZWtlecQgcBvX+5ErB7MIEf8oHZ/ulWPlgC4gJokjGSWPd/qTHoindm90ZWxzdM4AD0JA");
            byte[] secondAndThird = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgaJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXiConBrxCAJYzIJU3OJ8HVnEXc5kcfQPhtzyMT1K/av8BqiXPnCcaFzxEC/jqaH0Dvo3Fa0ZVXsQAP8M5UL9+JxzWipDnA1wmApqllyuZHkZNwG0eSY+LDKMBoB2WaYcJNWypJi4l1f6aIPgqJwa8Qg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGhc8RArIVZWayeobzKSv+zpJJmbrjsglY5J09/1KU37T5cSl595mMotqO7a2Hmz0XaRxoS6pVhsc2YSkMiU/YhHJCcA6N0aHICoXYBo3R4bomjZmVlzgADyMCiZnbOAA752qJsds4ADv3CpnNlbGtlecQgMhIrK9Y93FMxlhelqlPEscJv49zK8o02IrVpd6FAXIajc25kxCCNkrSJkAFzoE36Q1mjZmpq/OosQqBd2cH3PuulR4A36aR0eXBlpmtleXJlZ6Z2b3Rla2TNJxCndm90ZWtlecQgcBvX+5ErB7MIEf8oHZ/ulWPlgC4gJokjGSWPd/qTHoindm90ZWxzdM4AD0JA");
            byte[] expected = Convert.FromBase64String("gqRtc2lng6ZzdWJzaWeTgqJwa8QgG37AsEvqYbeWkJfmy/QH4QinBTUdC8mKvrEiCairgXihc8RAujReoxR7FeTUTqgOn+rS20XOF3ENA+JrSgZ5yvrDPg3NQAzQzUXddB0PVvPRn490oVSQaHEIY05EDJXVBFPJD4KicGvEIAljMglTc4nwdWcRdzmRx9A+G3PIxPUr9q/wGqJc+cJxoXPEQL+OpofQO+jcVrRlVexAA/wzlQv34nHNaKkOcDXCYCmqWXK5keRk3AbR5Jj4sMowGgHZZphwk1bKkmLiXV/pog+ConBrxCDn8PhNBoEd+fMcjYeLEVX0Zx1RoYXCAJCGZ/RJWHBooaFzxECshVlZrJ6hvMpK/7OkkmZuuOyCVjknT3/UpTftPlxKXn3mYyi2o7trYebPRdpHGhLqlWGxzZhKQyJT9iEckJwDo3RocgKhdgGjdHhuiaNmZWXOAAPIwKJmds4ADvnaomx2zgAO/cKmc2Vsa2V5xCAyEisr1j3cUzGWF6WqU8Sxwm/j3MryjTYitWl3oUBchqNzbmTEII2StImQAXOgTfpDWaNmamr86ixCoF3Zwfc+66VHgDfppHR5cGWma2V5cmVnpnZvdGVrZM0nEKd2b3Rla2V5xCBwG9f7kSsHswgR/ygdn+6VY+WALiAmiSMZJY93+pMeiKd2b3RlbHN0zgAPQkA=");
            byte[] a = Account.MergeMultisigTransactionBytes(firstAndThird, secondAndThird);
            byte[] b = Account.MergeMultisigTransactionBytes(secondAndThird, firstAndThird);
            Assert.AreEqual(a, b);
            Assert.AreEqual(a, expected);
        }

        [Test]
        public void testSignBytes()
        {
            byte[] b = new byte[15];
            new Random().NextBytes(b);
            Account account = new Account();
            Signature signature = account.SignBytes(b);
            Assert.IsTrue(account.Address.VerifyBytes(b, signature));
            int firstByte = (int)b[0];
            firstByte = (firstByte + 1) % 256;
            b[0] = (byte)firstByte;
            Assert.IsFalse(account.Address.VerifyBytes(b, signature));
        }

        [Test]
        public void testVerifyBytes()
        {
            byte[] message = Convert.FromBase64String("rTs7+dUj");
            Signature signature = new Signature(Convert.FromBase64String("COEBmoD+ysVECoyVOAsvMAjFxvKeQVkYld+RSHMnEiHsypqrfj2EdYqhrm4t7dK3ZOeSQh3aXiZK/zqQDTPBBw=="));
            Address address = new Address("DPLD3RTSWC5STVBPZL5DIIVE2OC4BSAWTOYBLFN2X6EFLT2ZNF4SMX64UA");
            Assert.IsTrue(address.VerifyBytes(message, signature));
            int firstByte = (int)message[0];
            firstByte = (firstByte + 1) % 256;
            message[0] = (byte)firstByte;
            Assert.IsFalse(address.VerifyBytes(message, signature));
        }

        [Test]
        public void testLogicsigTransaction()
        {
            Address from = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            Address to = new Address("PNWOET7LLOWMBMLE4KOCELCX6X3D3Q4H2Q4QJASYIEOF7YIPPQBG3YQ5YI");
            string mn = "advice pudding treat near rule blouse same whisper inner electric quit surface sunny dismiss leader blood seat clown cost exist hospital century reform able sponsor";
            Account account = new Account(mn);

            ulong fee = 1000;
            ulong amount = 2000;
            string genesisID = "devnet-v1.0";
            Digest genesisHash = new Digest("sC3P7e2SdbqKJK0tbiCdK9tdSpbe6XeCGKdoNzmlj0E=");
            ulong firstRound = 2063137;
            byte[] note = Convert.FromBase64String("8xMCTuLQ810=");

            var tx = new Transaction(from, fee, firstRound, firstRound + 1000, note, amount, to, genesisID, genesisHash);

            /*
            {
            "lsig": {
                "arg": [
                "MTIz",
                "NDU2"
                ],
                "l": "// version 1\nintcblock 1\nintc_0\n",
                "sig": "ToddojkrSVyrnSj/LdtY5izLD1MuL+iitkHjFo12fVnXjfnW7Z/olM43jvx+X4mEg/gc1FEAiH8jwRZcE+klDQ=="
            },
            "txn": {
                "amt": 2000,
                "fee": 1000,
                "fv": 2063137,
                "gen": "devnet-v1.0",
                "gh": "sC3P7e2SdbqKJK0tbiCdK9tdSpbe6XeCGKdoNzmlj0E=",
                "lv": 2064137,
                "note": "8xMCTuLQ810=",
                "rcv": "PNWOET7LLOWMBMLE4KOCELCX6X3D3Q4H2Q4QJASYIEOF7YIPPQBG3YQ5YI",
                "snd": "47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU",
                "type": "pay"
                }
            }
            */
            string goldenTx = "gqRsc2lng6NhcmeSxAMxMjPEAzQ1NqFsxAUBIAEBIqNzaWfEQE6HXaI5K0lcq50o/y3bWOYsyw9TLi/oorZB4xaNdn1Z14351u2f6JTON478fl+JhIP4HNRRAIh/I8EWXBPpJQ2jdHhuiqNhbXTNB9CjZmVlzQPoomZ2zgAfeyGjZ2Vuq2Rldm5ldC12MS4womdoxCCwLc/t7ZJ1uookrS1uIJ0r211Klt7pd4IYp2g3OaWPQaJsds4AH38JpG5vdGXECPMTAk7i0PNdo3JjdsQge2ziT+tbrMCxZOKcIixX9fY9w4fUOQSCWEEcX+EPfAKjc25kxCDn8PhNBoEd+fMcjYeLEVX0Zx1RoYXCAJCGZ/RJWHBooaR0eXBlo3BheQ==";

            byte[] program = { 0x01, 0x20, 0x01, 0x01, 0x22 /*int 1*/};

            List<byte[]> args = new List<byte[]>();
            byte[] arg1 = { 49, 50, 51 };
            byte[] arg2 = { 52, 53, 54 };
            args.Add(arg1);
            args.Add(arg2);

            LogicsigSignature lsig = new LogicsigSignature(program, args);
            account.SignLogicsig(lsig);
            SignedTransaction stx = Account.SignLogicsigTransaction(lsig, tx);

            Assert.AreEqual(Convert.ToBase64String(Encoder.EncodeToMsgPack(stx)), goldenTx);
        }
        [Test]
        public void testTealSign()
        {
            byte[] data = Convert.FromBase64String("Ux8jntyBJQarjKGF8A==");
            byte[] seed = Convert.FromBase64String("5Pf7eGMA52qfMT4R4/vYCt7con/7U3yejkdXkrcb26Q=");
            byte[] prog = Convert.FromBase64String("ASABASI=");
            Address addr = new Address("6Z3C3LDVWGMX23BMSYMANACQOSINPFIRF77H7N3AWJZYV6OH6GWTJKVMXY");
            Account acc = new Account(seed);
            Signature sig1 = acc.TealSign(data, addr);
            Signature sig2 = acc.TealSignFromProgram(data, prog);

            Assert.AreEqual(sig1, sig2);

            byte[] prefix = System.Text.Encoding.UTF8.GetBytes("ProgData");
            byte[] rawAddr = addr.Bytes;
            List<byte> buf = new List<byte>();
            buf.AddRange(prefix);
            buf.AddRange(rawAddr);
            buf.AddRange(data);
            //ByteBuffer buf = ByteBuffer.wrap(message);
            //buf.put(prefix).put(rawAddr).put(data);
            var pk = new Ed25519PublicKeyParameters(acc.Address.Bytes, 0);
            bool verified;
            try
            {
                var signer = new Org.BouncyCastle.Crypto.Signers.Ed25519Signer();
                signer.Init(false, pk); //false代表用于VerifySignature
                signer.BlockUpdate(buf.ToArray(), 0, buf.ToArray().Length);
                verified = signer.VerifySignature(sig1.Bytes);
            }
            catch (Exception)
            {
                verified = false;
            }
            Assert.IsTrue(verified);
        }
    }
}
