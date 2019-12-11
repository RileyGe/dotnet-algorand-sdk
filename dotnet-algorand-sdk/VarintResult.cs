namespace Algorand
{
    class VarintResult
    {
        public int value;
        public int length;

        public VarintResult(int value, int length)
        {
            this.value = value;
            this.length = length;
        }

        public VarintResult()
        {
            this.value = 0;
            this.length = 0;
        }
    }
}