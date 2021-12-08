using Algorand;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using Encoder = Algorand.Encoder;

namespace test
{
    [TestFixture]
    public class TransactionTests
    {
        private static Account DEFAULT_ACCOUNT = initializeDefaultAccount();

        private static Account initializeDefaultAccount()
        {
            try
            {
                string mnemonic = "awful drop leaf tennis indoor begin mandate discover uncle seven only coil atom any hospital uncover make any climb actor armed measure need above hundred";
                return new Account(mnemonic);
            }
            catch (Exception e)
            {
                Assert.Fail("Failed to initialize static default account." + e.Message);
            }
            return null;
        }

        [Test]
        public void testSerialization()
        {
            Address from = new Address("VKM6KSCTDHEM6KGEAMSYCNEGIPFJMHDSEMIRAQLK76CJDIRMMDHKAIRMFQ");
            Address to = new Address("CQW2QBBUW5AGFDXMURQBRJN2AM3OHHQWXXI4PEJXRCVTEJ3E5VBTNRTEAE");

            var tx = new Transaction(from, null, 301, 1300, null, 100,
                to, null, new Digest());

            var json = Encoder.EncodeToJson(tx);
            Transaction o = Encoder.DecodeFromJson<Transaction>(json);
            Assert.AreEqual(o, tx);
        }

        [Test]
        public void testSerializationMsgpack()
        {
            Address from = new Address("VKM6KSCTDHEM6KGEAMSYCNEGIPFJMHDSEMIRAQLK76CJDIRMMDHKAIRMFQ");
            Address to = new Address("CQW2QBBUW5AGFDXMURQBRJN2AM3OHHQWXXI4PEJXRCVTEJ3E5VBTNRTEAE");
            var tx = new Transaction(from, null, 301, 1300, null, 100,
                to, null, new Digest());

            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);
        }

        private void createAssetTest(int numDecimal, string goldenString)
        {
            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");

            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address sender = addr;
            Address manager = addr;
            Address reserve = addr;
            Address freeze = addr;
            Address clawback = addr;
            string metadataHash = "fACPO4nRgO55j1ndAK3W6Sgc4APkcyFh";

            var tx = Transaction.CreateAssetCreateTransaction(sender, 10, 322575, 323575, null, null,
                new Digest(gh), 100, numDecimal, false, "tst", "testcoin", "website",
                Encoding.UTF8.GetBytes(metadataHash), manager, reserve, freeze, clawback);

            Transaction.AssetParams expectedParams = new Transaction.AssetParams(100, numDecimal, false, "tst", "testcoin",
                "website", Encoding.UTF8.GetBytes(metadataHash), manager, reserve, freeze, clawback);

            Assert.AreEqual(expectedParams, tx.assetParams);
            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);
            byte[] encodedOut = Encoder.EncodeToMsgPack(stx);
            SignedTransaction decodedOut = Encoder.DecodeFromMsgPack<SignedTransaction>(encodedOut);

