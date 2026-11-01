public class Solution
{
    public int MinEatingSpeed(int[] piles, int hours)
    {
        ArgumentNullException.ThrowIfNull(piles);
        if (piles.Length == 0 || hours < piles.Length)
            throw new ArgumentException("At least one pile and enough hours are required.");

        var low = 1;
        var high = piles.Max();
        while (low < high)
        {
            var speed = low + (high - low) / 2;
            long needed = 0;
            foreach (var pile in piles)
                needed += ((long)pile + speed - 1) / speed;

            if (needed <= hours)
                high = speed;
            else
                low = speed + 1;
        }

        return low;
    }
}


