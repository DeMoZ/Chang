using System;
using System.Collections.Generic;
using DMZ.Events;
using UnityEngine;

namespace Chang
{
    public class PagesSoundController : IDisposable
    {
        private AudioSource _audioSource;
        private Dictionary<string, DMZState<bool>> _listeners = new();
        
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
            _audioSource.PlayOneShot(audioClip);
            _audioSource.UnPause();
            
            _listeners[audioClip.name].Value = true;
        }

        public void StopSound(AudioClip audioClip)
        {
            _audioSource?.Stop();
            _listeners[audioClip.name].Value = false;
        }

        public void UnregisterListeners()
        {
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
    }
}