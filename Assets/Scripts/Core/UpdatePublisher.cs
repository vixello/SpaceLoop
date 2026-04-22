using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public interface IUpdateObserver
    {
        void ObservedUpdate();
    }

    public class UpdatePublisher : MonoBehaviour
    {
        private static List<IUpdateObserver> _observers = new List<IUpdateObserver>();
        private static List<IUpdateObserver> _pendingObservers = new List<IUpdateObserver>();
        private static List<IUpdateObserver> _toRemove = new List<IUpdateObserver>();
        private static int _currentIndex;

        private void Update()
        {
            var snapshot = _observers.ToArray();

            foreach (var observer in snapshot)
            {
                observer?.ObservedUpdate();
            }

            // Apply removals safely
            foreach (var obs in _toRemove)
            {
                _observers.Remove(obs);
            }

            _toRemove.Clear();
            _observers.AddRange(_pendingObservers);
            _pendingObservers.Clear();
        }

        public void RegisterObserver(IUpdateObserver observer)
        {
            if (!_pendingObservers.Contains(observer) && !_observers.Contains(observer))
                _pendingObservers.Add(observer);
        }

        public void UnregisterObserver(IUpdateObserver observer)
        {
            _toRemove.Add(observer);
        }
    }
}
