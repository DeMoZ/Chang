using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DMZ.Events;
using UnityEngine;

namespace Chang
{
    public class PagesSoundController : IDisposable
    {
        private readonly AudioSource _audioSource;
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

        private void StopSound()
        {
            _cancellationTokenSource?.Cancel();
            _audioSource.Stop();
            
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