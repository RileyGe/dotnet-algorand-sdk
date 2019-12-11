namespace Algorand
{
    public class VRFPublicKey
    {
        private byte[] vrfPK;

        public VRFPublicKey()
        {
        }

        public VRFPublicKey(byte[] vrfPK)
        {
            this.vrfPK = vrfPK;
        }
    }
}