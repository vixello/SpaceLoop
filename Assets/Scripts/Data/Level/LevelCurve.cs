using UnityEngine;

namespace Assets.Scripts.Data.Level
{
    [CreateAssetMenu(menuName = "Game/Level Curve")]
    public class LevelCurve : ScriptableObject
    {
        [Header("XP Formula Parameters")]
        public int baseXp = 50;          // XP for level 1
        public int growthPerLevel = 40;  // Linear growth
        public int exponentNum = 3;      // Exponent numerator
        public int exponentDen = 2;      // Exponent denominator (3/2 = 1.5)

        [Header("Max Level")]
        public int maxLevel = 100;

        private int[] xpTable;

        private void OnEnable()
        {
            GenerateTable();
        }

        private void GenerateTable()
        {
            xpTable = new int[maxLevel + 1];

            for (int level = 1; level <= maxLevel; level++)
            {
                // Integer exponent using rational exponent (num/den)
                long pow = IntPow(level, exponentNum, exponentDen);

                xpTable[level] = baseXp + (int)(pow * growthPerLevel);
            }
        }

        // Integer power with rational exponent (num/den)
        private long IntPow(int value, int num, int den)
        {
            // Compute value^(num/den) using integer math only
            // value^num first
            long powered = 1;
            for (int i = 0; i < num; i++)
                powered *= value;

            // integer root (den-th root)
            return IntRoot(powered, den);
        }

        // Integer nth root (floor) -> find x^n ≈ value
        private long IntRoot(long value, int n)
        {
            long low = 1;
            long high = value;

            while (low <= high)
            {
                long mid = (low + high) / 2;
                long midToPowerN = 1;

                for (int i = 0; i < n; i++)
                    midToPowerN *= mid;

                if (midToPowerN == value)
                    return mid;

                if (midToPowerN < value)
                    low = mid + 1;
                else
                    high = mid - 1;
            }

            return high; // floor root
        }

        public int GetXpForLevel(int level)
        {
            level = Mathf.Clamp(level, 1, maxLevel);
            return xpTable[level];
        }
    }

}
