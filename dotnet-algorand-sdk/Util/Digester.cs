using Org.BouncyCastle.Crypto.Digests;

namespace Algorand
{
    internal class Digester
    {
        internal static byte[] Digest(byte[] data)
        {
            Sha512tDigest digest = new Sha512tDigest(256);
            digest.BlockUpdate(data, 0, data.Length);
            byte[] output = new byte[32];
            digest.DoFinal(output, 0);
            return output;
        }
    }
}