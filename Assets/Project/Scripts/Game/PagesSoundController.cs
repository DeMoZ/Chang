using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Events;
using UnityEngine;
using Debug = DMZ.DebugSystem.DMZLogger;

namespace Chang
{
    public class PagesSoundController : IDisposable
    {
        private const double ScheduleLeadTime = 0.1; // time to schedule the first clip of a sequence

        private readonly AudioSource _audioSource;
        private AudioSource _sequenceAudioSource; // second source, sequence clips alternate between sources to play gapless
        private readonly Dictionary<string, DMZState<bool>> _listeners = new();
        
        private CancellationTokenSource _cancellationTokenSource;
        
        public PagesSoundController(AudioSource audioSource)
        {
            _audioSource = audioSource;
        }
        
        public void Dispose()
        {
            Clear();
        }
        
        public void RegisterListener(string clipName, Action<bool> listener)
        {
            Debug.Log("RegisterListener sound: " + clipName);
            
            if (!_listeners.ContainsKey(clipName))
            {
                _listeners[clipName] = new DMZState<bool>();    
            }
            
            _listeners[clipName].Subscribe(listener);
        }

        public void UnregisterListener(string clipName, Action<bool> listener)
        {
            if (!_listeners.TryGetValue(clipName, out var state))
                return;

            state.Unsubscribe(listener);
        }

        public void UnregisterListener(string clipName)
        {
            if (!_listeners.TryGetValue(clipName, out var state))
                return;

            state.Dispose();
            _listeners.Remove(clipName);
        }

        public void PlaySound(AudioClip audioClip)
        {
            Debug.Log("PlaySound: " + audioClip.name);
           
            if (_audioSource.isPlaying)
            {
                if (_audioSource.clip == audioClip)
                {
                    StopSound();
                    return;
                }
                
                StopSound();
            }
            
            _audioSource.clip = audioClip;
            _audioSource.Play();

            if (_listeners.TryGetValue(_audioSource.clip.name, out var state))
            {
                state.Value = true;
            }
            
            _cancellationTokenSource = new CancellationTokenSource();
            MonitorAudioCompletion(audioClip, _cancellationTokenSource.Token).Forget();
        }
        
        // plays clips one after another without gaps, clips are scheduled on the audio (dsp) timeline
        public async UniTask PlaySoundsAsync(List<AudioClip> audioClips, CancellationToken token)
        {
            if (audioClips == null || audioClips.Count == 0)
            {
                return;
            }

            if (token.IsCancellationRequested)
            {
                return;
            }

            StopSound();

            try
            {
                await LoadAudioDataAsync(audioClips, token);

                using var cancellationRegistration = token.Register(StopSound);

                AudioSource[] sources = { _audioSource, GetSequenceAudioSource() };
                double[] endTimes = new double[audioClips.Count];
                double time = AudioSettings.dspTime + ScheduleLeadTime;

                for (int i = 0; i < audioClips.Count; i++)
                {
                    if (i >= 2)
                    {
                        // the source is free when its previous clip ends, there is still a whole clip (i - 1) ahead
                        double sourceFreeTime = endTimes[i - 2];
                        await UniTask.WaitUntil(() => AudioSettings.dspTime >= sourceFreeTime, cancellationToken: token);
                    }

                    AudioSource source = sources[i % 2];
                    source.clip = audioClips[i];
                    source.PlayScheduled(time);

                    time += (double)audioClips[i].samples / audioClips[i].frequency;
                    endTimes[i] = time;
                }

                double sequenceEndTime = time;
                await UniTask.WaitUntil(() => AudioSettings.dspTime >= sequenceEndTime, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
            }
        }

        // audio data of clips is loaded lazily on the first play, load it in advance to avoid a delay
        public void PreloadAudioData(IEnumerable<AudioClip> audioClips)
        {
            foreach (AudioClip clip in audioClips)
            {
                if (clip && clip.loadState == AudioDataLoadState.Unloaded)
                {
                    clip.LoadAudioData();
                }
            }
        }

        private async UniTask LoadAudioDataAsync(List<AudioClip> audioClips, CancellationToken token)
        {
            PreloadAudioData(audioClips);
            await UniTask.WaitUntil(() => audioClips.All(clip => clip.loadState != AudioDataLoadState.Loading),
                cancellationToken: token);
        }

        private AudioSource GetSequenceAudioSource()
        {
            if (_sequenceAudioSource)
            {
                return _sequenceAudioSource;
            }

            _sequenceAudioSource = _audioSource.gameObject.AddComponent<AudioSource>();
            _sequenceAudioSource.playOnAwake = false;
            _sequenceAudioSource.outputAudioMixerGroup = _audioSource.outputAudioMixerGroup;
            _sequenceAudioSource.volume = _audioSource.volume;
            _sequenceAudioSource.pitch = _audioSource.pitch;
            _sequenceAudioSource.spatialBlend = _audioSource.spatialBlend;
            _sequenceAudioSource.priority = _audioSource.priority;
            return _sequenceAudioSource;
        }

        private void StopSound()
        {
            _cancellationTokenSource?.Cancel();
            _audioSource.Stop();

            if (_sequenceAudioSource)
            {
                _sequenceAudioSource.Stop();
            }
            
            if (_audioSource.clip != null && _listeners.TryGetValue(_audioSource.clip.name, out var state))
            {
                state.Value = false;
            }
        }

        public void UnregisterListeners()
        {
            _cancellationTokenSource?.Cancel();
            Clear();
        }

        private void Clear()
        {
            foreach (var listener in _listeners)
            {
                listener.Value.Dispose();
            }
            
            _listeners.Clear();
        }
        
        private async UniTask MonitorAudioCompletion(AudioClip clip, CancellationToken token)
        {
            try
            {
                while (_audioSource.isPlaying && !token.IsCancellationRequested)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                if (_audioSource.clip == clip && _listeners.TryGetValue(clip.name, out var state))
                {
                    state.Value = false;
                }
                
                _audioSource.clip = null;
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}