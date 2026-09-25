using System;
using UnityEngine;

namespace GunSprint.Events
{
    /// <summary>
    /// Base class for ScriptableObject event channels (observer pattern).
    /// Publishers call <see cref="RaiseEvent"/>, listeners subscribe to <see cref="OnEventRaised"/>.
    /// Because the channel is a project asset, prefabs and scene objects can talk to each other
    /// without direct references or singletons.
    /// </summary>
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        public event Action<T> OnEventRaised;

        public void RaiseEvent(T value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
