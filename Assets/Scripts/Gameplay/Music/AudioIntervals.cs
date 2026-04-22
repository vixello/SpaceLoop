using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Music
{
    [System.Serializable]
    public class AudioIntervals
    {
        [SerializeField] public float steps;
        private int _lastInterval;

        public event Action onTrigger;

        public float GetBeatLenght(float bpm)
        {
            return 60f / (bpm * steps);
        }

        public void CheckForNewInterval(float interval)
        {
            if (Mathf.FloorToInt(interval) != _lastInterval)
            {
                _lastInterval = Mathf.FloorToInt(interval);
                onTrigger.Invoke();
            }
        }

        public IEnumerator RunIntervalRoutine(float bpm, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);

            float beatLength = GetBeatLenght(bpm);
            float start = Time.realtimeSinceStartup;

            while (true)
            {
                if (Time.realtimeSinceStartup - start > 2f)
                {
                    Debug.LogError("⚠️ Infinite loop detected, exiting...");
                    break;
                }
                yield return new WaitForSeconds(beatLength);
                onTrigger?.Invoke();
            }

        }
    }
}
