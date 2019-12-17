using System;
using System.Text;

namespace Algorand
{
    /// <summary>
    /// Provides an easy way to create mnemonics from 32-byte length keys.
    /// </summary>
    public class Mnemonic
    {
        private const int BITS_PER_WORD = 11;
        private const int CHECKSUM_LEN_WORDS = 1;
        private const int KEY_LEN_BYTES = 32;
        private const int MNEM_LEN_WORDS = 25; // includes checksum word
        private const int PADDING_ZEROS = BITS_PER_WORD - ((KEY_LEN_BYTES * 8) % BITS_PER_WORD);
        private const char MNEMONIC_DELIM = ' ';

        // on set up, verify expected relationship between constants
        static Mnemonic()
        {
            if (MNEM_LEN_WORDS * BITS_PER_WORD - (CHECKSUM_LEN_WORDS * BITS_PER_WORD) != KEY_LEN_BYTES * 8 + PADDING_ZEROS)
            {
                throw new Exception("cannot initialize mnemonic library: invalid constants");
            }
        }
        /// <summary>
        /// Converts a 32-byte key into a 25 word mnemonic. The generated
        /// mnemonic includes a checksum. Each word in the mnemonic represents 11 bits
        /// of data, and the last 11 bits are reserved for the checksum.
        /// </summary>
        /// <param name="key">32 byte length key</param>
        /// <returns>the mnemonic</returns>
        public static string FromKey(byte[] key)
        {
            if (key == null || key.Length != KEY_LEN_BYTES)
                throw new ArgumentException("key must not be null and the key length must be " + KEY_LEN_BYTES + " bytes");
            string chkWord = Checksum(key);
            int[] uint11Arr = ToUintNArray(key);
            string[] words = ApplyWords(uint11Arr);
            return MnemonicToString(words, chkWord);
        }
        /// <summary>
        /// toKey converts a mnemonic generated using this library into the source
        /// key used to create it. It returns an error if the passed mnemonic has an
        /// incorrect checksum, if the number of words is unexpected, or if one
        /// of the passed words is not found in the words list.
        /// </summary>
        /// <param name="mnemonicStr">words delimited by MNEMONIC_DELIM</param>
        /// <returns>32 byte array key</returns>
        public static byte[] ToKey(string mnemonicStr)
        {
            if (mnemonicStr is null) throw new ArgumentException("mnemonic must not be null");
            //Objects.requireNonNull(mnemonicStr, "mnemonic must not be null");
            string[] mnemonic = mnemonicStr.Split(MNEMONIC_DELIM);
            if (mnemonic.Length != MNEM_LEN_WORDS)
            {
                throw new ArgumentException("mnemonic does not have enough words");
            }
            // convert to uint11
            int numWords = MNEM_LEN_WORDS - CHECKSUM_LEN_WORDS;
            int[] uint11Arr = new int[numWords];
            for (int i = 0; i < numWords; i++)
            {
                uint11Arr[i] = -1;
            }
            for (int w = 0; w < Wordlist.RAW.Length; w++)
            {
                for (int i = 0; i < numWords; i++)
                {
                    if (Wordlist.RAW[w].Equals(mnemonic[i]))
                    {
                        uint11Arr[i] = w;
                    }
                }
            }
            for (int i = 0; i < numWords; i++)
            {
                if (uint11Arr[i] == -1)
                {
                    throw new ArgumentException("mnemonic contains word that is not in word list");
                }
            }
            byte[] b = ToByteArray(uint11Arr);
            // chop the last byte. The last byte was 3 bits, padded with 8 bits to create the 24th word.
            // Those last padded 8 bits is an extra zero byte.
            if (b.Length != KEY_LEN_BYTES + 1)
            {
                throw new ArgumentException("wrong key length");
            }
            if (b[KEY_LEN_BYTES] != (byte)0)
            {
                throw new ArgumentException("unexpected byte from key");
            }
            //byte[] bCopy = new byte[KEY_LEN_BYTES];
            byte[] bCopy = JavaHelper<byte>.ArrayCopyOf(b, KEY_LEN_BYTES);
            string chkWord = Checksum(bCopy);
            if (!chkWord.Equals(mnemonic[MNEM_LEN_WORDS - CHECKSUM_LEN_WORDS]))
            {
                throw new ArgumentException("checksum failed to validate");
            }
            return JavaHelper<byte>.ArrayCopyOf(b, KEY_LEN_BYTES);
        }

        // returns a word corresponding to the 11 bit checksum of the data
        internal static string Checksum(byte[] data)
        {
            //CryptoProvider.setupIfNeeded();
            //MessageDigest digest = MessageDigest.getInstance(CHECKSUM_ALG);
            //digest.update(Arrays.copyOf(data, data.length));
            //byte[] d = digest.digest();
            byte[] d = Digester.Digest(data);
            // optimize for CHECKSUM_LEN_WORDS = 1
            //d = Arrays.copyOfRange(d, 0, 2);
            return ApplyWord(ToUintNArray(new byte[] { d[0], d[1] })[0]);
        }

        // Assumes little-endian
        private static int[] ToUintNArray(byte[] arr)
        {
            int buffer = 0;
            int numBits = 0;
            int[] ret = new int[(arr.Length * 8 + BITS_PER_WORD - 1) / BITS_PER_WORD];

            int j = 0;

            for (int i = 0; i < arr.Length; i++)
            {
                // numBits is how many bits in arr[i] we've processed
                int v = arr[i];
                if (v < 0) v += 256; // deal with java signed types
                buffer |= (v << numBits);
                numBits += 8;
                if (numBits >= BITS_PER_WORD)
                {
                    // add to output
                    ret[j] = buffer & 0x7ff;
                    j++;
                    // drop from buffer
                    buffer = buffer >> BITS_PER_WORD;
                    numBits -= BITS_PER_WORD;
                }
            }
            if (numBits != 0)
            {
                ret[j] = buffer & 0x7ff;
            }
            return ret;
        }

        // reverses toUintNArray, might result in an extra byte
        // be careful since int is a signed type. But 11 < 32/2 so should be ok.
        private static byte[] ToByteArray(int[] arr)
        {
            int buffer = 0;
            int numBits = 0;
            byte[] ret = new byte[(arr.Length * BITS_PER_WORD + 8 - 1) / 8];

            int j = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                buffer |= (arr[i] << numBits);
                numBits += BITS_PER_WORD;
                while (numBits >= 8)
                {
                    ret[j] = (byte)(buffer & 0xff);
                    j++;
                    buffer = buffer >> 8;
                    numBits -= 8;
                }
            }
            if (numBits != 0)
            {
                ret[j] = (byte)(buffer & 0xff);
            }
            return ret;
        }

        private static string ApplyWord(int iN)
        {
            return Wordlist.RAW[iN];
        }

        private static string[] ApplyWords(int[] arrN)
        {
            string[] ret = new string[arrN.Length];
            for (int i = 0; i < arrN.Length; i++)
            {
                ret[i] = ApplyWord(arrN[i]);
            }
            return ret;
        }

        private static string MnemonicToString(string[] mnemonic, string checksum)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < mnemonic.Length; i++)
            {
                if (i > 0) s.Append(MNEMONIC_DELIM);
                s.Append(mnemonic[i]);
            }
            s.Append(MNEMONIC_DELIM);
            s.Append(checksum);
            return s.ToString();
        }
    }
}
