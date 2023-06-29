namespace Shader {

    public static class BitReversal {

        /// <summary>
        /// Returns a sequence from 0 to n
        /// </summary>
        public static uint[] Sequence(int n) {
            var res = new uint[n];
            for (uint i = 0; i < n; i++)
                res[i] = i;
            return res;
        }

        /// <summary>
        /// Reverses the bits of all xs
        /// </summary>
        public static uint[] Reverse(uint[] xs) {
            return Reverse(xs, Digits(xs.Length));
        }
        
        /// <summary>
        /// Reverses the bits of all xs, with digits n
        /// </summary>
        public static uint[] Reverse(uint[] xs, int n) {
            var len = xs.Length;
            var res = new uint[len];
            for (var i = 0; i < len; i++)
                res[i] = Reverse(xs[i], n);
            return res;
        }
        
        /// <summary>
        /// Reverses the bits of x, with n digits
        /// </summary>
        public static uint Reverse(uint x, int n) {
            uint res = 0;
            for (int i = 0; i < n; i++)
                res |= (uint)( ((x >> (n - 1 - i)) & 1u) << i );
            return res;
        }

        /// <summary>
        /// Returns the number of base2 digits of len-1
        /// </summary>
        public static int Digits(int len) {
            var digits = 1;
            len -= 1;
            while ((len >>= 1) != 0)
                digits++;
            return digits;
        }
    }
}