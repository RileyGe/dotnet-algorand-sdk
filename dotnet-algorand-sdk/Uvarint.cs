namespace Algorand
{
    class Uvarint
    {
        Uvarint()
        {
        }

        public static VarintResult parse(byte[] data)
        {
            int x = 0;
            int s = 0;

            for (int i = 0; i < data.Length; ++i)
            {
                int b = data[i] & 255;
                if (b < 128)
                {
                    if (i <= 9 && (i != 9 || b <= 1))
                    {
                        return new VarintResult(x | (b & 255) << s, i + 1);
                    }

                    return new VarintResult(0, -(i + 1));
                }

                x |= (b & 127 & 255) << s;
                s += 7;
            }

            return new VarintResult();
        }
    }
}