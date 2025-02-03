using System;
using System.Collections.Generic;

namespace DMZ.Events
{
    public class DMZState<T> : IDisposable
    {
        private readonly List<Action<T>> _subscribers = new();
        protected readonly object _lock = new();
        protected T _data;
        protected T _previousData;

        public DMZState(T state = default)
        {
            _previousData = state;
            _data = state;
        }

        public void Subscribe(Action<T> callback)
        {
            lock (_lock)
            {
                _subscribers.Add(callback);
            }
        }

        public void Unsubscribe(Action<T> callback)
        {
            lock (_lock)
            {
                _subscribers.Remove(callback);
            }
        }

        public T PreviousValue => _previousData;

        public T Value
        {
            get => _data;
            set
            {
                lock (_lock)
                {
                    if (Equals(value, _data))
                        return;

                    _previousData = _data;
                    _data = value;
                }

                NotifySubscribers();
            }
        }

        /// <summary>
        /// set value and do not notify even if state not changed
        /// </summary>
        /// <param name="value"></param>
        public void SetSilent(T value)
        {
            lock (_lock)
            {
                _previousData = _data;
                _data = value;
            }
        }
        
        /// <summary>
        /// set value and force notify even if state has not changed
        /// </summary>
        /// <param name="value"></param>
        public void SetAndForceNotify(T value)
        {
            lock (_lock)
            {
                _previousData = _data;
                _data = value;
            }

            NotifySubscribers();
        }

        /// <summary>
        /// set value to both data and previous data
        /// </summary>
        /// <param name="value"></param>
        public void ResetValue(T value)
        {
            lock (_lock)
            {
                _previousData = value;
                _data = value;
            }

            NotifySubscribers();
        }

        /// <summary>
        /// set value to both data and previous data and force notify even if state has not changed
        /// </summary>
        /// <param name="value"></param>
        public void ResetAndForceNotify(T value)
        {
            lock (_lock)
            {
                _previousData = value;
                _data = value;
            }

            NotifySubscribers();
        }

        protected virtual void NotifySubscribers()
        {
            List<Action<T>> subscribersCopy;

            lock (_lock)
            {
                subscribersCopy = new List<Action<T>>(_subscribers);
            }

            foreach (var subscriber in subscribersCopy)
            {
                subscriber?.Invoke(_data);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }
    }

    public class DMZState<T1, T2> : DMZState<T1>
    {
        private readonly List<Action<T1, T2>> _dualSubscribers = new();
        protected T2 _data2;
        protected T2 _previousData2;

        public DMZState(T1 state1 = default, T2 state2 = default) : base(state1)
        {
            _previousData2 = state2;
            _data2 = state2;
        }

        public void Subscribe(Action<T1, T2> callback)
        {
            lock (_lock)
            {
                _dualSubscribers.Add(callback);
            }
        }

        public void Unsubscribe(Action<T1, T2> callback)
        {
            lock (_lock)
            {
                _dualSubscribers.Remove(callback);
            }
        }

        public (T1, T2) PreviousValues => (PreviousValue, _previousData2);

        public (T1, T2) Values
        {
            get => (Value, _data2);
            set
            {
                lock (_lock)
                {
                    if (Equals(value.Item1, _data) && Equals(value.Item2, _data2))
                        return;

                    _previousData = _data;
                    _data = value.Item1;
                    _previousData2 = _data2;
                    _data2 = value.Item2;
                }

                NotifySubscribers();
            }
        }

        public void SetAndForceNotify(T1 value1, T2 value2)
        {
            lock (_lock)
            {
                _previousData = _data;
                _data = value1;
                _previousData2 = _data2;
                _data2 = value2;
            }

            NotifySubscribers();
        }

        public void ResetValue(T1 value1, T2 value2)
        {
            lock (_lock)
            {
                _previousData = value1;
                _previousData2 = value2;
            }
        }

        protected override void NotifySubscribers()
        {
            base.NotifySubscribers();
            List<Action<T1, T2>> dualSubscribersCopy;

            lock (_lock)
            {
                dualSubscribersCopy = new List<Action<T1, T2>>(_dualSubscribers);
            }

            foreach (var subscriber in dualSubscribersCopy)
            {
                subscriber?.Invoke(_data, _data2);
            }
        }
    }
}