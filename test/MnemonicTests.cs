using Algorand;
using NUnit.Framework;
using System;

namespace test
{
    [TestFixture]
    public class MnemonicTests
    {
        [Test]
        public void testZeroVector()
        {
            byte[] zeroKeys = new byte[32];
            string expectedMn = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon invest";
            string mn = Mnemonic.FromKey(zeroKeys);

            Assert.AreEqual(mn, expectedMn);
            byte[] goBack = Mnemonic.ToKey(mn);
            Assert.AreEqual(goBack, zeroKeys);
        }

        [Test]
        public void testWordNotInList()
        {
            string mn = "abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon abandon zzz invest";

            var ex = Assert.Throws<ArgumentException>(() => { Mnemonic.ToKey(mn); });
            Assert.AreEqual("mnemonic contains word that is not in word list", ex.Message);
        }

        [Test]
        public void testGenerateAndRecovery()
        {
            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                byte[] randKey = new byte[32];
                r.NextBytes(randKey);
                string mn = Mnemonic.FromKey(randKey);
                byte[] regenKey = Mnemonic.ToKey(mn);
                Assert.AreEqual(regenKey, randKey);
            }
        }

        [Test]
        public void testCorruptedChecksum()
        {
            Random r = new Random();
            for (int i = 0; i < 1000; i++)
            {
                byte[] randKey = new byte[32];
                r.NextBytes(randKey);
                string mn = Mnemonic.FromKey(randKey);
                string[] words = mn.Split(" ");
                string oldWord = words[words.Length - 1];
                string newWord = oldWord;
                while (oldWord.Equals(newWord))
                {
                    newWord = Wordlist.RAW[r.Next(2 ^ 11)];
                }
                words[words.Length - 1] = newWord;

                string corruptedMn = "";

                for (int j = 0; j < words.Length; j++)
                {
                    if (j > 0) corruptedMn += " ";
                    corruptedMn += words[j];
                }
                var ex = Assert.Throws<ArgumentException>(() => { Mnemonic.ToKey(corruptedMn); });
                Assert.AreEqual("checksum failed to validate", ex.Message);
                //    Assert.AreEqualThrownBy(()->Mnemonic.toKey())
                //.isInstanceOf(GeneralSecurityException.class)
                //        .hasMessage();
            }
        }

        [Test]
        public void testInvalidKeylen()
        {
            Random r = new Random();
            int[] badLengths = new int[] { 0, 31, 33, 100, 35, 2, 30 };
            foreach (int badlen in badLengths)
            {
                byte[] randKey = new byte[badlen];
                r.NextBytes(randKey);
                var ex = Assert.Throws<ArgumentException>(() => { Mnemonic.FromKey(randKey); });
                Assert.AreEqual("key must not be null and the key length must be 32 bytes", ex.Message);
            }
        }
    }
}
