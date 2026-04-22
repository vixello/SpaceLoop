using Assets.Scripts.Core.Interfaces;
using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Assets.Scripts.Gameplay.Music
{

    [DefaultExecutionOrder(-1)]

        public class BeatManager : MonoBehaviour, IInitializable
        {
            #region AUDIO SOURCES AND AUDIO DATA
            [SerializeField] private AudioSource _audioSource;
            public AudioSource AudioSource => _audioSource;

            private float _bpm = 90f;
            public float CurrentAudioTime => _audioSource.time;
            #endregion

            #region AUDIO INTERVALS
            [SerializeField] private AudioIntervals[] _intervals;
            [SerializeField] private float frequencyEnergyThreshold = 0.1f;  // Threshold for detecting high-pitched notes
            private const float _analyzeInterval = 0.02f;
            #endregion

            #region FREQUENCY JOB
            private bool _jobScheduled = false;
            private float _jobAnalyzeTimer = 0f;
            private float[] _jobTempSpectrumBuffer = new float[1024];

            private NativeArray<float> _jobSpectrumData;
            private NativeArray<float> _jobEnergyResult;

            private FrequencyAnalysisJob _frequencyAnalysisJob;
            private JobHandle _jobHandle;
            #endregion

            #region INITIALIZATION
            private bool _isInitialized = false;
            public bool IsInitialized() => _isInitialized;
            #endregion

 
            private void Start()
            {
                //await Initialize();
                //EventBus.Instance.OnPlayLevelAudio += PlayLevelAudio;
            }

            public async UniTask Initialize()
            {
                _jobSpectrumData = new NativeArray<float>(1024, Allocator.Persistent);
                _jobEnergyResult = new NativeArray<float>(4, Allocator.Persistent); // low, mid, high, total

                foreach (AudioIntervals interval in _intervals)
                {
                    interval.onTrigger += () => TriggerBeat(interval.steps);
                }

                await LoadClipAsync(_audioSource.clip);
                _isInitialized = true;
                this.enabled = true;
            }

            void OnDestroy()
            {
                if (_jobScheduled)
                {
                    _jobHandle.Complete();
                    _jobScheduled = false;
                }

                if (_jobSpectrumData.IsCreated)
                {
                    _jobSpectrumData.Dispose();
                }

                if (_jobEnergyResult.IsCreated)
                {
                    _jobEnergyResult.Dispose();
                }
                //EventBus.Instance.OnPlayLevelAudio -= PlayLevelAudio;
            }

            void Update()
            {
                AnalyzeAudioIntervals();

                _jobAnalyzeTimer += Time.deltaTime;
                if (_jobScheduled && _jobHandle.IsCompleted)
                {
                    _jobHandle.Complete();         // Finalize async job
                    AnalyzeJobResult();            // Safe to use job result now
                    _jobScheduled = false;
                }

                if (!_jobScheduled && _jobAnalyzeTimer >= _analyzeInterval)  // If no job is scheduled, start a new one
                {
                    AnalyzeHighNotes();
                }
                //NoteManagerSO.Instance.LightOnMidiNote(_audioSource);
            }

            private void AnalyzeAudioIntervals()
            {
                foreach (AudioIntervals interval in _intervals)
                {
                    float sampledTime = (_audioSource.timeSamples / (_audioSource.clip.frequency * interval.GetBeatLenght(_bpm)));
                    interval.CheckForNewInterval(sampledTime);
                }
            }

            private void AnalyzeHighNotes()
            {
                if (_jobAnalyzeTimer >= _analyzeInterval)
                {
                    _audioSource.GetSpectrumData(_jobTempSpectrumBuffer, 0, FFTWindow.BlackmanHarris);
                    _jobSpectrumData.CopyFrom(_jobTempSpectrumBuffer);

                    _frequencyAnalysisJob = new FrequencyAnalysisJob
                    {
                        spectrum = _jobSpectrumData,
                        energyResult = _jobEnergyResult
                    };

                    _jobHandle = _frequencyAnalysisJob.Schedule();
                    _jobScheduled = true;
                    _jobAnalyzeTimer = 0f;
                }
            }

            private void AnalyzeJobResult()
            {
                float low = _jobEnergyResult[0];
                float mid = _jobEnergyResult[1];
                float high = _jobEnergyResult[2];
                float total = _jobEnergyResult[3];

                TriggerResizeEvent(total > frequencyEnergyThreshold ? total : 0f); // If the total energy exceeds the threshold, trigger the resize event

                //EventBus.Instance.InvokeResizeObjectWithPitch(PitchType.Low, low);
                //EventBus.Instance.InvokeResizeObjectWithPitch(PitchType.Medium, mid);
                //EventBus.Instance.InvokeResizeObjectWithPitch(PitchType.High, high);
            }

            private void TriggerResizeEvent(float pitch)
            {
                //EventBus.Instance.InvokeResizeObjectWithPitch(PitchType.Total, pitch);
            }

            private void TriggerBeat(float beatStep)
            {
               // EventBus.Instance.InvokeBeatTriggered(beatStep);
            }

            private async Task LoadClipAsync(AudioClip clip, Action<AudioClip> onLoaded = null)
            {
                if (clip == null)
                {
                    Debug.LogWarning("LoadClip called with null clip.");
                    return;
                }

                if (!clip.loadInBackground)
                    clip.LoadAudioData();

                while (clip.loadState == AudioDataLoadState.Loading)
                    await Task.Yield();

                if (clip.loadState != AudioDataLoadState.Loaded)
                {
                    Debug.LogError($"Failed to load clip: {clip.name}");
                    return;
                }

                onLoaded?.Invoke(clip);
            }

            public void ChangeAudioClip(AudioClip newClip)
            {

                if (_audioSource != null && newClip != null)
                {
                    float currentTime = _audioSource.time;
                    _audioSource.clip = newClip; // Change the AudioSource'_distanceToPlayer clip to the new clip
                    _audioSource.time = currentTime;

                    _audioSource.Play();
                }
            }

            public float GetTimeToNextBeat()
            {
                if (_intervals == null || _intervals.Length == 0)
                {
                    Debug.LogWarning("No intervals set in BeatManager.");
                    return 0f;
                }

                float beatLength = _intervals[1].GetBeatLenght(_bpm);
                float currentTime = _audioSource.time;
                float timeIntoBeat = currentTime % beatLength;

                return beatLength - timeIntoBeat;
            }

            public float GetBeatDuration()
            {
                return _intervals[1].GetBeatLenght(_bpm);
            }

            private void PlayLevelAudio()
            {
                _audioSource.Play();
            }
        }


}