            Assert.AreEqual(decodedOut, stx);
            Assert.AreEqual(Convert.ToBase64String(encodedOut), goldenString);
            //Assert.AreEqual(decodedOut, stx);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testAssetParamsValidation()
        {
            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            Address manager = addr;
            Address reserve = addr;
            Address freeze = addr;
            Address clawback = addr;
            byte[] tooShortMetadataHash = Encoding.UTF8.GetBytes("fACPO4nRgO55j1ndAK3W6Sgc4APkcyF");
            byte[] tooLongMetadataHash = Encoding.UTF8.GetBytes("fACPO4nRgO55j1ndAK3W6Sgc4APkcyFhfACPO4nRgO55j1ndAK3W6Sgc4APkcyFh");
            byte[] normalMetadataHash = Convert.FromBase64String("IsHwwbvsnx9X5HMW1U468AXzbvRWk8VffLw0NQrmEq0=");
            String url_100 = new string('w', 100);
            
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                new Transaction.AssetParams(100, 3, false, "tst", "testcoin", url_100,
                    normalMetadataHash, manager, reserve, freeze, clawback);
            });
            Assert.AreEqual("asset url cannot be greater than 96 characters", ex.Message);

            var ex2 = Assert.Throws<ArgumentException>(() =>
            {
                new Transaction.AssetParams(100, 3, false, "tst", "testcoin", "website",
                    tooShortMetadataHash, manager, reserve, freeze, clawback);
            });
            Assert.AreEqual( "asset metadataHash must have 32 bytes", ex2.Message);


            var ex3 = Assert.Throws<ArgumentException>(() =>
            {
                new Transaction.AssetParams(100, 3, false, "tst", "testcoin", "website",
                    tooLongMetadataHash, manager, reserve, freeze, clawback);
            });
            Assert.AreEqual("asset metadataHash must have 32 bytes", ex3.Message);
        }

        [Test]
        public void testMakeAssetCreateTxn()
        {
            createAssetTest(0, "gqNzaWfEQEDd1OMRoQI/rzNlU4iiF50XQXmup3k5czI9hEsNqHT7K4KsfmA/0DUVkbzOwtJdRsHS8trm3Arjpy9r7AXlbAujdHhuh6RhcGFyiaJhbcQgZkFDUE80blJnTzU1ajFuZEFLM1c2U2djNEFQa2N5RmiiYW6odGVzdGNvaW6iYXWnd2Vic2l0ZaFjxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFmxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFtxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFyxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aF0ZKJ1bqN0c3SjZmVlzQ+0omZ2zgAE7A+iZ2jEIEhjtRiks8hOyBDyLU8QgcsPcfBZp6wg3sYvf3DlCToiomx2zgAE7/ejc25kxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aR0eXBlpGFjZmc=");
        }

        [Test]
        public void testMakeAssetCreateTxnWithDecimals()
        {
            createAssetTest(1, "gqNzaWfEQCj5xLqNozR5ahB+LNBlTG+d0gl0vWBrGdAXj1ibsCkvAwOsXs5KHZK1YdLgkdJecQiWm4oiZ+pm5Yg0m3KFqgqjdHhuh6RhcGFyiqJhbcQgZkFDUE80blJnTzU1ajFuZEFLM1c2U2djNEFQa2N5RmiiYW6odGVzdGNvaW6iYXWnd2Vic2l0ZaFjxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aJkYwGhZsQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2hbcQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2hcsQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2hdGSidW6jdHN0o2ZlZc0P3KJmds4ABOwPomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4ABO/3o3NuZMQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2kdHlwZaRhY2Zn");
        }

        [Test]
        public void testSerializationAssetConfig()
        {
            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address sender = addr;
            Address manager = addr;
            Address reserve = addr;
            Address freeze = addr;
            Address clawback = addr;

            var tx = Transaction.CreateAssetConfigureTransaction(sender, 10, 322575, 323575, null, null,
                new Digest(gh), 1234, manager, reserve, freeze, clawback, false);

            //Transaction tx = Transaction.AssetConfigureTransactionBuilder()
            //        .sender(sender)
            //        .fee(10)
            //        .firstValid(322575)
            //        .lastValid(323575)
            //        .genesisHash(gh)
            //        .assetIndex(1234)
            //        .manager(manager)
            //        .reserve(reserve)
            //        .freeze(freeze)
            //        .clawback(clawback)
            //        .build();

            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);

            var encodedOutBytes = Encoder.EncodeToMsgPack(stx);
            string goldenstring = "gqNzaWfEQBBkfw5n6UevuIMDo2lHyU4dS80JCCQ/vTRUcTx5m0ivX68zTKyuVRrHaTbxbRRc3YpJ4zeVEnC9Fiw3Wf4REwejdHhuiKRhcGFyhKFjxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFmxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFtxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aFyxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aRjYWlkzQTSo2ZlZc0NSKJmds4ABOwPomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4ABO/3o3NuZMQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2kdHlwZaRhY2Zn";

            SignedTransaction o = Encoder.DecodeFromMsgPack<SignedTransaction>(encodedOutBytes);
            Assert.AreEqual(Convert.ToBase64String(encodedOutBytes), goldenstring);
            Assert.AreEqual(o, stx);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testAssetConfigStrictEmptyAddressChecking()
        {
            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address sender = addr;
            Address manager = addr;
            Address reserve = addr;
            Address freeze = addr;

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Transaction.CreateAssetConfigureTransaction(sender, 10, 322575, 323575, null, null,
                    new Digest(gh), 1234, manager, reserve, freeze, new Address(), true);
            });
            Assert.AreEqual(ex.Message, "strict empty address checking requested but empty or default address supplied to one or more manager addresses");

            //Assert.AreEqualThrownBy(()->Transaction.AssetConfigureTransactionBuilder()
            //    .sender(sender)
            //    .fee(10)
            //    .firstValid()
            //    .lastValid()
            //    .genesisHash(gh)
            //    .assetIndex()
            //    .manager(manager)
            //    .reserve(reserve)
            //    .freeze(freeze)
            //    .clawback(new Address())
            //    .build())
            //.isInstanceOf(RuntimeException)
            //        .hasMessage();
            ex = Assert.Throws<ArgumentException>(() =>
            {
                Transaction.CreateAssetConfigureTransaction(sender, 10, 322575, 323575, null, null,
                    new Digest(gh), 1234, manager, reserve, freeze, new Address(), true);
            });
            Assert.AreEqual(ex.Message, "strict empty address checking requested but empty or default address supplied to one or more manager addresses");

            //    Assert.AreEqualThrownBy(()->Transaction.AssetConfigureTransactionBuilder()
            //    .sender(sender)
            //    .fee(10)
            //    .firstValid(322575)
            //    .lastValid(323575)
            //    .genesisHash(gh)
            //    .assetIndex(1234)
            //    .manager(manager)
            //    .reserve(reserve)
            //    .freeze(freeze)
            //    .clawback(new Address())
            //    .build())
            //.isInstanceOf(RuntimeException)
            //        .hasMessage();
        }

        [Test]
        public void testSerializationAssetFreeze()
        {
            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address sender = addr;
            Address target = addr;
            ulong assetFreezeID = 1;
            bool freezeState = true;
            var tx = Transaction.CreateAssetFreezeTransaction(sender, target, freezeState, 10,
                322575, 323576, null, new Digest(gh), assetFreezeID);
            //Transaction tx = Transaction.AssetFreezeTransactionBuilder()
            //        .sender(sender)
            //        .freezeTarget(target)
            //        .freezeState(freezeState)
            //        .fee(10)
            //        .firstValid()
            //        .lastValid()
            //        .genesisHash(gh)
            //        .assetIndex(assetFreezeID)
            //        .build();
            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));
            SignedTransaction o = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));
            string goldenstring = "gqNzaWfEQAhru5V2Xvr19s4pGnI0aslqwY4lA2skzpYtDTAN9DKSH5+qsfQQhm4oq+9VHVj7e1rQC49S28vQZmzDTVnYDQGjdHhuiaRhZnJ6w6RmYWRkxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aRmYWlkAaNmZWXNCRqiZnbOAATsD6JnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKibHbOAATv+KNzbmTEIAn70nYsCPhsWua/bdenqQHeZnXXUOB+jFx2mGR9tuH9pHR5cGWkYWZyeg==";

            Assert.AreEqual(encodedOutBytes, goldenstring);
            Assert.AreEqual(o, stx);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testPaymentTransaction()
        {
            string FROM_SK = "advice pudding treat near rule blouse same whisper inner electric quit surface sunny dismiss leader blood seat clown cost exist hospital century reform able sponsor";
            byte[] seed = Mnemonic.ToKey(FROM_SK);
            Account account = new Account(seed);

            Address fromAddr = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            Address toAddr = new Address("PNWOET7LLOWMBMLE4KOCELCX6X3D3Q4H2Q4QJASYIEOF7YIPPQBG3YQ5YI");
            Address closeTo = new Address("IDUTJEUIEVSMXTU4LGTJWZ2UE2E6TIODUKU6UW3FU3UKIQQ77RLUBBBFLA");
            string goldenstring = "gqNzaWfEQPhUAZ3xkDDcc8FvOVo6UinzmKBCqs0woYSfodlmBMfQvGbeUx3Srxy3dyJDzv7rLm26BRv9FnL2/AuT7NYfiAWjdHhui6NhbXTNA+ilY2xvc2XEIEDpNJKIJWTLzpxZpptnVCaJ6aHDoqnqW2Wm6KRCH/xXo2ZlZc0EmKJmds0wsqNnZW6sZGV2bmV0LXYzMy4womdoxCAmCyAJoJOohot5WHIvpeVG7eftF+TYXEx4r7BFJpDt0qJsds00mqRub3RlxAjqABVHQ2y/lqNyY3bEIHts4k/rW6zAsWTinCIsV/X2PcOH1DkEglhBHF/hD3wCo3NuZMQg5/D4TQaBHfnzHI2HixFV9GcdUaGFwgCQhmf0SVhwaKGkdHlwZaNwYXk=";

            ulong firstValidRound = 12466;
            ulong lastValidRound = 13466;
            ulong amountToSend = 1000;
            byte[] note = Convert.FromBase64String("6gAVR0Nsv5Y=");
            string genesisID = "devnet-v33.0";
            Digest genesisHash = new Digest("JgsgCaCTqIaLeVhyL6XlRu3n7Rfk2FxMeK+wRSaQ7dI=");

            var tx = new Transaction(fromAddr, 4, firstValidRound, lastValidRound,
                    note, amountToSend, toAddr, genesisID, genesisHash);
            tx.closeRemainderTo = closeTo;
            Account.SetFeeByFeePerByte(tx, 4);

            //Transaction tx = Transaction.PaymentTransactionBuilder()
            //        .sender()
            //        .fee(4)
            //        .firstValid(firstValidRound)
            //        .lastValid(lastValidRound)
            //        .note(note)
            //        .genesisID(genesisID)
            //        .genesisHash(genesisHash)
            //        .amount(amountToSend)
            //        .receiver(toAddr)
            //        .closeRemainderTo(closeTo)
            //        .build();

            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);

            SignedTransaction stx = account.SignTransaction(tx);
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));

            SignedTransaction stxDecoded = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));
            Assert.AreEqual(stxDecoded, stx);
            Assert.AreEqual(encodedOutBytes, goldenstring);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testTransactionGroupLimit()
        {

            Transaction[] txs = new Transaction[TxGroup.MAX_TX_GROUP_SIZE + 1];

            bool gotExpectedException = false;
            Digest gid = null;
            try
            {
                gid = TxGroup.ComputeGroupID(txs);
            }
            catch (ArgumentException e)
            {
                gotExpectedException = true;
                Assert.AreEqual(e.Message, "max group size is " + TxGroup.MAX_TX_GROUP_SIZE);
            }
            Assert.IsTrue(gotExpectedException);
            Assert.IsNull(gid);

        }

        [Test]
        public void testTransactionGroup()
        {
            Address from = new Address("UPYAFLHSIPMJOHVXU2MPLQ46GXJKSDCEMZ6RLCQ7GWB5PRDKJUWKKXECXI");
            Address to = new Address("UPYAFLHSIPMJOHVXU2MPLQ46GXJKSDCEMZ6RLCQ7GWB5PRDKJUWKKXECXI");
            ulong fee = 1000;
            ulong amount = 2000;
            string genesisID = "devnet-v1.0";
            Digest genesisHash = new Digest("sC3P7e2SdbqKJK0tbiCdK9tdSpbe6XeCGKdoNzmlj0E=");
            ulong firstRound1 = 710399;
            byte[] note1 = Convert.FromBase64String("wRKw5cJ0CMo=");


            var tx1 = new Transaction(from, fee, firstRound1, firstRound1 + 1000,
                    note1, amount, to, genesisID, genesisHash);            

            ulong firstRound2 = 710515;
            byte[] note2 = Convert.FromBase64String("dBlHI6BdrIg=");


            var tx2 = new Transaction(from, fee, firstRound2, firstRound2 + 1000,
                    note2, amount, to, genesisID, genesisHash);
            
            // check serialization/deserialization without group field
            Assert.AreEqual(Encoder.DecodeFromMsgPack<Transaction>(Encoder.EncodeToMsgPack(tx1)), tx1);
            Assert.AreEqual(Encoder.DecodeFromMsgPack<Transaction>(Encoder.EncodeToMsgPack(tx2)), tx2);

            string goldenTx1 = "gaN0eG6Ko2FtdM0H0KNmZWXNA+iiZnbOAArW/6NnZW6rZGV2bmV0LXYxLjCiZ2jEILAtz+3tknW6iiStLW4gnSvbXUqW3ul3ghinaDc5pY9Bomx2zgAK2uekbm90ZcQIwRKw5cJ0CMqjcmN2xCCj8AKs8kPYlx63ppj1w5410qkMRGZ9FYofNYPXxGpNLKNzbmTEIKPwAqzyQ9iXHremmPXDnjXSqQxEZn0Vih81g9fEak0spHR5cGWjcGF5";
            string goldenTx2 = "gaN0eG6Ko2FtdM0H0KNmZWXNA+iiZnbOAArXc6NnZW6rZGV2bmV0LXYxLjCiZ2jEILAtz+3tknW6iiStLW4gnSvbXUqW3ul3ghinaDc5pY9Bomx2zgAK21ukbm90ZcQIdBlHI6BdrIijcmN2xCCj8AKs8kPYlx63ppj1w5410qkMRGZ9FYofNYPXxGpNLKNzbmTEIKPwAqzyQ9iXHremmPXDnjXSqQxEZn0Vih81g9fEak0spHR5cGWjcGF5";

            // goal clerk send dumps unsigned transaction as signed with empty signature in order to save tx type
            SignedTransaction stx1 = new SignedTransaction(tx1, new Signature(), new MultisigSignature(), new LogicsigSignature(), tx1.TxID());
            SignedTransaction stx2 = new SignedTransaction(tx2, new Signature(), new MultisigSignature(), new LogicsigSignature(), tx2.TxID());

            Assert.AreEqual(Convert.ToBase64String(Encoder.EncodeToMsgPack(stx1)), goldenTx1);
            Assert.AreEqual(Convert.ToBase64String(Encoder.EncodeToMsgPack(stx2)), goldenTx2);
            TestUtil.SerializeDeserializeCheck(stx1);
            TestUtil.SerializeDeserializeCheck(stx2);


            Digest gid = TxGroup.ComputeGroupID(tx1, tx2);
            tx1.AssignGroupID(gid);
            tx2.AssignGroupID(gid);

            // check serialization/deserialization with group field set
            Assert.AreEqual(Encoder.DecodeFromMsgPack<Transaction>(Encoder.EncodeToMsgPack(tx1)), tx1);
            Assert.AreEqual(Encoder.DecodeFromMsgPack<Transaction>(Encoder.EncodeToMsgPack(tx2)), tx2);

            // goal clerk group sets Group to every transaction and concatenate them in output file
            // simulating that behavior here
            string goldenTxg = "gaN0eG6Lo2FtdM0H0KNmZWXNA+iiZnbOAArW/6NnZW6rZGV2bmV0LXYxLjCiZ2jEILAtz+3tknW6iiStLW4gnSvbXUqW3ul3ghinaDc5pY9Bo2dycMQgLiQ9OBup9H/bZLSfQUH2S6iHUM6FQ3PLuv9FNKyt09SibHbOAAra56Rub3RlxAjBErDlwnQIyqNyY3bEIKPwAqzyQ9iXHremmPXDnjXSqQxEZn0Vih81g9fEak0so3NuZMQgo/ACrPJD2Jcet6aY9cOeNdKpDERmfRWKHzWD18RqTSykdHlwZaNwYXmBo3R4boujYW10zQfQo2ZlZc0D6KJmds4ACtdzo2dlbqtkZXZuZXQtdjEuMKJnaMQgsC3P7e2SdbqKJK0tbiCdK9tdSpbe6XeCGKdoNzmlj0GjZ3JwxCAuJD04G6n0f9tktJ9BQfZLqIdQzoVDc8u6/0U0rK3T1KJsds4ACttbpG5vdGXECHQZRyOgXayIo3JjdsQgo/ACrPJD2Jcet6aY9cOeNdKpDERmfRWKHzWD18RqTSyjc25kxCCj8AKs8kPYlx63ppj1w5410qkMRGZ9FYofNYPXxGpNLKR0eXBlo3BheQ==";
            stx1 = new SignedTransaction(tx1, new Signature(), new MultisigSignature(), new LogicsigSignature(), tx1.TxID());
            stx2 = new SignedTransaction(tx2, new Signature(), new MultisigSignature(), new LogicsigSignature(), tx2.TxID());
            byte[] stx1Enc = Encoder.EncodeToMsgPack(stx1);
            byte[] stx2Enc = Encoder.EncodeToMsgPack(stx2);
            var concat = stx1Enc.ToList();
            concat.AddRange(stx2Enc);

            Assert.AreEqual(Convert.ToBase64String(concat.ToArray()), goldenTxg);

            // check assignGroupID
            Transaction[] result = TxGroup.AssignGroupID(tx1, tx2);
            Assert.AreEqual(result.Length, 2);

            result = TxGroup.AssignGroupID(from, tx1, tx2);
            Assert.AreEqual(result.Length, 2);

            result = TxGroup.AssignGroupID(to, tx1, tx2);
            Assert.AreEqual(result.Length, 0);
        }

        [Test]
        public void testTransactionGroupEmpty()
        {
            var ex = Assert.Throws<ArgumentException>(() => { TxGroup.ComputeGroupID(); });
            Assert.AreEqual(ex.Message, "empty transaction list");
        }

        [Test]
        public void testTransactionGroupNull()
        {
            var ex = Assert.Throws<ArgumentException>(() => { TxGroup.ComputeGroupID(); });
            Assert.AreEqual(ex.Message, "empty transaction list");
        }

        [Test]
        public void testMakeAssetAcceptanceTxn()
        {

            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address recipient = addr;

            ulong assetIndex = 1;
            ulong firstValidRound = 322575;
            ulong lastValidRound = 323575;

            Transaction tx = Transaction.CreateAssetAcceptTransaction(recipient, 10, firstValidRound, lastValidRound,
                null, null, new Digest(gh), assetIndex);
            tx.fee = 10;
            Account.SetFeeByFeePerByte(tx, 10);

            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);

            /*  Example from: go-algorand-sdk/transaction/transaction_test.go
            {
                "sig:b64": "nuras5PxJv/AHQXzuV37XMymvFWViptRt76jPRYzrcVy0iL4r15gVKpPbpcFnhGvf5VMmkET4ksqzydy2X2GCA==",
                "txn": {
                  "arcv:b64": "CfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f0=",
                  "fee": 2280,
                  "fv": 322575,
                  "gh:b64": "SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=",
                  "lv": 323575,
                  "snd:b64": "CfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f0=",
                  "type": "axfer",
                  "xaid": 1
                }
              }
             */
            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));
            string goldenstring = "gqNzaWfEQJ7q2rOT8Sb/wB0F87ld+1zMprxVlYqbUbe+oz0WM63FctIi+K9eYFSqT26XBZ4Rr3+VTJpBE+JLKs8nctl9hgijdHhuiKRhcmN2xCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aNmZWXNCOiiZnbOAATsD6JnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKibHbOAATv96NzbmTEIAn70nYsCPhsWua/bdenqQHeZnXXUOB+jFx2mGR9tuH9pHR5cGWlYXhmZXKkeGFpZAE=";

            SignedTransaction stxDecoded = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));
            Assert.AreEqual(stxDecoded, stx);
            Assert.AreEqual(encodedOutBytes, goldenstring);
            TestUtil.SerializeDeserializeCheck(stx);
        }


        [Test]
        public void testMakeAssetTransferTxn()
        {

            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address sender = addr;
            Address recipient = addr;
            Address closeAssetsTo = addr;

            ulong assetIndex = 1;
            ulong firstValidRound = 322575;
            ulong lastValidRound = 323576;
            ulong amountToSend = 1;

            Transaction tx = Transaction.CreateAssetTransferTransaction(sender, recipient, closeAssetsTo, amountToSend, 10,
                firstValidRound, lastValidRound, null, null, new Digest(gh), assetIndex);

            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);

            /*
             * Golden from: go-algorand-sdk/transaction/transaction_test.go
                {
                  "sig:b64": "2QSzdZ18WrohAol0XWfT+FtX3Bouy+iPL2kzVPh+/B8w12MZAPL4t56y5BR9BVOd4kPhV8w/vMrHg5SUi1uvBA==",
                  "txn": {
                    "aamt": 1,
                    "aclose:b64": "CfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f0=",
                    "arcv:b64": "CfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f0=",
                    "fee": 2750,
                    "fv": 322575,
                    "gh:b64": "SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=",
                    "lv": 323576,
                    "snd:b64": "CfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f0=",
                    "type": "axfer",
                    "xaid": 1
                  }
                }
             */
            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));
            string goldenstring = "gqNzaWfEQNkEs3WdfFq6IQKJdF1n0/hbV9waLsvojy9pM1T4fvwfMNdjGQDy+LeesuQUfQVTneJD4VfMP7zKx4OUlItbrwSjdHhuiqRhYW10AaZhY2xvc2XEIAn70nYsCPhsWua/bdenqQHeZnXXUOB+jFx2mGR9tuH9pGFyY3bEIAn70nYsCPhsWua/bdenqQHeZnXXUOB+jFx2mGR9tuH9o2ZlZc0KvqJmds4ABOwPomdoxCBIY7UYpLPITsgQ8i1PEIHLD3HwWaesIN7GL39w5Qk6IqJsds4ABO/4o3NuZMQgCfvSdiwI+Gxa5r9t16epAd5mdddQ4H6MXHaYZH224f2kdHlwZaVheGZlcqR4YWlkAQ==";

            SignedTransaction stxDecoded = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));

            Assert.AreEqual(stxDecoded, stx);
            Assert.AreEqual(encodedOutBytes, goldenstring);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testMakeAssetRevocationTransaction()
        {

            Address addr = new Address("BH55E5RMBD4GYWXGX5W5PJ5JAHPGM5OXKDQH5DC4O2MGI7NW4H6VOE4CP4");
            byte[] gh = Convert.FromBase64String("SGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiI=");
            Address revoker = addr;
            Address revokeFrom = addr;
            Address receiver = addr;

            ulong assetIndex = 1;
            ulong firstValidRound = 322575;
            ulong lastValidRound = 323575;
            ulong amountToSend = 1;

            Transaction tx = Transaction.CreateAssetRevokeTransaction(revoker, revokeFrom, receiver, amountToSend,
                10, firstValidRound, lastValidRound, null, null, new Digest(gh), assetIndex);

            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);

            SignedTransaction stx = DEFAULT_ACCOUNT.SignTransaction(tx);
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));
            string goldenstring = "gqNzaWfEQHsgfEAmEHUxLLLR9s+Y/yq5WeoGo/jAArCbany+7ZYwExMySzAhmV7M7S8+LBtJalB4EhzEUMKmt3kNKk6+vAWjdHhuiqRhYW10AaRhcmN2xCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aRhc25kxCAJ+9J2LAj4bFrmv23Xp6kB3mZ111Dgfoxcdphkfbbh/aNmZWXNCqqiZnbOAATsD6JnaMQgSGO1GKSzyE7IEPItTxCByw9x8FmnrCDexi9/cOUJOiKibHbOAATv96NzbmTEIAn70nYsCPhsWua/bdenqQHeZnXXUOB+jFx2mGR9tuH9pHR5cGWlYXhmZXKkeGFpZAE=";
            SignedTransaction stxDecoded = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));

            Assert.AreEqual(stxDecoded, stx);
            Assert.AreEqual(encodedOutBytes, goldenstring);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void testEncoding()
        {
            Address addr1 = new Address("726KBOYUJJNE5J5UHCSGQGWIBZWKCBN4WYD7YVSTEXEVNFPWUIJ7TAEOPM");
            Address addr2 = new Address("42NJMHTPFVPXVSDGA6JGKUV6TARV5UZTMPFIREMLXHETRKIVW34QFSDFRE");
            Account account1 = new Account(Convert.FromBase64String("cv8E0Ln24FSkwDgGeuXKStOTGcze5u8yldpXxgrBxumFPYdMJymqcGoxdDeyuM8t6Kxixfq0PJCyJP71uhYT7w=="));

            string lease = "f4OxZX/x/FO5LcGBSKHWXfwtSx+j1ncoSt3SABJtkGk=";

            var txn = new Transaction(account1.Address, 1000 * 10, 12345, 12346,
                    null, 5000, addr1,
                    null, new Digest("f4OxZX/x/FO5LcGBSKHWXfwtSx+j1ncoSt3SABJtkGk="))
            {
                closeRemainderTo = addr2,
                lease = Convert.FromBase64String(lease)
            };
            Account.SetFeeByFeePerByte(txn, 1000 * 10);

            //Transaction txn = Transaction.PaymentTransactionBuilder()
            //    .sender(account1.getAddress())
            //    .fee(Account.MIN_TX_FEE_UALGOS.longValue() * 10)
            //    .firstValid(12345)
            //    .lastValid(12346)
            //    .genesisHashB64("f4OxZX/x/FO5LcGBSKHWXfwtSx+j1ncoSt3SABJtkGk=")
            //    .amount(5000)
            //    .receiver(addr1)
            //    .closeRemainderTo(addr2)
            //    .leaseB64(lease)
            //    .build();

            byte[] packed = Encoder.EncodeToMsgPack(txn);
            Transaction txnDecoded = Encoder.DecodeFromMsgPack<Transaction>(packed);
            Assert.AreEqual(txnDecoded.lease, txn.lease);
            Assert.AreEqual(txnDecoded.lease, Convert.FromBase64String(lease));
            Assert.AreEqual(txnDecoded, txn);
        }

        [Test]
        public void testTransactionWithLease()
        {

            string FROM_SK = "advice pudding treat near rule blouse same whisper inner electric quit surface sunny dismiss leader blood seat clown cost exist hospital century reform able sponsor";
            byte[] seed = Mnemonic.ToKey(FROM_SK);
            Account account = new Account(seed);

            Address fromAddr = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            Address toAddr = new Address("PNWOET7LLOWMBMLE4KOCELCX6X3D3Q4H2Q4QJASYIEOF7YIPPQBG3YQ5YI");
            Address closeTo = new Address("IDUTJEUIEVSMXTU4LGTJWZ2UE2E6TIODUKU6UW3FU3UKIQQ77RLUBBBFLA");
            string goldenstring = "gqNzaWfEQOMmFSIKsZvpW0txwzhmbgQjxv6IyN7BbV5sZ2aNgFbVcrWUnqPpQQxfPhV/wdu9jzEPUU1jAujYtcNCxJ7ONgejdHhujKNhbXTNA+ilY2xvc2XEIEDpNJKIJWTLzpxZpptnVCaJ6aHDoqnqW2Wm6KRCH/xXo2ZlZc0FLKJmds0wsqNnZW6sZGV2bmV0LXYzMy4womdoxCAmCyAJoJOohot5WHIvpeVG7eftF+TYXEx4r7BFJpDt0qJsds00mqJseMQgAQIDBAECAwQBAgMEAQIDBAECAwQBAgMEAQIDBAECAwSkbm90ZcQI6gAVR0Nsv5ajcmN2xCB7bOJP61uswLFk4pwiLFf19j3Dh9Q5BIJYQRxf4Q98AqNzbmTEIOfw+E0GgR358xyNh4sRVfRnHVGhhcIAkIZn9ElYcGihpHR5cGWjcGF5";

            ulong firstValidRound = 12466;
            ulong lastValidRound = 13466;
            ulong amountToSend = 1000;
            byte[] note = Convert.FromBase64String("6gAVR0Nsv5Y=");
            string genesisID = "devnet-v33.0";
            Digest genesisHash = new Digest("JgsgCaCTqIaLeVhyL6XlRu3n7Rfk2FxMeK+wRSaQ7dI=");
            byte[] lease = { 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4, 1, 2, 3, 4 };

            var tx = new Transaction(fromAddr, 4, firstValidRound, lastValidRound, note, amountToSend,
                toAddr, genesisID, genesisHash)
            {
                closeRemainderTo = closeTo,
                lease = lease
            };
            Account.SetFeeByFeePerByte(tx, 4);

            //    Transaction tx = Transaction.PaymentTransactionBuilder()
            //.sender(fromAddr)
            //.fee(4)
            //.firstValid(firstValidRound)
            //.lastValid(lastValidRound)
            //.note(note)
            //.genesisID(genesisID)
            //.genesisHash(genesisHash)
            //.amount(amountToSend)
            //.receiver(toAddr)
            //.closeRemainderTo(closeTo)
            //.lease(lease)
            //.build();
            byte[] outBytes = Encoder.EncodeToMsgPack(tx);
            Transaction o = Encoder.DecodeFromMsgPack<Transaction>(outBytes);
            Assert.AreEqual(o, tx);

            SignedTransaction stx = account.SignTransaction(tx);
            
            string encodedOutBytes = Convert.ToBase64String(Encoder.EncodeToMsgPack(stx));


            SignedTransaction testtx = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(goldenstring));

            var goldenStringRaw=Convert.FromBase64String(goldenstring);


            SignedTransaction stxDecoded = Encoder.DecodeFromMsgPack<SignedTransaction>(Convert.FromBase64String(encodedOutBytes));

            Assert.AreEqual(stxDecoded, stx);
            Assert.AreEqual( goldenstring, encodedOutBytes);
            TestUtil.SerializeDeserializeCheck(stx);
        }

        [Test]
        public void EmptyByteArraysShouldBeRejected()
        {
            Address fromAddr = new Address("47YPQTIGQEO7T4Y4RWDYWEKV6RTR2UNBQXBABEEGM72ESWDQNCQ52OPASU");
            Address toAddr = new Address("PNWOET7LLOWMBMLE4KOCELCX6X3D3Q4H2Q4QJASYIEOF7YIPPQBG3YQ5YI");

            var tx = new Transaction(fromAddr, 4, 1, 10, new byte[] { }, 1,
                toAddr, null, new Digest());
            //tx.closeRemainderTo = closeTo;
            tx.lease = new byte[] { };

            //Transaction tx = Transaction.PaymentTransactionBuilder()
            //        .sender(fromAddr)
            //        .fee(4)
            //        .firstValid(1)
            //        .lastValid(10)
            //        .amount(1)
            //        .genesisHash(new Digest())
            //        .receiver(toAddr)
            //        .note(new byte[] { })
            //        .lease(new byte[] { })
            //        .build();

            Assert.IsNull(tx.note);
            Assert.IsNull(tx.lease);
        }
    }
}
