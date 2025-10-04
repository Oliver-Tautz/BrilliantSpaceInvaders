// Assets/Scripts/Utils/MathUtils.cs
namespace YourGame.Utils
{
    public static class MathUtils
    {
        /// <summary>
        /// Sum of all integers from x to y inclusive.
        /// Handles x > y by swapping. Uses long internally to reduce overflow risk.
        /// </summary>
        public static long SumRange(long x, long y)
        {
            if (x > y) (x, y) = (y, x);

            // n = count of terms
            long n = y - x + 1;

            // Use (x + y) * n / 2 but do it in long to avoid int overflow.
            // For odd n, divide first to keep intermediate smaller.
            if ((n & 1) == 0)        // n even
                return (n / 2) * (x + y);
            else                     // n odd
                return n * ((x + y) / 2); // (x+y) will be even if n is odd when x,y are integers in an inclusive range
        }

        /// <summary>
        /// int overload (casts to long for safety, then clamps back if needed).
        /// </summary>
        public static int SumRange(int x, int y, bool checkedClamp = false)
        {
            long r = SumRange((long)x, (long)y);
            if (checkedClamp)
            {
                if (r > int.MaxValue) return int.MaxValue;
                if (r < int.MinValue) return int.MinValue;
            }
            return (int)r; // may overflow if very large and not clamped
        }
    }
}
// End of Assets/Scripts/Utils/MathUtils.cs
