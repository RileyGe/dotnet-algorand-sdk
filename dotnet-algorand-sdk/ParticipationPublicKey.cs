namespace Algorand
{
    public class ParticipationPublicKey
    {
        private byte[] votePK;

        public ParticipationPublicKey()
        {
        }

        public ParticipationPublicKey(byte[] votePK)
        {
            this.votePK = votePK;
        }
    }
}