using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
    [BurstCompile]
    public struct GenerateRandomHeightJob : IJob
    {
        [ReadOnly] public float previousHeight;
        public float min;
        public float max;
        public float minDistanceInBetween;
        public float time;

        public NativeArray<float> result;
        private Unity.Mathematics.Random random;

        public void Execute()
        {
            random = new Unity.Mathematics.Random((uint)(time * 10000) + 1);
            result[0] = GenerateRandomHeight(previousHeight, min, max, minDistanceInBetween, time);
        }

        private float GenerateRandomHeight(float previousHeight, float min, float max, float minDistanceInBetween, float time)
        {
            float randomHeight = math.lerp(min, max, random.NextFloat(0f, 1f));

            int attempts = 0;
            do
            {
                randomHeight = math.lerp(min, max, random.NextFloat(0f, 1f));
                attempts++;
                if (attempts > 100)
                {
                    Debug.Log("Too many job attempts");
                    break;
                }
            }
            while (math.abs(randomHeight - previousHeight) < minDistanceInBetween);

            return randomHeight;
        }
    }



    [BurstCompile]
    public struct FrequencyAnalysisJob : IJob
    {
        [ReadOnly] public NativeArray<float> spectrum;
        public NativeArray<float> energyResult;

        public void Execute()
        {
            float low = 0f, mid = 0f, high = 0f;

            for (int i = 0; i < 100; i++) low += spectrum[i];
            for (int i = 100; i < 400; i++) mid += spectrum[i];
            for (int i = 400; i < 1024; i++) high += spectrum[i];

            energyResult[0] = low;
            energyResult[1] = mid;
            energyResult[2] = high;
            energyResult[3] = low + mid + high;
        }
    }

}
